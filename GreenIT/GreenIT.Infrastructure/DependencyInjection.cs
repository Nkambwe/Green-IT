using GreenIT.Infrastructure.Data;
using GreenIT.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GreenIT.Infrastructure {
    public static class DependencyInjection {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration) {
            var connectionString = configuration
                .GetConnectionString("GreenITConnection");

            if (string.IsNullOrWhiteSpace(connectionString)) {
                throw new InvalidOperationException(
                    "GreenIT database connection string is missing.");
            }

            services.AddDbContext<GreenITDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    sql => sql
                        .UseCompatibilityLevel(130)
                        .EnableRetryOnFailure()
                )
            );

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<GreenITDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }
    }
}