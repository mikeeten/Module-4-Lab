### Exercise 6: The Consistent Fault (Standardized Error Handling)

**Context:** When the API crashes, it currently returns a raw HTML error page. The TMS frontend needs structured JSON (RFC 9457 `ProblemDetails`) so it can show a helpful alert whether the failure was saving an Enrollment, publishing a Course outline, recording an Assessment attempt, or issuing a Certificate. 

*Note: `UseStatusCodePages` is optional but recommended alongside `ProblemDetails`. It turns empty-body status codes (such as bare 404 responses) into the same consistent JSON shape.*

> [!NOTE]
> **Your Task: Implement ProblemDetails Exception Handling**
> 
> Configure your application pipeline to catch unhandled errors and format them into the RFC-compliant `ProblemDetails` JSON schema.
> 
> **1. Pipeline Configuration (`Program.cs`):**
> Configure these settings within your application startup flow:
> ```csharp
> // TODO 1: In the Builder section, add the ProblemDetails service
> builder.Services.AddProblemDetails();
> 
> // TODO 2: In the Middleware section, use the Exception Handler
> app.UseExceptionHandler();
> 
> // TODO 3 (optional alignment with Essentials): 
> app.UseStatusCodePages();
> ```
> 
> **2. Exception Class Definition:**
> Add this custom exception class to your project (either in a new file or at the bottom of `EnrollmentService.cs`):
> ```csharp
> public class TmsDatabaseException(string message) : Exception(message);
> ```
> 
> **3. Testing Endpoint Configuration:**
> Wire this test route into your `Program.cs` file to verify the exception handling behavior:
> ```csharp
> // TODO 4: Map a test route '/api/error' that intentionally throws.
> app.MapGet("/api/error", () =>
> {
>     throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
> });
> ```

* **Run / Call / Expected / Common failure**
  * **Run:** Execute `dotnet run` in your terminal.
  * **Call:** `GET http://localhost:5000/api/error` (or whatever path throws inside your test route).
  * **Expected:** Response is a clean JSON payload containing at least `type`, `title`, `status`, and `detail` fields (RFC 9457 `ProblemDetails` shape) instead of a raw system stack trace rendered as HTML.
  * **Common failure raw HTML:** `UseExceptionHandler()` is missing or registered after endpoint execution so exceptions never reach it. Place it before your routing/mapping middleware layers per your Essentials guidelines.
