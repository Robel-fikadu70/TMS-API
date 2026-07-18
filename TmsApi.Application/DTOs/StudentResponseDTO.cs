namespace TmsApi.Application.DTOs;

public record StudentResponseDTO(
    int Id,
    string RegistrationNumber,
    string Name,
    decimal GPA,
    int EnrollmentCount,
    bool IsActive
);
