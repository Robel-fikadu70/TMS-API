namespace TmsApi.DTOs;

public record CourseResponseDto(
    int Id,
    string Code,
    string Title,
    int Capacity,
    int EnrollmentCount
);
