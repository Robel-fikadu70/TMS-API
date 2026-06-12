public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        // Create a short-lived scope manually
        using var scope = _scopeFactory.CreateScope();

        // Resolve the scoped service from the new scope's provider
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

        // Use the service... (Simulation)
        Console.WriteLine("Worker successfully processed batch using scoped service.");
    }
}
