using System.ComponentModel.DataAnnotations;

namespace WeatherForecast.Web.Options;

public sealed class WeatherApiClientOptions
{
	public const string SectionName = "WeatherApiClient";

	[Required]
	public string BaseUrl { get; init; } = "http://localhost:5181";
}
