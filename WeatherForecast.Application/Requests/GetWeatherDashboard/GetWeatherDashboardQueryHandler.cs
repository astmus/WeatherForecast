using MediatR;
using WeatherForecast.Application.Abstractions;
using WeatherForecast.Application.Exceptions;
using WeatherForecast.Application.Models;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Features.GetWeatherDashboard;

public sealed class GetWeatherDashboardQueryHandler : IRequestHandler<GetWeatherDashboardQuery, WeatherDashboardDto>
{
	private readonly IWeatherForecastProvider _weatherForecastProvider;

	public GetWeatherDashboardQueryHandler(IWeatherForecastProvider weatherForecastProvider)
	{
		_weatherForecastProvider = weatherForecastProvider;
	}

	public async Task<WeatherDashboardDto> Handle(GetWeatherDashboardQuery request, CancellationToken cancellationToken)
	{
		Task<WeatherSnapshot> currentWeatherTask = _weatherForecastProvider.GetCurrentWeatherAsync(cancellationToken);
		Task<IReadOnlyList<HourlyForecast>> hourlyForecastTask = _weatherForecastProvider.GetHourlyForecastAsync(cancellationToken);
		Task<IReadOnlyList<ForecastDay>> dailyForecastTask = _weatherForecastProvider.GetDailyForecastAsync(cancellationToken);

		await Task.WhenAll(currentWeatherTask, hourlyForecastTask, dailyForecastTask);

		WeatherSnapshot weatherSnapshot = await currentWeatherTask;
		IReadOnlyList<HourlyForecast> hourlyForecastSource = await hourlyForecastTask;
		IReadOnlyList<ForecastDay> dailyForecastSource = await dailyForecastTask;
		DateTime localTime = weatherSnapshot.LocalTime;
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
			}).ToArray();

		if (hourlyForecast.Count == 0)
			throw new WeatherApplicationException("Поставщик погоды не вернул почасовой прогноз на оставшиеся часы дня и следующий день.",
				nameof(Handle));

		IReadOnlyList<DailyWeatherDto> dailyForecast = dailyForecastSource
			.OrderBy(forecastDay => forecastDay.Date)
			.Take(3)
			.Select(forecastDay => new DailyWeatherDto
			{
				Date = forecastDay.Date,
				MaxTemperatureC = forecastDay.Summary.MaxTemperatureC,
				MinTemperatureC = forecastDay.Summary.MinTemperatureC,
				RainChancePercentage = forecastDay.Summary.RainChancePercentage,
				Condition = forecastDay.Summary.Condition.Text,
				IconUrl = forecastDay.Summary.Condition.IconUrl
			}).ToArray();

		if (dailyForecast.Count < 3)
			throw new WeatherApplicationException("Поставщик погоды вернул прогноз менее чем на три дня.", nameof(Handle));

		return new WeatherDashboardDto
		{
			LocationName = weatherSnapshot.LocationName,
			LocalTime = weatherSnapshot.LocalTime,
			Current = new CurrentWeatherModel
			{
				TemperatureC = weatherSnapshot.Current.TemperatureC,
				FeelsLikeC = weatherSnapshot.Current.FeelsLikeC,
				HumidityPercentage = weatherSnapshot.Current.HumidityPercentage,
				WindKph = weatherSnapshot.Current.WindKph,
				LastUpdatedAt = weatherSnapshot.Current.LastUpdatedAt,
				IsDay = weatherSnapshot.Current.IsDay,
				Condition = weatherSnapshot.Current.Condition.Text,
				IconUrl = weatherSnapshot.Current.Condition.IconUrl
			},
			HourlyForecast = hourlyForecast,
			DailyForecast = dailyForecast
		};
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
