using Refit;
using WeatherForecast.Application.Models;

namespace WeatherForecast.Web.Clients;

public interface IWeatherApiClient
{
	[Get("/api/weather/current")]
	Task<CurrentWeatherDto> GetCurrentWeatherAsync(CancellationToken cancellationToken);

	[Get("/api/weather/hourly")]
	Task<IReadOnlyList<HourlyWeatherDto>> GetHourlyForecastAsync(CancellationToken cancellationToken);

	[Get("/api/weather/daily")]
	Task<IReadOnlyList<DailyWeatherDto>> GetDailyForecastAsync(CancellationToken cancellationToken);
}
