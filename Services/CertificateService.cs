using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.DTOs;
using TmsApi.Entities;

namespace TmsApi.Services;

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

public class CertificateService(TmsDbContext context) : ICertificateService
{
    public async Task<CertificateResponseDto> IssueAsync(
        IssueCertificateRequest request,
        CancellationToken ct
    )
    {
        // 1. Verify Enrollment exists
        var enrollment = await context.Enrollments.FirstOrDefaultAsync(
            e => e.StudentId == request.StudentId && e.CourseId == request.CourseId,
            ct
        );

        if (enrollment == null)
            throw new InvalidOperationException("Student is not enrolled in this course.");

        // 2. Check if certificate already exists
        var exists = await context.Certificates.AnyAsync(
            c => c.StudentId == request.StudentId && c.CourseId == request.CourseId,
            ct
        );

        if (exists)
            throw new InvalidOperationException(
                "Certificate already issued for this student in this course."
            );

        // 3. Create the certificate
        var certificate = new Certificate
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            IssuedAt = DateTime.UtcNow,
            //Generate Serial
            SerialNumber =
                $"CERT-{request.StudentId}-{request.CourseId}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
        };

        context.Certificates.Add(certificate);
        await context.SaveChangesAsync(ct);

        return (await GetByIdAsync(certificate.Id, ct))!;
    }

    public async Task<CertificateResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context
            .Certificates.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CertificateResponseDto(
                c.Id,
                c.SerialNumber,
                c.IssuedAt,
                c.StudentId,
                c.Student.Name,
                c.Course.Title
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CertificateResponseDto?> GetBySerialAsync(string serial, CancellationToken ct)
    {
        return await context
            .Certificates.AsNoTracking()
            .Where(c => c.SerialNumber == serial)
            .Select(c => new CertificateResponseDto(
                c.Id,
                c.SerialNumber,
                c.IssuedAt,
                c.StudentId,
                c.Student.Name,
                c.Course.Title
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<CertificateResponseDto>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct
    )
    {
        return await context
            .Certificates.AsNoTracking()
            .Where(c => c.StudentId == studentId)
            .Select(c => new CertificateResponseDto(
                c.Id,
                c.SerialNumber,
                c.IssuedAt,
                c.StudentId,
                c.Student.Name,
                c.Course.Title
            ))
            .ToListAsync(ct);
    }

    public async Task<bool> RevokeAsync(int id, CancellationToken ct)
    {
        var cert = await context.Certificates.FindAsync([id], ct);
        if (cert == null)
            return false;

        context.Certificates.Remove(cert);
        await context.SaveChangesAsync(ct);
        return true;
    }
}
