using TmsApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;


var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

builder.Services.AddAuthorization();


var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>(); 

app.UseExceptionHandler("/error");
app.UseRouting();
app.UseAuthentication();   // establish identity
app.UseAuthorization();    // enforce policies



app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization(); 

app.Run();
