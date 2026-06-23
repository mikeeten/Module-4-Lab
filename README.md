# Exercise 2: The Memory Leak (Captive Dependencies)

Context: The TMS has a background worker that recalculates scholarships every hour. The server crashed with an OutOfMemoryException. The investigation traced the crashtoa Scoped service (IEnrollmentService) being held by a Singleton (EnrollmentWorker) a captive dependency. 

# Quick reference the three lifetimes

* **Transient:** New instance every time it is resolved.
* **Scoped:** One instance per HTTP request; disposed when the request ends.
* **Singleton:** One instance for the entire application lifetime.

If a Scoped service (for example something that should track this request’s enrollments)
is captured inside a Singleton, it can live across requests. Under load, connections or in- memory state leak or go stale another request may see the wrong student’s data.
Prerequisite (read once): Many HTTP requests can be in flight at the same time. Eachrequest should get its own scoped services. A singleton lives forever it must not keepareference to a scoped instance created for an old request.

# First, make the failure visible

> [!NOTE]
> **Step A: Buggy registration (temporary)**
> 
> Implement `EnrollmentWorker` so its constructor takes `IEnrollmentService` directly (not `IServiceScopeFactory` yet).
> 
> **Service Registration:**
> ```csharp
> builder.Services.AddSingleton<EnrollmentWorker>();
> builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
> ```
> 
> **Host Validation Setup:**
> Add host validation so the container catches illegal lifetime wiring early:
> ```csharp
> builder.Host.UseDefaultServiceProvider(options =>
> {
>     options.ValidateScopes = true;
>     options.ValidateOnBuild = true;
> });
> ```
> 
> **Execution:**
> `dotnet run`

Expected (the “good” failure): The app throws at startup or on first resolve with anerror similar to:
Cannot consume scoped service 'IEnrollmentService' from singleton 'EnrollmentWorker'. That message IS the captive dependency detector working. Read it carefully it names
the two lifetimes that clash. Step B Confirm the failure: If the app refused to start in Step A, that IS the expectedfailure. Read the error message carefully it names both lifetimes. Skip ahead to “Your
Task” below.
If the app somehow started (unlikely with ValidateOnBuild = true), add this smoke-test
route to Program.cs and run concurrent requests to expose the bug:

> [!NOTE]
> **Worker Smoke Test Endpoint & Parallel Verification**
> 
> Add this endpoint to your API setup:
> ```csharp
> app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
> {
>     worker.ProcessBatch();
>     return Results.Ok("processed");
> });
> ```
> 
> **Execution via PowerShell:**
> Run this parallel script block from your PowerShell terminal (replace the base URL with yours from `dotnet run`):
> ```powershell
> \$base = "http://localhost:5000" # or https://localhost:7xxx match your terminal
> 1..15 | ForEach-Object -Parallel {
>     Invoke-WebRequest -Uri "\$using:base/api/enrollments/worker-smoke" -UseBasicParsing | Out-Null
> } -ThrottleLimit 15
> ```



Expected: Exceptions or inconsistent data under parallel calls the scoped service is beingshared across requests. Troubleshooting: If nothing fails, confirm ValidateScopes = true is set. If the app refuses tostart after the singleton registration, that IS the expected failure proceed to the fix below. 

# Your Task: Fix the Captive Dependency

Inject IServiceScopeFactory into the singleton and create a short-lived scope each timethe worker runs. Resolve IEnrollmentService from that scope only. 

> [!NOTE]
> **Step B: Safe Singleton Dependency Resolution via IServiceScopeFactory**
> 
> **Service Registrations:**
> These registrations are given; do **NOT** change them:
> ```csharp
> builder.Services.AddSingleton<EnrollmentWorker>();
> builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
> ```
> 
> **Implementation inside `EnrollmentWorker.cs`:**
> ```csharp
> public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
> {
>     public void ProcessBatch()
>     {
>         // TODO 2: Create a short-lived scope using the injected factory. 
>         // Stuck? using var scope = scopeFactory.CreateScope();
> 
>         // TODO 3: Resolve the scoped service from the new scope's provider. 
>         // Stuck? var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
> 
>         // TODO 4: Use the service, then let the 'using' block dispose the scope
>         // and its scoped services automatically. 
>     }
> }
> ```

# Run / Call / Expected / Common failure
 **Run / verify / common failure**
  * **Run:** `dotnet run` after implementing `IServiceScopeFactory`.
  * **Call:** Same as Step A or B startup should succeed; if you have `worker-smoke`, run the PowerShell parallel block again.
  * **Expected after fix:** No `Cannot consume scoped service…` at startup; under parallel calls, behavior is stable (no cross-request stale state for scoped work done inside `ProcessBatch`).
  * **Common failure:** Forgetting `using` on the scope—scoped services never dispose.
  * **Common failure:** Resolving `IEnrollmentService` from the root `app.Services` inside the singleton is still wrong. Always `CreateScope()` first.

