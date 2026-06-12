// // // File: EnrollmentWorker.cs
// // public class EnrollmentWorker
// // {
// //     private readonly IEnrollmentService _enrollmentService;

// //     // BAD: Constructor takes Scoped service directly
// //     public EnrollmentWorker(IEnrollmentService enrollmentService)
// //     {
// //         _enrollmentService = enrollmentService;
// //     }

// //     public void ProcessBatch()
// //     {
// //         // Simulate work
// //         Console.WriteLine("Processing batch...");
// //     }
// // }

// // File: EnrollmentWorker.cs
// using Microsoft.Extensions.DependencyInjection;

// public class EnrollmentWorker
// {
//     private readonly IServiceScopeFactory _scopeFactory;

//     // FIX: Inject IServiceScopeFactory instead of the Scoped service
//     public EnrollmentWorker(IServiceScopeFactory scopeFactory)
//     {
//         _scopeFactory = scopeFactory;
//     }

//     public void ProcessBatch()
//     {
//         // Create a new scope for this specific operation
//         // The 'using' statement ensures the scope (and its services) are disposed correctly
//         using var scope = _scopeFactory.CreateScope();

//         // Resolve the Scoped service from the NEW scope
//         var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

//         // Use the service
//         // Note: In a real scenario, you might call enrollmentService.GetAllAsync() here
//         Console.WriteLine("Processing batch with a fresh scoped service...");

//         // When 'using' block ends, the scope and the IEnrollmentService are disposed
//     }
// }

using Microsoft.Extensions.DependencyInjection; // Important

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    // FIX: Inject the Factory, not the service
    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        // FIX: Create a new scope for this specific operation
        using var scope = _scopeFactory.CreateScope();

        // FIX: Resolve the service from the NEW scope
        var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

        // Use the service safely
        Console.WriteLine("Worker processing batch with fresh scoped service...");

        // Example usage (optional):
        // var records = await enrollmentService.GetAllAsync();
    }
}
