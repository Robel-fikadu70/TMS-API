namespace TmsApi.DTOs;

public record StudentRecord(
    int Id,
    string RegistrationNumber,
    string Name,
    decimal GPA,
    bool IsActive
);
