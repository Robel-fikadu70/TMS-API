// TmsApi/DTOs/EnrollmentRecord.cs
namespace TmsApi.DTOs;

public record EnrollmentRecord(
    int Id,
    string StudentId,
    string CourseCode,
    decimal? Grade,
    DateTime EnrolledAt
);
