namespace WeatherForecast.Application.Models;

public sealed class CurrentWeatherDto
{
	public required string LocationName { get; init; }

	public required DateTime LocalTime { get; init; }

	public required CurrentWeatherModel Current { get; init; }
}
