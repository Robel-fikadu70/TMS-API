using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;
public interface IAssessmentService
{
    Task<AssessmentResponseDto> CreateAsync(
        int courseId,
        CreateAssessmentRequest request,
        CancellationToken ct
    );
    Task<IEnumerable<AssessmentResponseDto>> GetByCourseIdAsync(int courseId, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<AssessmentResponseDto?> GetByIdAsync(int assessmentId, CancellationToken ct);
    Task<AssessmentResponseDto> UpdateAsync(
        int assessmentId,
        UpdateAssessmentRequest request,
        CancellationToken ct
    );
}