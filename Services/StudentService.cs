using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.DTOs;
using TmsApi.Entities;

namespace TmsApi.Services;

public interface IStudentService
{
    Task<StudentRecord> RegisterAsync(string name, decimal GPA);
    Task<StudentRecord?> GetByIdAsync(int id, bool includeDeleted = false);
    Task<IReadOnlyList<StudentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
    Task<PagedResult<StudentRecord>> GetPagedStudentsAsync(int pageNumber, int pageSize);
    Task<StudentRecord?> UpdateAsync(int id, UpdateStudentRequest request);
    Task<bool> SoftDeleteAsync(int id);
    Task<int> BulkArchiveEnrollmentsAsync(int yearThreshold);
}

public class StudentService : IStudentService
{
    private readonly ILogger<StudentService> _logger;
    private readonly TmsDbContext _context;

    public StudentService(ILogger<StudentService> logger, TmsDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // Helper method to map a Student entity to a StudentRecord DTO
    private StudentRecord MapToStudentRecord(Student student)
    {
        return new StudentRecord(
            Id: student.Id,
            RegistrationNumber: student.RegistrationNumber,
            Name: student.Name,
            GPA: student.GPA,
            IsActive: student.IsActive,
            Version: student.Version
        );
    }

    public async Task<StudentRecord> RegisterAsync(string name, decimal GPA) // Removed 'age' from parameters
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student name is required.", nameof(name));

        // Create a new Student entity
        var studentEntity = new Student
        {
            RegistrationNumber = Guid.NewGuid().ToString("N")[..8].ToUpper(), // Generate a unique reg number
            Name = name,
            GPA = GPA,
            IsActive = true,
        };

        _context.Students.Add(studentEntity);

        // Access the "Shadow" property through the Entry API
        _context.Entry(studentEntity).Property("LastUpdated").CurrentValue = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Registered student {studentName} with ID: {studentId}",
            studentEntity.Name,
            studentEntity.Id
        );

        // Map the created entity to the DTO before returning
        return MapToStudentRecord(studentEntity);
    }

    public async Task<StudentRecord?> GetByIdAsync(int id, bool includeDeleted = false)
    {
        IQueryable<Student> query = _context.Students;

        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        var student = await query.FirstOrDefaultAsync(s => s.Id == id);
        return student == null ? null : MapToStudentRecord(student);
    }

    public async Task<PagedResult<StudentRecord>> GetPagedStudentsAsync(
        int pageNumber,
        int pageSize
    )
    {
        if (pageNumber < 1)
            pageNumber = 1;
        if (pageSize < 1 || pageSize > 100)
            pageSize = 20;

        int recordsToSkip = (pageNumber - 1) * pageSize;

        // Get total count first for pagination metadata
        var totalStudents = await _context.Students.CountAsync();

        // Perform the paged query, mapping to StudentRecord DTOs
        var pagedStudentEntities = await _context
            .Students.OrderBy(s => s.Name)
            .Skip(recordsToSkip)
            .Take(pageSize)
            .ToListAsync();

        var pagedStudentRecords = pagedStudentEntities.Select(MapToStudentRecord).ToList();

        return new PagedResult<StudentRecord>(
            PageNumber: pageNumber,
            PageSize: pageSize,
            TotalCount: totalStudents,
            TotalPages: (int)Math.Ceiling(totalStudents / (double)pageSize),
            Data: pagedStudentRecords.AsReadOnly()
        );
    }

    public async Task<IReadOnlyList<StudentRecord>> GetAllAsync()
    {
        var studentEntities = await _context.Students.ToListAsync();

        var studentRecords = studentEntities.Select(MapToStudentRecord).ToList();

        return studentRecords.AsReadOnly(); // Return as IReadOnlyList for immutability
    }

    public async Task<StudentRecord?> UpdateAsync(int id, UpdateStudentRequest request)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
        if (student == null)
            return null;

        // Exercise 8: Set the original version to check for concurrency
        _context.Entry(student).Property(s => s.Version).OriginalValue = request.Version;

        // Update all editable fields
        if (request.Name != null)
            student.Name = request.Name;
        if (request.GPA.HasValue)
            student.GPA = request.GPA.Value;
        if (request.IsActive.HasValue)
            student.IsActive = request.IsActive.Value;

        // Set Audit Shadow Property
        _context.Entry(student).Property("LastUpdated").CurrentValue = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            return MapToStudentRecord(student);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concurrency conflict on Student {Id}", id);
            throw; // Controller will catch this
        }
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
        if (student == null)
            return false;

        student.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> BulkArchiveEnrollmentsAsync(int yearThreshold)
    {
        // We want to archive everything where the EnrolledAt year is LESS than that.

        int rowsAffected = await _context
            .Enrollments.Where(e => e.EnrolledAt.Year < yearThreshold && !e.IsArchived)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.IsArchived, true));

        _logger.LogInformation("Bulk archive completed. {Count} rows archived.", rowsAffected);
        return rowsAffected;
    }

    public async Task<bool> DeleteAsync(int id) // Changed to int id
    {
        // First, check for existing enrollments using the DbContext
        var hasEnrollment = await _context.Enrollments.AnyAsync(e => e.StudentId == id);

        if (hasEnrollment)
        {
            _logger.LogWarning("Cannot delete student {studentId}: active enrollments exist.", id);
            return false;
        }

        // Find the student to delete
        var studentToDelete = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);

        if (studentToDelete == null)
        {
            _logger.LogWarning("Delete Failed: student with ID: {studentId} not found.", id);
            return false;
        }

        _context.Students.Remove(studentToDelete); // Stage for deletion
        await _context.SaveChangesAsync(); // Commit deletion to the database

        _logger.LogInformation("Deleted Student {studentId}", id);
        return true;
    }
}
