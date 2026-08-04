using TmsApi.Domain.Entities;
namespace TmsApi.Application.DTOs;
public record EnrollmentDetailDto (
    int Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseTitle,
    EnrollmentStatus Status,
    DateTime EnrolledAt
);