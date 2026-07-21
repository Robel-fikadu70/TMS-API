using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

public interface IStudentService
{
    Task<StudentResponseDTO> RegisterAsync(RegisterStudentRequest request, CancellationToken ct);
    Task<StudentResponseDTO?> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResponse<StudentResponseDTO>> GetPagedStudentsAsync(
        PagedRequest request,
        bool includeDeleted,
        CancellationToken ct
    );
    Task<StudentResponseDTO?> UpdateAsync(int id, UpdateStudentRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> SoftDeleteAsync(int id);
    Task<int> BulkArchiveEnrollmentsAsync(int yearThreshold);
    Task<bool> StudentExists(int studentId, CancellationToken ct);
}