// TmsApi/DTOs/EnrollmentRecord.cs
namespace TmsApi.Application.DTOs;

public record EnrollmentRecord(
    int Id,
    string StudentId,
    string CourseCode,
    decimal? Grade,
    DateTime EnrolledAt
);
