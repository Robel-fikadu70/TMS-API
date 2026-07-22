using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;
public interface ICourseService
{
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest course, CancellationToken ct);
    Task<bool> DeleteAsync(string code);
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct
    );
    Task<CourseResponseDto?> GetByCodeAsync(string code, CancellationToken ct);

    Task<IReadOnlyList<TopCourseSummaryRecord>> GetTopCoursesByEnrollmentAsync(int topCount);
    Task<List<Course>> GetAllAsync(CancellationToken ct);
}