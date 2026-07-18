namespace TmsApi.Application.DTOs
{
    public record TopCourseSummaryRecord(
        string CourseCode,
        string CourseTitle,
        int EnrollmentCount = 0
    );
}
