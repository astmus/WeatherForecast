using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using WeatherForecast.Application.Abstractions;
using WeatherForecast.Infrastructure.Integration.WeatherApi;
using WeatherForecast.Infrastructure.Options;
using WeatherForecast.Infrastructure.Services;

namespace WeatherForecast.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services
			.AddOptions<WeatherApiOptions>()
			.Bind(configuration.GetSection(WeatherApiOptions.SectionName))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services
			.AddRefitClient<IWeatherApiClient>()
			.ConfigureHttpClient((serviceProvider, httpClient) =>
			{
				var options = serviceProvider.GetRequiredService<IOptions<WeatherApiOptions>>().Value;

				httpClient.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
				httpClient.Timeout = Timeout.InfiniteTimeSpan;
			})
			.AddStandardResilienceHandler(options =>
			{
				options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15);
				options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
				options.Retry.MaxRetryAttempts = 3;
				options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
				options.CircuitBreaker.MinimumThroughput = 10;
				options.CircuitBreaker.FailureRatio = 0.5;
				options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
			});

		services.AddScoped<IWeatherForecastProvider, WeatherForecastProvider>();

		return services;
	}
}
