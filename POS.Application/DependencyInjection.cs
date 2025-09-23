using Microsoft.Extensions.DependencyInjection;

namespace POS.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Aquí puedes registrar servicios de la capa Application
            // Ejemplo: services.AddTransient<IUserService, UserService>();

            return services;
        }
    }
}