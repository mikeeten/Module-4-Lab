namespace TmsApi;

// public class EnrollmentWorker
// {
//     private readonly IEnrollmentService _service;

//     // ❌ Bug: directly injects a scoped service into a singleton
//     public EnrollmentWorker(IEnrollmentService service)
//     {
//         _service = service;
//     }

//     public void ProcessBatch()
//     {
//         // Example usage: fetch all enrollments
//         var all = _service.GetAllAsync().Result;
//         Console.WriteLine($"Processed {all.Count} enrollments");
//     }
// }

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    // ✅ Correct: inject scope factory instead of scoped service
    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        // Create a short-lived scope for each batch
        using var scope = _scopeFactory.CreateScope();

        // Resolve the scoped service inside this scope
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

        var all = svc.GetAllAsync().Result;
        Console.WriteLine($"Processed {all.Count} enrollments");
    }
}


