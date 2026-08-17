

using Application.Persistence;
using Application.Services;
using Application.Services.Implementations;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

            builder.Services.AddScoped<ICsvAnalysisService, CsvAnalysisService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
                scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

            app.UseSwagger(); 
            app.UseSwaggerUI();
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
