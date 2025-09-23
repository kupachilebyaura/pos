using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace POS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registro de ApplicationDbContext (por si quieres hacerlo aquí)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Aquí puedes registrar otros servicios de infraestructura, repositorios, etc.
            // Ejemplo: services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}