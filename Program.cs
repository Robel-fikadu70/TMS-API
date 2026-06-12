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

var builder = WebApplication.CreateBuilder(args);

// --- SERVICES ---
builder
    .Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

// FIX: Add ProblemDetails to satisfy the ExceptionHandler "slot"
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler(options => { });

var app = builder.Build();

// --- MIDDLEWARE PIPELINE (Order is critical for Exercise 1B) ---

// 1. Custom Logging (Outer wrapper)
app.UseMiddleware<RequestLoggingMiddleware>();

// 2. Exception Handler (Must be early to catch errors from below)
app.UseExceptionHandler();

// 3. The rest of the security/routing stack
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 4. Endpoints
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
