### Exercise 4: The Unsearchable Logs (Structured Logging)

**Context:** A student complained that their grade was not saved. The IT team searched Application Insights for logs, but could not filter by the student’s ID because the logs were written as raw concatenated strings. 

The difference between searchable and unsearchable logs comes down to one core development habit:

**Anti-Pattern (Do NOT use):**
```csharp
// BAD: one concatenated blob, not queryable
logger.LogInformation("Enrolling student " + studentId + " in course " + course);
```

**Structured Pattern (Always use):**
```csharp
// GOOD: StudentId and Course become queryable properties in any log aggregator
logger.LogInformation("Enrolling student {StudentId} in course {Course}", studentId, course);
```

> [!NOTE]
> **Your Task: Audit and Fix EnrollmentService Logging**
> 
> Open `EnrollmentService.cs`. The `EnrollAsync` method already uses structured logging. Now apply the same discipline across the service and add proper log levels.
> 
> **1. Add a LogWarning to `EnrollAsync` for duplicate checks:**
> ```csharp
> public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
> {
>     // Check for duplicate enrollment
>     var existing = _store.Values
>         .FirstOrDefault(e => e.StudentId == studentId && e.CourseCode == courseCode);
>         
>     if (existing is not null)
>     { 
>         _logger.LogWarning("Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})", studentId, courseCode, existing.Id);
>         return Task.FromResult(existing);
>     }
>     
>     var id = Guid.NewGuid().ToString("N")[..8];
>     var record = new EnrollmentRecord(id, studentId, courseCode, DateTime.UtcNow); 
>     _store[id] = record; 
>     
>     _logger.LogInformation("Enrolled {StudentId} in {CourseCode} record {EnrollmentId}", studentId, courseCode, id);
>     return Task.FromResult(record);
> }
> ```
> 
> **2. Add a LogWarning to `GetByIdAsync` when the record does not exist:**
> ```csharp
> public Task<EnrollmentRecord?> GetByIdAsync(string id)
> { 
>     _store.TryGetValue(id, out var record);
>     if (record is null)
>     { 
>         _logger.LogWarning("Enrollment {EnrollmentId} not found", id);
>     }
>     return Task.FromResult(record);
> }
> ```
> 
> **3. Add structured logging to `DeleteAsync`:**
> ```csharp
> public Task<bool> DeleteAsync(string id)
> {
>     var removed = _store.Remove(id);
>     if (removed) 
>     {
>         _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
>     }
>     else
>     {
>         _logger.LogWarning("Delete failed: enrollment {EnrollmentId} not found", id);
>     }
>     return Task.FromResult(removed);
> }
> ```

#### Log Level Decision Rules
Use these architectural guidelines consistently throughout the TMS architecture:
* **Information:** A business event completed successfully (e.g., enrollment created or deleted).
* **Warning:** Something unexpected but recoverable happened (e.g., duplicate attempt or record not found).
* **Error:** An operation failed completely and needs urgent attention (e.g., system exceptions or data corruption).

* **Run / Call / Expected / Common failure**
  * **Run:** `dotnet run`, then call `POST /api/enrollments` twice with the same student and course variables (you will wire this endpoint in Session 3 / Exercise 5). For now, you can verify by calling the methods directly from a temporary test endpoint, or wait until Session 3.
  * **Call:** Watch the application console telemetry output stream.
  * **Expected:** The first execution logs `[Information] Enrolled S-001 in CS-101`. The second execution logs `[Warning] Duplicate enrollment attempt...`. A `GET` call requesting a non-existent identifier safely logs `[Warning] Enrollment xyz not found`.
  * **Common failure (Concatenation):** Still seeing concatenated strings because you used `"Enrolling " + studentId` instead of the structured pattern token layout `"Enrolling {StudentId}", studentId`.
  * **Common failure (Log Levels):** All output streams show `[Information]` because you omitted or forgot to map `LogWarning` inside your conditional edge-case blocks.
