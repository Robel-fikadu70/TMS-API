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
