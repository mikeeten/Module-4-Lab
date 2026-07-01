### Exercise 7: The Environment Toggle (Dev vs Prod)

**Context:** In Development, you want rich diagnostics and an interactive API explorer. In Production, you must hide api explorers entirely and avoid leaking critical system stack traces to external clients.

#### Before You Code (Prerequisites)
1. Install Scalar (required for `MapScalarApiReference`):
   ```bash
   dotnet add package Scalar.AspNetCore
   ```
2. Add the OpenAPI service registration and the Scalar namespaces to `Program.cs`:
   ```csharp
   using Scalar.AspNetCore;
   
   var builder = WebApplication.CreateBuilder(args);
   
   // ... your existing service registrations ... 
   builder.Services.AddOpenApi(); // Required before MapOpenApi() will work
   ```
   *Warning: Without `AddOpenApi()`, calling `MapOpenApi()` later will throw an exception at application startup.*

> [!NOTE]
> **Your Task: Implement Environment-Aware Pipelines**
> 
> Configure your application pipeline to expose interactive documentation tools exclusively in the Development environment while locking down your Production configuration.
> 
> **Pipeline Configuration (`Program.cs`):**
> ```csharp
> // TODO 1: Check if the app is running in Development mode. 
> if (app.Environment.IsDevelopment()) 
> { 
>     // TODO 2: In Development only expose the OpenAPI document and an interactive API explorer. 
>     // Use the built-in MapOpenApi() and MapScalarApiReference(). 
>     app.MapOpenApi(); 
>     app.MapScalarApiReference();
> }
> else
> {
>     // TODO 3: In Production use the exception handler middleware so stack traces
>     // are never shown to external users. 
>     app.UseExceptionHandler();
> }
> ```
> 
> **Testing & Verification Tasks:**
> * **TODO 4:** Run your application in both environments and verify the behaviors listed in the execution sections below.

* **Run / Call / Expected / Common failure**

  **Development Environment Verification:**
  * **Run:** Execute `dotnet run` (the default value remains set to `Development` in your local `launchSettings.json`).
  * **Call:** Open your browser and navigate to `http://localhost:5000/scalar/v1` (path may vary slightly depending on your template; use the link shown in your console if different).
  * **Expected:** The interactive Scalar UI loads successfully and lists your available OpenAPI documentation endpoints.

  **Production Environment Verification (Windows PowerShell):**
  * **Run:** Force your shell session into Production mode and initialize the runtime:
    ```powershell
    \$env:ASPNETCORE_ENVIRONMENT = "Production"
    dotnet run
    ```
  * **Call:** Try hitting the same Scalar documentation URL used above, and trigger an endpoint that intentionally throws an unhandled error (such as `/api/error`).
  * **Expected:** The `/scalar/v1` route returns an HTTP `404 Not Found` response or is completely unexposed; unhandled errors safely return standard `ProblemDetails` JSON payloads instead of raw HTML stack traces.
  * **Cleanup:** Reset your shell session variables when done testing:
    ```powershell
    Remove-Item Env:ASPNETCORE_ENVIRONMENT
    ```

  * **Common failure (Scalar does not compile):** The underlying dependency package was not added (`dotnet add package Scalar.AspNetCore`) or the namespace declaration `using Scalar.AspNetCore;` is missing from the top of your code file.
