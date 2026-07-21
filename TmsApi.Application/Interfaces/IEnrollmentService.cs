using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct
    );
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
    Task<PagedResponse<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        PagedRequest request,
        CancellationToken ct
    );
    Task<bool>ExistsAsync(int studentId, string courseCode, CancellationToken ct);
    Task<List<EnrollmentWithDetailsResponseDto>?>GetByStudentIdAsync(int studentId, CancellationToken ct);
    Task<bool> DeleteAsync(int id);
}