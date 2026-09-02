using NSubstitute;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Common;
using TmsApi.Domain.Entities;
using TmsApi.Application.DTOs;
namespace TmsApi.Tests;

public class EnrollStudentHandlerTests
{
    [Fact]
    public async Task Handle_WhenAlreadyEnrolled_ReturnsDuplicateError()
    {
        // Arrange: create a mock IEnrollmentService (Application-layer interface)
        var enrollmentService = Substitute.For<IEnrollmentService>();
        var courseService = Substitute.For<ICourseService>();
        var cachedCourseService = Substitute.For<ICachedCourseService>();

        enrollmentService
        .ExistsAsync(99, "CS-401", Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(true));
        // Course lookup runs first in the handler; return any non-null// course so the duplicate check is the branch under test.
        var courseDto = new CourseResponseDto
        (
            1,
            "CS-401",
            "Advanced Web Dev",
            30,
            5
        );
        courseService
        .GetByCodeAsync("CS-401", Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<CourseResponseDto?>(courseDto));
        var handler = new EnrollStudentHandler(enrollmentService, courseService, cachedCourseService);
        var command = new EnrollStudentCommand(StudentId: 99, CourseCode: "CS-401");
        // Act
        var result = await handler.Handle(command, CancellationToken.None);// Assert: handler surfaces the duplicate without touching the database.// Assert on the machine-readable Code (the contract) plus full record
                                                                           // equality, NOT on the human-readable Message,see M7 sealed-record pattern.
        Assert.False(result.IsSuccess);
        Assert.Equal("already_enrolled", result.Error.Code);
        Assert.Equal(EnrollmentError.AlreadyEnrolled(99, "CS-401"), result.Error);
    }
    [Fact]
    public async Task Handle_WhenCourseFull_ReturnsCapacityError()
    {
        // Arrange: course is at capacity (Enrollments.Count >= MaxCapacity).// M7's handler checks capacity against the course object, not via service// calls.
        var enrollmentService = Substitute.For<IEnrollmentService>();
        var courseService = Substitute.For<ICourseService>();
        var cachedCourseService = Substitute.For<ICachedCourseService>();

        var courseDto = new CourseResponseDto
        (
            1,
            "CS-401",
            "Advanced Web Dev",
            30,
            5
        );

        courseService
        .GetByCodeAsync("CS-401", Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<CourseResponseDto?>(courseDto));
        var handler = new EnrollStudentHandler(enrollmentService, courseService, cachedCourseService); 
        var command = new EnrollStudentCommand(StudentId: 100, CourseCode: "CS-401");
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        // Assert: typed error matches the M7 sealed-record factory
        Assert.False(result.IsSuccess);
        Assert.Equal("course_full", result.Error.Code);
        Assert.Equal(EnrollmentError.CourseFull("Advanced Web Dev", 35), result.Error);
        
    }
    [Fact]
    public async Task Handle_SuccessfulPath_AddsEnrollmentOnce()
    {
        // Arrange: course has room, student is not already enrolled; expect one// AddAsync call.
        var enrollmentService = Substitute.For<IEnrollmentService>();
        var courseService = Substitute.For<ICourseService>();
        var cachedCourseService = Substitute.For<ICachedCourseService>();
        var courseDto = new CourseResponseDto
        (
            1,
            "CS-401",
            "Advanced Web Dev",
            35,
            20 // Room available
        );
        courseService
        .GetByCodeAsync("CS-401", Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<CourseResponseDto?>(courseDto));

        enrollmentService
        .ExistsAsync(100, "CS-401", Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(false));
        var handler = new EnrollStudentHandler(enrollmentService, courseService, cachedCourseService); 
        var command = new EnrollStudentCommand(StudentId: 100, CourseCode: "CS-401");
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert: handler produced a typed success payload with the rightIDsAssert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.StudentId);
        Assert.Equal("CS-401", result.Value.CourseCode);
    
    }
}