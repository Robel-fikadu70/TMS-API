using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

public record CreateCourseRequest
{
    [Required]
    [RegularExpression(@"^[A-Z]{3}-\d{3}$", ErrorMessage = "Code must follow pattern XXX-000")]
    public required string Code { get; init; }

    [Required, MaxLength(200)]
    public required string Title { get; init; }

    [Range(1, 200)]
    public int Capacity { get; init; }
}
