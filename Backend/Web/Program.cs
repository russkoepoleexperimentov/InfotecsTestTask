

using Application.Services;
using Application.Services.Implementations;

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

            builder.Services.AddTransient<ICsvAnalysisService, CsvAnalysisService>();

            var app = builder.Build();

            app.UseSwagger(); 
            app.UseSwaggerUI();
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
