using System.Collections.Generic;
using System.Linq;

// 1. THE DATA SHAPE (Lives at the top level)
public record EnrollmentRecord(string Id, string StudentId, string CourseCode, DateTime EnrolledAt);

// 2. THE CONTRACT (Lives at the top level)
public interface IEnrollmentService
{
    Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
    Task<EnrollmentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

// 3. THE IMPLEMENTATION
public class EnrollmentService : IEnrollmentService
{
    private static readonly Dictionary<string, EnrollmentRecord> _store = new();
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ILogger<EnrollmentService> logger)
    {
        _logger = logger;
    }

    // --- ALL METHODS MUST BE INSIDE THESE CLASS BRACES ---

    public async Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
    {
        // Exercise 4: Duplicate check with Warning
        var existing = _store.Values.FirstOrDefault(e =>
            e.StudentId == studentId && e.CourseCode == courseCode
        );

        if (existing is not null)
        {
            _logger.LogWarning(
                "Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})",
                studentId,
                courseCode,
                existing.Id
            );
            return existing;
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new EnrollmentRecord(id, studentId, courseCode, DateTime.UtcNow);
        _store[id] = record;

        _logger.LogInformation(
            "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
            studentId,
            courseCode,
            id
        );

        return record;
    }

    public async Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        if (!_store.TryGetValue(id, out var record))
        {
            // Exercise 4: Structured Warning
            _logger.LogWarning("Enrollment {EnrollmentId} not found", id);
            return null;
        }
        return record;
    }

    public async Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        return _store.Values.ToList();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);
        if (removed)
        {
            _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
        }
        else
        {
            _logger.LogWarning("Delete failed enrollment {EnrollmentId} not found", id);
        }
        return removed;
    }
} // <--- This closing brace must be at the very end of the file

public class TmsDatabaseException(string message) : Exception(message);
