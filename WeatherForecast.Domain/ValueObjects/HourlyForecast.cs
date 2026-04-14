namespace WeatherForecast.Domain.ValueObjects;

public sealed class HourlyForecast
{
	public required DateTime Time { get; init; }

	public double TemperatureC { get; init; }          

	public int RainChancePercentage { get; init; } 
               
	public required WeatherCondition Condition { get; init; }
}
