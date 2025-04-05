using LayeredAppTemplate.Application;
using LayeredAppTemplate.Application.Common.Interfaces;
using LayeredAppTemplate.Application.Interfaces;
using LayeredAppTemplate.Application.Services;
using LayeredAppTemplate.Infrastructure.Caching;
using LayeredAppTemplate.Persistence;
using LayeredAppTemplate.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LayeredAppTemplate.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, string connectionString)
        {
            // DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString)); // veya UseNpgsql, UseSqlite

            // Generic Repository
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

            // User Service (override edilen)
            services.AddScoped<IUserService, UserService>();

            // AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));

            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();

            return services;
        }
    }
}
