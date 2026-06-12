// using Microsoft.AspNetCore.Authentication;

// var builder = WebApplication.CreateBuilder(args);

// // 1. Register Services
// // Add the custom "Training" authentication scheme
// builder
//     .Services.AddAuthentication("Training")
//     .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

// // Add Authorization services
// builder.Services.AddAuthorization();

// var app = builder.Build();

// // 2. Configure the Middleware Pipeline
// // Routing must happen first to identify the endpoint
// app.UseRouting();

// // Authentication (Who are you?) must run before Authorization
// app.UseAuthentication();

// // Authorization (Are you allowed?) must run before the endpoint executes
// app.UseAuthorization();

// // 3. Map the Endpoint
// app.MapGet(
//         "/api/assessments/results",
//         () =>
//             Results.Ok(
//                 new
//                 {
//                     courseCode = "CS-101",
//                     studentId = "S-001",
//                     letterGrade = "A",
//                 }
//             )
//     )
//     .RequireAuthorization();

// app.Run();

// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Diagnostics;

// var builder = WebApplication.CreateBuilder(args);

// // Register Services
// builder
//     .Services.AddAuthentication("Training")
//     .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
// builder.Services.AddAuthorization();

// var app = builder.Build();

// // 1. Custom Logging Middleware (Outermost)
// // Must be first to wrap everything and add headers before any processing
// app.UseMiddleware<RequestLoggingMiddleware>();

// // 2. Exception Handler (Early to catch errors from logging or routing)
// app.UseExceptionHandler();

// // 3. Routing
// app.UseRouting();

// // 4. Authentication
// app.UseAuthentication();

// // 5. Authorization
// app.UseAuthorization();

// // 6. Endpoints
// app.MapGet(
//         "/api/assessments/results",
//         () =>
//             Results.Ok(
//                 new
//                 {
//                     courseCode = "CS-101",
//                     studentId = "S-001",
//                     letterGrade = "A",
//                 }
//             )
//     )
//     .RequireAuthorization();

// app.Run();

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection; // Required for IServiceScopeFactory

// 1. Create Builder
var builder = WebApplication.CreateBuilder(args);

// --- EXERCISE 2 ---
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler(options => { });

builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// 2. Add Host Validation (This is the "Safety Catch")
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder
    .Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();



var app = builder.Build();

// 2. Configure the Middleware Pipeline
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 3. Map the Endpoint
app.MapGet(
        "/api/assessments/results",
        () =>
            Results.Ok(
                new
                {
                    courseCode = "CS-101",
                    studentId = "S-001",
                    letterGrade = "A",
                }
            )
    )
    .RequireAuthorization();

app.MapGet(
    "/api/enrollments/worker-smoke",
    (EnrollmentWorker worker) =>
    {
        worker.ProcessBatch();
        return Results.Ok("processed");
    }
);

app.Run();
