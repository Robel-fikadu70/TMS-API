using System.Collections.Generic;
using System.Linq;

public record StudentRecord(string Id, string Name, int Age, decimal GPA);

public interface IStudentService
{
    Task<StudentRecord> RegisterAsync(string name, int age, decimal GPA);
    Task<StudentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<StudentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public class StudentService : IStudentService
{
    public static readonly Dictionary<string, StudentRecord> _store = new();
    private readonly ILogger<StudentService> _logger;

    public StudentService(ILogger<StudentService> logger)
    {
        _logger = logger;
    }

    public async Task<StudentRecord> RegisterAsync(string name, int age, decimal GPA)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student name is required.");

        var id = Guid.NewGuid().ToString("N")[..8];

        var record = new StudentRecord(id, name, age, GPA);

        _store[id] = record;

        _logger.LogInformation("Registered {name} in record, with ID: {studentId}", name, id);

        return record;
    }

    public async Task<StudentRecord?> GetByIdAsync(string id)
    {
        if (!_store.TryGetValue(id, out var record))
        {
            _logger.LogWarning("Student with ID {studentID} not found", id);
            return null;
        }

        return record;
    }

    public async Task<IReadOnlyList<StudentRecord>> GetAllAsync()
    {
        return _store.Values.ToList();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var hasEnrollment = EnrollmentService._store.Values.Any(e => e.StudentId == id);

        if (hasEnrollment)
        {
            _logger.LogWarning("Cannot delete student {studentId}, active enrollments exist", id);

            return false;
        }
        var removed = _store.Remove(id);
        if (removed)
        {
            _logger.LogInformation("Deleted Student {studentId}", id);
        }
        else
        {
            _logger.LogWarning("Delete Failed: student with ID: {studenId} not found", id);
        }
        return removed;
    }
}
