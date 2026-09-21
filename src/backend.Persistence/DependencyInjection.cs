using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using backend.Application.Common.Interfaces;
using backend.Persistence.Context;

namespace backend.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var databaseUrl = configuration["DATABASE_URL"];

            if (string.IsNullOrWhiteSpace(databaseUrl))
            {
                throw new InvalidOperationException(
                    "DATABASE_URL is not configured.");
            }

            var uri = new Uri(databaseUrl);

            var userInfo = uri.UserInfo.Split(':', 2);

            if (userInfo.Length != 2)
            {
                throw new InvalidOperationException(
                    "DATABASE_URL has an invalid format.");
            }

            var username = Uri.UnescapeDataString(userInfo[0]);
            var password = Uri.UnescapeDataString(userInfo[1]);

            var connectionString = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = uri.AbsolutePath.Trim('/'),
                Username = username,
                Password = password,
                SslMode = SslMode.Require
            }.ConnectionString;

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IApplicationDbContext>(
                sp => sp.GetRequiredService<ApplicationDbContext>());

            return services;
        }
    }
}
