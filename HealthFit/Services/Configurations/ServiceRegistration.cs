using System.Reflection;

namespace HealthFit.Services.Configurations
{
    public static class ServiceRegistration
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var interfaceTypes = assembly.GetTypes()
                .Where(t => t.IsInterface && t.Namespace != null && t.Namespace.Contains("Interfaces"));

            foreach (var interfaceType in interfaceTypes)
            {
                var implTypeName = interfaceType.Name.Substring(1); // Bỏ chữ 'I'
                var implementationType = assembly.GetTypes()
                    .FirstOrDefault(t =>
                        t.IsClass &&
                        !t.IsAbstract &&
                        t.Name == implTypeName &&
                        interfaceType.IsAssignableFrom(t));

                if (implementationType != null)
                {
                    services.AddScoped(interfaceType, implementationType);
                }
            }
        }
    }
}
