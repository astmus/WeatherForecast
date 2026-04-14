using Refit;

namespace WeatherForecast.Infrastructure.Integration.WeatherApi;

public interface IWeatherApiClient
{
	[Get("/current.json")]
	Task<WeatherApiCurrentResponse> GetCurrentWeatherAsync(
		[AliasAs("key")] string apiKey,
		[AliasAs("q")] string query,
		[AliasAs("lang")] string language,
		[AliasAs("aqi")] string aqi,
		CancellationToken cancellationToken = default);

	[Get("/forecast.json")]
	Task<WeatherApiForecastResponse> GetForecastAsync(
		[AliasAs("key")] string apiKey,
		[AliasAs("q")] string query,
		[AliasAs("days")] int days,
		[AliasAs("lang")] string language,
		[AliasAs("aqi")] string aqi,
		[AliasAs("alerts")] string alerts,
		CancellationToken cancellationToken = default);
}
