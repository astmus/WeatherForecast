namespace WeatherForecast.Domain.ValueObjects;

public sealed class ForecastDay
{
	public required DateOnly Date { get; init; }

	public required DailyForecast Summary { get; init; }

	public required IReadOnlyCollection<HourlyForecast> Hours { get; init; }
}
