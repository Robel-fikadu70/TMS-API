using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Api.Filters;
using TmsApi.Api.Middlewares;
using TmsApi.Api.Security;
using TmsApi.Application.Common;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICES (BUILDER SECTION) ---

builder.Services.AddProblemDetails(); // Required for Exercise 6
builder.Services.AddOpenApi(); // Required for Exercise 7
builder.Services.AddControllers(options =>
{
    // This applies the filter to EVERY controller in the project
    options.Filters.Add<AuditLogFilter>();
});
builder.Services.AddExceptionHandler(options => { }); // Required to prevent startup crash

// Exercise 2 Services & DI Validation
builder.Services.AddSingleton<EnrollmentWorker>();

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();

// Register TmsDbContext scoped for incoming HTTP requests

builder.Services.AddDbContext<TmsDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
        .LogTo(Console.WriteLine, LogLevel.Information) // Log SQLto output window
        .EnableSensitiveDataLogging()
); // Show parameters in querylogs (dev only)

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
app.UseStatusCodePages(); //( Exercise 6 TODO 3) Turns 404s into JSON ProblemDetails

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

// if (app.Environment.IsDevelopment())
// {
//     using var scope = app.Services.CreateScope();
//     var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
//     await TmsApi.Persistence.DataSeeder.SeedAsync(context);
// }

app.Run();
