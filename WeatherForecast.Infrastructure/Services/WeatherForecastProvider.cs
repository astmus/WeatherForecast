using System.Globalization;
using Microsoft.Extensions.Options;
using WeatherForecast.Application.Abstractions;
using WeatherForecast.Domain.ValueObjects;
using WeatherForecast.Infrastructure.Exceptions;
using WeatherForecast.Infrastructure.Integration.WeatherApi;
using WeatherForecast.Infrastructure.Options;

namespace WeatherForecast.Infrastructure.Services;

public sealed class WeatherForecastProvider : IWeatherForecastProvider
{
	private const double MoscowLatitude = 55.7558;
	private const double MoscowLongitude = 37.6176;
	private const int ForecastDaysCount = 3;

	private readonly IWeatherApiClient _weatherApiClient;
	private readonly WeatherApiOptions _weatherApiOptions;

	public WeatherForecastProvider(IWeatherApiClient weatherApiClient, IOptions<WeatherApiOptions> weatherApiOptions)
	{
		_weatherApiClient = weatherApiClient;
		_weatherApiOptions = weatherApiOptions.Value;
	}

	public async Task<WeatherSnapshot> GetCurrentWeatherAsync(CancellationToken cancellationToken)
	{
		try
		{
			var locationQuery = BuildMoscowQuery();
			WeatherApiCurrentResponse currentResponse = await _weatherApiClient.GetCurrentWeatherAsync(
				_weatherApiOptions.ApiKey, locationQuery, _weatherApiOptions.Language, "no", cancellationToken);
			DateTime localTime = ParseWeatherApiDateTime(currentResponse.Location.LocalTime);

			return new WeatherSnapshot
			{
				LocationName = _weatherApiOptions.LocationName,
				LocalTime = localTime,
				Current = new CurrentWeather
				{
					TemperatureC = currentResponse.Current.TemperatureC,
					FeelsLikeC = currentResponse.Current.FeelsLikeC,
					HumidityPercentage = currentResponse.Current.Humidity,
					WindKph = currentResponse.Current.WindKph,
					LastUpdatedAt = ParseWeatherApiDateTime(currentResponse.Current.LastUpdated),
					IsDay = currentResponse.Current.IsDay == 1,
					Condition = new WeatherCondition
					{
						Text = currentResponse.Current.Condition.Text,
						IconUrl = NormalizeIconUrl(currentResponse.Current.Condition.Icon)
					}
				},
				ForecastDays = []
			};
		}
		catch (Exception exception)
		{
			throw new InfrastructureException("Не удалось получить текущую погоду из внешнего API.", exception, nameof(GetCurrentWeatherAsync));
		}
	}

	public async Task<IReadOnlyList<HourlyForecast>> GetHourlyForecastAsync(CancellationToken cancellationToken)
	{
		try
		{
			WeatherApiForecastResponse forecastResponse = await GetForecastResponseAsync(cancellationToken);

			return forecastResponse.Forecast.ForecastDays
				.SelectMany(forecastDay => forecastDay.Hours)
				.Select(hour => new HourlyForecast
				{
					Time = DateTime.Parse(hour.Time, CultureInfo.InvariantCulture),
					TemperatureC = hour.TemperatureC,
					RainChancePercentage = hour.ChanceOfRain,
					Condition = new WeatherCondition
					{
						Text = hour.Condition.Text,
						IconUrl = NormalizeIconUrl(hour.Condition.Icon)
					}
				})
				.ToArray();
		}
		catch (Exception exception)
		{
			throw new InfrastructureException("Не удалось получить почасовой прогноз из внешнего API.", exception, nameof(GetHourlyForecastAsync));
		}
	}

	public async Task<IReadOnlyList<ForecastDay>> GetDailyForecastAsync(CancellationToken cancellationToken)
	{
		try
		{
			WeatherApiForecastResponse forecastResponse = await GetForecastResponseAsync(cancellationToken);

			return forecastResponse.Forecast.ForecastDays
				.Select(MapForecastDayWithoutHours)
				.ToArray();
		}
		catch (Exception exception)
		{
			throw new InfrastructureException("Не удалось получить прогноз по дням из внешнего API.", exception, nameof(GetDailyForecastAsync));
		}
	}

	private async Task<WeatherApiForecastResponse> GetForecastResponseAsync(CancellationToken cancellationToken)
	{
		var locationQuery = BuildMoscowQuery();
		return await _weatherApiClient.GetForecastAsync(
			_weatherApiOptions.ApiKey,
			locationQuery,
			ForecastDaysCount,
			_weatherApiOptions.Language,
			"no",
			"no",
			cancellationToken);
	}

	private static ForecastDay MapForecastDayWithoutHours(WeatherApiForecastResponse.WeatherApiForecastDay forecastDay)
	{
		return new ForecastDay
		{
			Date = DateOnly.Parse(forecastDay.Date, CultureInfo.InvariantCulture),
			Summary = new DailyForecast
			{
				MaxTemperatureC = forecastDay.Day.MaxTemperatureC,
				MinTemperatureC = forecastDay.Day.MinTemperatureC,
				RainChancePercentage = forecastDay.Day.DailyChanceOfRain,
				Condition = new WeatherCondition
				{
					Text = forecastDay.Day.Condition.Text,
					IconUrl = NormalizeIconUrl(forecastDay.Day.Condition.Icon)
				}
			},
			Hours = []
		};
	}

	private static DateTime ParseWeatherApiDateTime(string value)
	{
		if (DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
			return result;

		if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
			return result;

		throw new FormatException($"WeatherAPI returned unsupported date format: '{value}'.");
	}

	private static string BuildMoscowQuery()
	{
		return string.Create(CultureInfo.InvariantCulture, $"{MoscowLatitude},{MoscowLongitude}");
	}

	private static string NormalizeIconUrl(string iconUrl)
	{
		if (string.IsNullOrWhiteSpace(iconUrl))
			return string.Empty;

		return iconUrl.StartsWith("//", StringComparison.Ordinal)
			? $"https:{iconUrl}"
			: iconUrl;
	}
}
