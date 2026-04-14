using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Abstractions;

public interface IWeatherForecastProvider
{
	Task<WeatherSnapshot> GetCurrentWeatherAsync(CancellationToken cancellationToken);

	Task<IReadOnlyList<HourlyForecast>> GetHourlyForecastAsync(CancellationToken cancellationToken);

	Task<IReadOnlyList<ForecastDay>> GetDailyForecastAsync(CancellationToken cancellationToken);
}
