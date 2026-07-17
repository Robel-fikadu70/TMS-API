namespace TmsApi.DTOs;

public record CreateAssessmentRequest(string Title, decimal MaxScore, decimal Weight);

public record UpdateAssessmentRequest(string Title, decimal MaxScore, decimal Weight);

public record AssessmentResponseDto(
    int Id,
    string Title,
    decimal MaxScore,
    decimal Weight,
    int CourseId
);

public record AssessmentDetailDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required decimal MaxScore { get; init; }
    public required decimal Weight { get; init; }
    public required int CourseId { get; init; }
    public required IReadOnlyList<LinkDto> Links { get; init; }
}
