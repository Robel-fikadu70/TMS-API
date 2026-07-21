using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

public interface ICertificateService
{
    Task<CertificateResponseDto> IssueAsync(IssueCertificateRequest request, CancellationToken ct);
    Task<CertificateResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CertificateResponseDto?> GetBySerialAsync(string serial, CancellationToken ct);
    Task<IEnumerable<CertificateResponseDto>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct
    );
    Task<bool> RevokeAsync(int id, CancellationToken ct);
}