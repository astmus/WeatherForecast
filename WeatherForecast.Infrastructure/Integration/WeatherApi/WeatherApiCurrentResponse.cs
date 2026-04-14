using System.Text.Json.Serialization;

namespace WeatherForecast.Infrastructure.Integration.WeatherApi;

public sealed class WeatherApiCurrentResponse
{
	[JsonPropertyName("location")]
	public required WeatherApiLocation Location { get; init; }

	[JsonPropertyName("current")]
	public required WeatherApiCurrent Current { get; init; }

	public sealed class WeatherApiLocation
	{
		[JsonPropertyName("localtime")]
		public required string LocalTime { get; init; }
	}

	public sealed class WeatherApiCurrent
	{
		[JsonPropertyName("temp_c")]
		public double TemperatureC { get; init; }

		[JsonPropertyName("feelslike_c")]
		public double FeelsLikeC { get; init; }

		[JsonPropertyName("humidity")]
		public int Humidity { get; init; }

		[JsonPropertyName("wind_kph")]
		public double WindKph { get; init; }

		[JsonPropertyName("last_updated")]
		public required string LastUpdated { get; init; }

		[JsonPropertyName("is_day")]
		public int IsDay { get; init; }

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
