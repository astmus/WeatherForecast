using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;
using WeatherForecast.Api.Infrastructure;
using WeatherForecast.Application;
using WeatherForecast.Infrastructure;

namespace WeatherForecast.Api;

public static class Program
{
	private const int RequestsPerMinuteLimit = 60;

	public static void Main(string[] args)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		IServiceCollection services = builder.Services;
		services.AddRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
			options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
			{
				string partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

				return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
				{
					PermitLimit = RequestsPerMinuteLimit,
					Window = TimeSpan.FromMinutes(1),
					QueueLimit = 0,
					AutoReplenishment = true
				});
			});
		});

		services.AddExceptionHandler<GlobalExceptionHandler>();
		services.AddProblemDetails();
		services.AddControllers();
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(options =>
		{
			string xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFileName);

			if (File.Exists(xmlPath))
				options.IncludeXmlComments(xmlPath);

			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "WeatherForecast.Api",
				Version = "v1"
			});

		});

		services.AddApplication();
		services.AddInfrastructure(builder.Configuration);

		WebApplication app = builder.Build();

		app.UseExceptionHandler();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseRateLimiter();
		app.MapControllers();
		app.Run();
	}
}
