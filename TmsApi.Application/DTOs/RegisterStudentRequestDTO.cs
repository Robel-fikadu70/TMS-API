using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

public record RegisterStudentRequest
{
    [Required]
    public required string Name { get; init; }

    [Required]
    public required decimal GPA { get; init; }
};

public record UpdateStudentRequest(string? Name, decimal? GPA, bool? IsActive, uint Version);
