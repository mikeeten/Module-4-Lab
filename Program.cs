using TmsApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;



var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

builder.Services.AddAuthorization();

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();
// builder.Services.AddSingleton<EnrollmentWorker>();
// builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddOpenApi();

var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>(); 

if (app.Environment.IsDevelopment())
{
    // Development: expose OpenAPI + Scalar explorer
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    // Production: hide explorers, use exception handler
    app.UseExceptionHandler();
}


app.UseExceptionHandler("/api/error");
app.UseStatusCodePages();
app.UseRouting();
app.UseAuthentication();   // establish identity
app.UseAuthorization();    // enforce policies

app.MapControllers();

app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization(); 

app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});

app.Run();

