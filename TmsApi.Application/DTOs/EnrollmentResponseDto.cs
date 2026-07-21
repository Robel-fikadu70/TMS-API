namespace TmsApi.Application.DTOs;

public record EnrollmentResponseDto(int Id, int CourseId, int StudentId, DateTime EnrolledAt);
public record EnrollmentWithDetailsResponseDto(
    int Id, 
    int CourseId, 
    string CourseName,
    string CourseCode,
    int StudentId, 
    DateTime EnrolledAt
);
