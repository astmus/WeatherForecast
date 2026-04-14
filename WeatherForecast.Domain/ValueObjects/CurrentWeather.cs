namespace WeatherForecast.Domain.ValueObjects;

public sealed class CurrentWeather
{
	public double TemperatureC { get; init; }

	public double FeelsLikeC { get; init; }

	public int HumidityPercentage { get; init; }

	public double WindKph { get; init; }

	public required DateTime LastUpdatedAt { get; init; }

	public bool IsDay { get; init; }

	public required WeatherCondition Condition { get; init; }
}
