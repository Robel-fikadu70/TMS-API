namespace TmsApi.DTOs;

public record IssueCertificateRequest(int StudentId, int CourseId);

public record CertificateResponseDto(
    int Id,
    string SerialNumber,
    DateTime IssuedAt,
    int StudentId,
    string StudentName,
    string CourseTitle
);
