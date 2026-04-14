namespace WeatherForecast.Application.Models;

public sealed class DailyWeatherDto
{
	public required DateOnly Date { get; init; }

	public double MaxTemperatureC { get; init; }

	public double MinTemperatureC { get; init; }

	public int RainChancePercentage { get; init; }

	public required string Condition { get; init; }

	public required string IconUrl { get; init; }
}
