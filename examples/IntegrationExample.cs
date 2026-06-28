// Example showing how to wire a custom service into ASP.NET Core DI in Program.cs
using AspNetSpaTemplate.Configuration;
using AspNetSpaTemplate.Services;
using AspNetSpaTemplate.Data.Repositories;

public class IntegrationExample
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // 1. Using existing extension methods
        services.AddCoreServices();

        // 2. Registering your own service
        // Since ProductService requires ProductRepository, and ProductRepository likely requires DbContext,
        // you would register them in order of dependency.
        
        // Example registration:
        // services.AddScoped<ProductRepository>();
        // services.AddScoped<ProductService>();
        
        // 3. Adding custom policies or configurations
        services.AddDevelopmentCors();
    }
}
