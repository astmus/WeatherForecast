using System.Text.Json.Serialization;

namespace WeatherForecast.Infrastructure.Integration.WeatherApi;

public sealed class WeatherApiErrorResponse
{
	[JsonPropertyName("error")]
	public WeatherApiError? Error { get; init; }

	public sealed class WeatherApiError
	{
		[JsonPropertyName("message")]
		public string? Message { get; init; }
	}
}
