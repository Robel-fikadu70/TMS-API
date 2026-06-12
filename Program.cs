using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICES (BUILDER SECTION) ---

builder.Services.AddControllers(); // Required for Exercise 5
builder.Services.AddProblemDetails(); // Required for Exercise 6
builder.Services.AddExceptionHandler(options => { }); // Required to prevent startup crash
builder.Services.AddOpenApi(); // Required for Exercise 7

// Exercise 2: Services & DI Validation
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Exercise 3: Options Pattern
builder
    .Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Session 1: Auth
builder
    .Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

var app = builder.Build();

// --- 2. MIDDLEWARE PIPELINE (ORDER MATTERS) ---

// 1. Logging is the outer wrapper (Session 1B)
app.UseMiddleware<RequestLoggingMiddleware>();

// 2. Exception handling (Session 3 / Exercise 6)
app.UseExceptionHandler();
app.UseStatusCodePages(); // Turns 404s into JSON ProblemDetails

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 3. Environment Toggle (Exercise 7)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// 4. Map Controllers (Exercise 5)
app.MapControllers();

// --- 3. MINIMAL API ENDPOINTS (FOR TESTING) ---

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

app.MapGet(
    "/api/error",
    () =>
    {
        throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
    }
);

app.Run();
