using System.Collections.Generic;
using System.Linq;

public record EnrollmentRecord(string Id, string StudentId, string CourseCode, DateTime EnrolledAt);

public interface IEnrollmentService
{
    Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
    Task<EnrollmentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public class EnrollmentService : IEnrollmentService
{
    public static readonly Dictionary<string, EnrollmentRecord> _store = new();
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ILogger<EnrollmentService> logger)
    {
        _logger = logger;
    }

    public async Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
    {
        // student exists
        if (!StudentService._store.ContainsKey(studentId))
        {
            throw new ArgumentException($"Student {studentId} does not exist.");
        }

        // course exists
        if (!CourseService._store.ContainsKey(courseCode))
        {
            throw new ArgumentException($"Course {courseCode} does not exist.");
        }
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
        // capacity check
        var course = CourseService._store[courseCode];

        if (course.EnrolledCount >= course.Capacity)
        {
            throw new ArgumentException($"Course {courseCode} is full.");
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new EnrollmentRecord(id, studentId, courseCode, DateTime.UtcNow);
        _store[id] = record;

        // increment enrolled count
        CourseService._store[courseCode] = course with
        {
            EnrolledCount = course.EnrolledCount + 1,
        };

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
        if (!_store.TryGetValue(id, out var enrollment))
        {
            _logger.LogWarning("Delete failed enrollment {EnrollmentId} not found", id);

            return false;
        }

        _store.Remove(id);

        if (CourseService._store.TryGetValue(enrollment.CourseCode, out var course))
        {
            CourseService._store[enrollment.CourseCode] = course with
            {
                EnrolledCount = course.EnrolledCount - 1,
            };
        }

        _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);

        return true;
    }
}

public class TmsDatabaseException(string message) : Exception(message);
