### Exercise 5: The Enrollment API (Controllers with Real CRUD)

**Context:** Now that the infrastructure is stable, you must expose HTTP endpoints for the Angular frontend. The TMS uses MVC Controllers where each resource gets its own controller class with a constructor-injected service.

#### Before You Code (Prerequisites)
1. Confirm `builder.Services.AddControllers();` is in the builder section of `Program.cs` (added in Session 2 prerequisites).
2. Confirm `app.MapControllers();` is in the middleware section (after auth, before `app.Run()`).
3. Create a `Controllers` folder in your project root directory.

> [!NOTE]
> **Your Task: Implement EnrollmentsController CRUD Operations**
> 
> Create a new file called `Controllers/EnrollmentsController.cs` and implement the setup, creation, and deletion actions outlined below.
> 
> **Part A: GET Endpoints Setup**
> ```csharp
> using Microsoft.AspNetCore.Mvc;
> 
> [ApiController]
> [Route("api/enrollments")]
> public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
> {
>     // GET /api/enrollments returns all enrollment records
>     [HttpGet]
>     public async Task<IActionResult> GetAll()
>     {
>         var enrollments = await enrollmentService.GetAllAsync();
>         return Ok(enrollments);
>     }
> 
>     // GET /api/enrollments/{id} returns one or 404
>     [HttpGet("{id}")]
>     public async Task<IActionResult> GetById(string id)
>     {
>         var record = await enrollmentService.GetByIdAsync(id);
>         return record is not null ? Ok(record) : NotFound();
>     }
> }
> ```
> *Verification Check:* Run `dotnet run`, then call `GET /api/enrollments`. You should see a `200 OK` status with an empty array `[]` (no enrollments yet).
> 
> **Part B: POST Endpoint with 201 + Location Header**
> Add this POST action to the same controller:
> ```csharp
> // POST /api/enrollments creates and returns 201 with Location header
> [HttpPost]
> public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
> {
>     var record = await enrollmentService.EnrollAsync(request.StudentId, request.CourseCode);
>     return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
> }
> ```
> Add the supporting request model at the bottom of the same file (or inside a separate `Models` folder):
> ```csharp
> public record CreateEnrollmentRequest(string StudentId, string CourseCode);
> ```
> *REST Compliance Note:* `CreatedAtAction` does three things natively: sets the status to `201 Created`, sets the HTTP `Location` header to the unique URL of the new resource (`/api/enrollments/{id}`), and returns the created record payload inside the response body. This is the correct HTTP semantic for resource creation rather than a bare `200 OK`.
> 
> **Part C: DELETE Endpoint with 204 / 404 Handlers**
> Add this DELETE action to the controller class:
> ```csharp
> // DELETE /api/enrollments/{id} returns 204 or 404
> [HttpDelete("{id}")]
> public async Task<IActionResult> Delete(string id)
> {
>     var deleted = await enrollmentService.DeleteAsync(id);
>     return deleted ? NoContent() : NotFound();
> }
> ```
> *Note: `NoContent()` returns a standard HTTP `204 No Content` status, which represents a successful deletion operation that returns no response body.*

* **Run / Call / Expected / Common failure**
  * **Run:** Execute `dotnet run` in your terminal.
  * **Call:** Use `curl`, your browser, or Scalar to test all four endpoints sequentially:
    ```bash
    # 1. GET all (empty list)
    curl http://localhost:5000/api/enrollments
    # Expected: 200 OK with []
    
    # 2. POST a new enrollment
    curl -X POST http://localhost:5000/api/enrollments \
      -H "Content-Type: application/json" \
      -d '{"studentId":"S-001","courseCode":"CS-101"}' 
    # Expected: 201 Created with Location header and JSON body
    
    # 3. GET the created enrollment (replace {id} with the string returned from step 2)
    curl http://localhost:5000/api/enrollments/{id}
    # Expected: 200 OK with the record details
    
    # 4. DELETE the enrollment
    curl -X DELETE http://localhost:5000/api/enrollments/{id}
    # Expected: 204 No Content
    
    # 5. GET the deleted enrollment again
    curl http://localhost:5000/api/enrollments/{id}
    # Expected: 404 Not Found
    ```
  * **Common failure (404 for everything):** Missing `AddControllers()` or `MapControllers()` in `Program.cs`, or a typo in the `[Route]` prefix.
  * **Common failure (POST returns 200 instead of 201):** You used `Ok()` instead of `CreatedAtAction()`. The `Location` header is required for REST compliance.
  * **Common failure (POST returns 500 with "Value cannot be null"):** The `[FromBody]` attribute is missing from the method parameter, or the `Content-Type: application/json` header was omitted from the request client.

#### Troubleshooting Tips
* If `CreatedAtAction` throws a runtime error stating *"No route matches the supplied values,"* your `nameof(GetById)` argument does not match your target GET action method name. They must be exactly identical.
* If `curl` is unavailable on Windows environments, use a native PowerShell block instead:
  ```powershell
  Invoke-WebRequest -Uri "http://localhost:5000/api/enrollments" -Method GET
  ```
