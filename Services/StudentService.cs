using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.DTOs;
using TmsApi.Entities;

namespace TmsApi.Services;

public interface IStudentService
{
    Task<StudentResponseDTO> RegisterAsync(RegisterStudentRequest request, CancellationToken ct);
    Task<StudentResponseDTO?> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResponse<StudentResponseDTO>> GetPagedStudentsAsync(
        PagedRequest request,
        bool includeDeleted,
        CancellationToken ct
    );
    Task<StudentResponseDTO?> UpdateAsync(int id, UpdateStudentRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> SoftDeleteAsync(int id);
    Task<int> BulkArchiveEnrollmentsAsync(int yearThreshold);
    Task<bool> StudentExists(int studentId, CancellationToken ct);
}

public class StudentService(ILogger<StudentService> _logger, TmsDbContext _context)
    : IStudentService
{
    public async Task<StudentResponseDTO> RegisterAsync(
        RegisterStudentRequest request,
        CancellationToken ct
    )
    {
        // Create a new Student entity
        var studentEntity = new Student
        {
            RegistrationNumber = Guid.NewGuid().ToString("N")[..8].ToUpper(), // Generate a unique reg number
            Name = request.Name,
            GPA = request.GPA,
            IsActive = true,
        };

        _context.Students.Add(studentEntity);

        // Access the "Shadow" property through the Entry API
        _context.Entry(studentEntity).Property("LastUpdated").CurrentValue = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return (await GetByIdAsync(studentEntity.Id, ct))!;
    }

    public async Task<StudentResponseDTO?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context
            .Students.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentResponseDTO(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.GPA,
                s.Enrollments.Count,
                s.IsActive
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResponse<StudentResponseDTO>> GetPagedStudentsAsync(
        PagedRequest request,
        bool includeDeleted,
        CancellationToken ct
    )
    {
        var query = _context.Students.AsNoTracking();

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = $"%{request.Search}%";
            query = query.Where(s =>
                EF.Functions.ILike(s.Name, search)
                || EF.Functions.ILike(s.RegistrationNumber, search)
            );
        }

        var totalCount = await query.CountAsync(ct);

        var OrderBy = string.IsNullOrWhiteSpace(request.OrderBy) ? "Name" : request.OrderBy;
        query = OrderBy switch
        {
            "GPA" => request.Descending
                ? query.OrderByDescending(s => s.GPA)
                : query.OrderBy(s => s.GPA),
            "Name" or _ => request.Descending
                ? query.OrderByDescending(s => s.Name)
                : query.OrderBy(s => s.Name),
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StudentResponseDTO(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.GPA,
                s.Enrollments.Count,
                s.IsActive
            ))
            .ToListAsync(ct);

        return new PagedResponse<StudentResponseDTO>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }

    public async Task<StudentResponseDTO?> UpdateAsync(int id, UpdateStudentRequest request)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
        if (student == null)
            return null;

        // Set the original version to check for concurrency
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
            return new StudentResponseDTO(
                student.Id,
                student.RegistrationNumber,
                student.Name,
                student.GPA,
                student.Enrollments.Count,
                student.IsActive
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concurrency conflict on Student {Id}", id);
            throw; // Controller will catch this
        }
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var hasEnrollment = await _context.Enrollments.AnyAsync(e => e.StudentId == id);

        if (hasEnrollment)
        {
            _logger.LogWarning("Cannot delete student {studentId}: active enrollments exist.", id);
            return false;
        }

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

    public async Task<bool> DeleteAsync(int id)
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

    public async Task<bool> StudentExists(int studentId, CancellationToken ct)
    {
        var student = GetByIdAsync(studentId, ct);
        if (student == null)
            return false;

        return true;
    }
}
