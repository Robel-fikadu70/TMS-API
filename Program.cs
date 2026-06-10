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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Register Services
builder
    .Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

var app = builder.Build();

// 1. Custom Logging Middleware (Outermost)
// Must be first to wrap everything and add headers before any processing
app.UseMiddleware<RequestLoggingMiddleware>();

// 2. Exception Handler (Early to catch errors from logging or routing)
app.UseExceptionHandler();

// 3. Routing
app.UseRouting();

// 4. Authentication
app.UseAuthentication();

// 5. Authorization
app.UseAuthorization();

// 6. Endpoints
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

app.Run();
