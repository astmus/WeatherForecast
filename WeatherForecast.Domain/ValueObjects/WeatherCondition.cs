namespace WeatherForecast.Domain.ValueObjects;

public sealed record WeatherCondition
{
	public required string Text { get; init; }

	public required string IconUrl { get; init; }
}
