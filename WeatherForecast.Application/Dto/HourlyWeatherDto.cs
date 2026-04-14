namespace WeatherForecast.Application.Models;

public sealed class HourlyWeatherDto
{
	public required DateTime Time { get; init; }

	public double TemperatureC { get; init; }

	public int RainChancePercentage { get; init; }

	public required string Condition { get; init; }

	public required string IconUrl { get; init; }
}
