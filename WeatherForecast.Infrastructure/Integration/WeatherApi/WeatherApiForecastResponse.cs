using System.Text.Json.Serialization;

namespace WeatherForecast.Infrastructure.Integration.WeatherApi;

public sealed class WeatherApiForecastResponse
{
	[JsonPropertyName("forecast")]
	public required WeatherApiForecast Forecast { get; init; }

	public sealed class WeatherApiForecast
	{
		[JsonPropertyName("forecastday")]
		public required WeatherApiForecastDay[] ForecastDays { get; init; }
	}

	public sealed class WeatherApiForecastDay
	{
		[JsonPropertyName("date")]
		public required string Date { get; init; }

		[JsonPropertyName("day")]
		public required WeatherApiDay Day { get; init; }

		[JsonPropertyName("hour")]
		public required WeatherApiHour[] Hours { get; init; }
	}

	public sealed class WeatherApiDay
	{
		[JsonPropertyName("maxtemp_c")]
		public double MaxTemperatureC { get; init; }

		[JsonPropertyName("mintemp_c")]
		public double MinTemperatureC { get; init; }

		[JsonPropertyName("daily_chance_of_rain")]
		public int DailyChanceOfRain { get; init; }

		[JsonPropertyName("condition")]
		public required WeatherApiCondition Condition { get; init; }
	}

	public sealed class WeatherApiHour
	{
		[JsonPropertyName("time")]
		public required string Time { get; init; }

		[JsonPropertyName("temp_c")]
		public double TemperatureC { get; init; }

		[JsonPropertyName("chance_of_rain")]
		public int ChanceOfRain { get; init; }

		[JsonPropertyName("condition")]
		public required WeatherApiCondition Condition { get; init; }
	}

	public sealed class WeatherApiCondition
	{
		[JsonPropertyName("text")]
		public required string Text { get; init; }

		[JsonPropertyName("icon")]
		public required string Icon { get; init; }
	}
}
