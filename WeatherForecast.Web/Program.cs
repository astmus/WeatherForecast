using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Refit;
using WeatherForecast.Web.Clients;
using WeatherForecast.Web.Endpoints;
using WeatherForecast.Web.Options;

namespace WeatherForecast.Web;

public static class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		var services = builder.Services;

		services.AddOptions<WeatherApiClientOptions>()
			.Bind(builder.Configuration.GetSection(WeatherApiClientOptions.SectionName))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services
			.AddRefitClient<IWeatherApiClient>()
			.ConfigureHttpClient((serviceProvider, httpClient) =>
			{
				var options = serviceProvider.GetRequiredService<IOptions<WeatherApiClientOptions>>().Value;

				httpClient.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
			})
			.AddResilienceHandler("weather-api", (pipelineBuilder, _) =>
			{
				pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(10));
				pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
				{
					MaxRetryAttempts = 3,
					BackoffType = DelayBackoffType.Exponential,
					UseJitter = true,
					Delay = TimeSpan.FromMilliseconds(300)
				});
				pipelineBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
				{
					SamplingDuration = TimeSpan.FromSeconds(30),
					MinimumThroughput = 6,
					FailureRatio = 0.5,
					BreakDuration = TimeSpan.FromSeconds(20)
				});
			});

		services.AddProblemDetails();
		services.AddRazorPages();

		var app = builder.Build();

		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error");
			app.UseHsts();
		}
		else
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseStaticFiles();
		app.UseRouting();
		app.UseAuthorization();
		app.MapRazorPages();
		app.MapWeatherEndpoints();
		app.Run();
	}
}
