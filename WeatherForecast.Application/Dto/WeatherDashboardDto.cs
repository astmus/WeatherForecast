namespace WeatherForecast.Application.Models;

public sealed class WeatherDashboardDto
{
	public required string LocationName { get; init; }

	public required DateTime LocalTime { get; init; }

	public required CurrentWeatherModel Current { get; init; }

	public required IReadOnlyList<HourlyWeatherDto> HourlyForecast { get; init; }

	public required IReadOnlyList<DailyWeatherDto> DailyForecast { get; init; }

}
