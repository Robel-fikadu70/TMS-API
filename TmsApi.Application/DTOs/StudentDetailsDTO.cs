namespace TmsApi.Application.DTOs;

public record StudentDetailDTO
{
    public required int Id { get; set; }
    public required string RegistrationNumber { get; set; }
    public required string Name { get; set; }
    public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;

    public required IReadOnlyList<LinkDto> Links { get; init; }
}
