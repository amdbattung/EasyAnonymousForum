using Microsoft.EntityFrameworkCore;
using Npgsql;
using EasyAnonymousForum.Data;

namespace EasyAnonymousForum.Extensions.ServiceExtensions
{
    public static class DataContextServiceExtension
    {
        public static IServiceCollection AddDataContext(this IServiceCollection services, IConfiguration configuration)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("EasyAnonymousForumContext"));
            dataSourceBuilder.UseNodaTime();
            var dataSource = dataSourceBuilder.Build();
            services.AddDbContext<DataContext>(options =>
            {
                options.UseNpgsql(dataSource, o => o.UseNodaTime());
            });

            return services;
        }
    }
}
