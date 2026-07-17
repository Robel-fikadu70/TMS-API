using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.DTOs;
using TmsApi.Entities;

namespace TmsApi.Services;

public interface IAssessmentService
{
    Task<AssessmentResponseDto> CreateAsync(
        int courseId,
        CreateAssessmentRequest request,
        CancellationToken ct
    );
    Task<IEnumerable<AssessmentResponseDto>> GetByCourseIdAsync(int courseId, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}

public class AssessmentService(TmsDbContext _context, ILogger<AssessmentService> _logger)
    : IAssessmentService
{
    public async Task<AssessmentResponseDto> CreateAsync(
        int courseId,
        CreateAssessmentRequest request,
        CancellationToken ct
    )
    {
        var currentWeigh = await _context
            .Assessments.Where(a => a.CourseId == courseId)
            .SumAsync(a => a.Weight, ct);

        if (currentWeigh + request.Weight > 1.0m)
        {
            throw new InvalidOperationException("Total course weight cannot exceed 100% (1.0).");
        }

        var assessment = new Assessment
        {
            Title = request.Title,
            MaxScore = request.MaxScore,
            Weight = request.Weight,
            CourseId = courseId,
        };

        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync(ct);
        return new AssessmentResponseDto(
            assessment.Id,
            assessment.Title,
            assessment.MaxScore,
            assessment.Weight,
            assessment.CourseId
        );
    }

    public async Task<IEnumerable<AssessmentResponseDto>> GetByCourseIdAsync(
        int courseId,
        CancellationToken ct
    )
    {
        return await _context
            .Assessments.AsNoTracking()
            .Where(a => a.CourseId == courseId)
            .Select(a => new AssessmentResponseDto(a.Id, a.Title, a.MaxScore, a.Weight, a.CourseId))
            .ToListAsync(ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var assessmentToDelete = await _context.Assessments.FindAsync([id], ct);
        if (assessmentToDelete == null)
            return false;

        _context.Assessments.Remove(assessmentToDelete);
        await _context.SaveChangesAsync(ct);

        return true;
    }
}
