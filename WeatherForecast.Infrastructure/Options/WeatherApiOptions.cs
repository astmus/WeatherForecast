using System.ComponentModel.DataAnnotations;

namespace WeatherForecast.Infrastructure.Options;

public sealed class WeatherApiOptions
{
	public const string SectionName = "WeatherApi";

	[Required]
	public string BaseUrl { get; init; } = "https://api.weatherapi.com/v1/";

	[Required]
	public string ApiKey { get; init; } = string.Empty;

	[Required]
	public string Language { get; init; } = "ru";

	[Required]
	public string LocationName { get; init; } = "Москва";
}
