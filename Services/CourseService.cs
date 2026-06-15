using System.Collections.Generic;
using System.Linq;

public record CourseRecord(string Code, string Title, int Capacity, int EnrolledCount);

public interface ICourseService
{
    Task<CourseRecord> CreateAsync(string code, string title, int capacity);
    Task<CourseRecord?> GetByCodeAsync(string code);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string code);
}

public class CourseService : ICourseService
{
    public static readonly Dictionary<string, CourseRecord> _store = new();

    private readonly ILogger<CourseService> _logger;

    public CourseService(ILogger<CourseService> logger)
    {
        _logger = logger;
    }

    public async Task<CourseRecord> CreateAsync(string code, string title, int capacity)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Course code is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Course title is required.");

        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than 0.");

        if (_store.ContainsKey(code))
            throw new ArgumentException($"Course {code} already exists.");

        var record = new CourseRecord(code, title, capacity, 0);

        _store[code] = record;

        _logger.LogInformation("Created course {CourseCode} with title {CourseTitle}", code, title);

        return record;
    }

    public async Task<CourseRecord?> GetByCodeAsync(string code)
    {
        if (!_store.TryGetValue(code, out var record))
        {
            _logger.LogWarning("Course {CourseCode} not found", code);

            return null;
        }

        return record;
    }

    public async Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    {
        return _store.Values.ToList();
    }

    public async Task<bool> DeleteAsync(string code)
    {
        var hasEnrollment = EnrollmentService._store.Values.Any(e => e.CourseCode == code);

        if (hasEnrollment)
        {
            _logger.LogWarning("Cannot delete course {CourseCode}, active enrollments exist", code);

            return false;
        }
        var removed = _store.Remove(code);

        if (removed)
        {
            _logger.LogInformation("Deleted course {CourseCode}", code);
        }
        else
        {
            _logger.LogWarning("Delete failed: course {CourseCode} not found", code);
        }

        return removed;
    }
}
