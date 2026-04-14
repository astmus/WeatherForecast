namespace WeatherForecast.Domain.ValueObjects;

public sealed class DailyForecast
{
	public double MaxTemperatureC { get; init; }

	public double MinTemperatureC { get; init; }

	public int RainChancePercentage { get; init; }

	public required WeatherCondition Condition { get; init; }
}
