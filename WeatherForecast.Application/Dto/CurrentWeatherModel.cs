namespace WeatherForecast.Application.Models;

public sealed class CurrentWeatherModel
{
	public double TemperatureC { get; init; }

	public double FeelsLikeC { get; init; }

	public int HumidityPercentage { get; init; }

	public double WindKph { get; init; }

	public required DateTime LastUpdatedAt { get; init; }

	public bool IsDay { get; init; }

	public required string Condition { get; init; }

	public required string IconUrl { get; init; }
}
