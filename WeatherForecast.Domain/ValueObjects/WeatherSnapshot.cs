namespace WeatherForecast.Domain.ValueObjects;

public sealed class WeatherSnapshot
{
	public required string LocationName { get; init; }

	public required DateTime LocalTime { get; init; }

	public required CurrentWeather Current { get; init; }

	public required IReadOnlyCollection<ForecastDay> ForecastDays { get; init; }
}
