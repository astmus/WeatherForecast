using MediatR;
using WeatherForecast.Application.Abstractions;
using WeatherForecast.Application.Exceptions;
using WeatherForecast.Application.Models;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Features.GetHourlyForecast;

public sealed class GetHourlyForecastQueryHandler : IRequestHandler<GetHourlyForecastQuery, IReadOnlyList<HourlyWeatherDto>>
{
	private readonly IWeatherForecastProvider _weatherForecastProvider;

	public GetHourlyForecastQueryHandler(IWeatherForecastProvider weatherForecastProvider)
	{
		_weatherForecastProvider = weatherForecastProvider;
	}

	public async Task<IReadOnlyList<HourlyWeatherDto>> Handle(GetHourlyForecastQuery request, CancellationToken cancellationToken)
	{
		WeatherSnapshot currentWeather = await _weatherForecastProvider.GetCurrentWeatherAsync(cancellationToken);
		IReadOnlyList<HourlyForecast> hourlyForecastSource = await _weatherForecastProvider.GetHourlyForecastAsync(cancellationToken);
		DateTime localTime = currentWeather.LocalTime;
		DateTime tomorrow = localTime.Date.AddDays(1);

		IReadOnlyList<HourlyWeatherDto> hourlyForecast = hourlyForecastSource
			.Where(hour => IsVisibleHour(hour.Time, localTime, tomorrow))
			.OrderBy(hour => hour.Time)
			.Select(hour => new HourlyWeatherDto
			{
				Time = hour.Time,
				TemperatureC = hour.TemperatureC,
				RainChancePercentage = hour.RainChancePercentage,
				Condition = hour.Condition.Text,
				IconUrl = hour.Condition.IconUrl
			})
			.ToArray();

			if (hourlyForecast.Count == 0)
			{
				throw new WeatherApplicationException("Поставщик погоды не вернул почасовой прогноз на оставшиеся часы дня и следующий день.", nameof(Handle));
			}

			return hourlyForecast;
	}

	private static bool IsVisibleHour(DateTime hourTime, DateTime localTime, DateTime tomorrow)
	{
		if (hourTime <= localTime)
		{
			return false;
		}

		return hourTime.Date == localTime.Date || hourTime.Date == tomorrow;
	}
}
