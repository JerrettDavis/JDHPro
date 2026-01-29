using Microsoft.Extensions.DependencyInjection;
using Reqnroll.Microsoft.Extensions.DependencyInjection;

namespace JdhPro.Tests.E2E.Support;

public static class ScenarioDependencies
{
    [ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        
        // Register WebDriverContext as singleton
        services.AddSingleton<WebDriverContext>();
        
        return services;
    }
}
