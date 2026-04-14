using MediatR;
using WeatherForecast.Application.Abstractions;
using WeatherForecast.Application.Exceptions;
using WeatherForecast.Application.Models;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Features.GetDailyForecast;

public sealed class GetDailyForecastQueryHandler : IRequestHandler<GetDailyForecastQuery, IReadOnlyList<DailyWeatherDto>>
{
	private readonly IWeatherForecastProvider _weatherForecastProvider;

	public GetDailyForecastQueryHandler(IWeatherForecastProvider weatherForecastProvider)
	{
		_weatherForecastProvider = weatherForecastProvider;
	}

	public async Task<IReadOnlyList<DailyWeatherDto>> Handle(GetDailyForecastQuery request, CancellationToken cancellationToken)
	{
		IReadOnlyList<ForecastDay> dailyForecastSource = await _weatherForecastProvider.GetDailyForecastAsync(cancellationToken);
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
			})
			.ToArray();

		if (dailyForecast.Count < 3)
		{
			throw new WeatherApplicationException("Поставщик погоды вернул прогноз менее чем на три дня.", nameof(Handle));
		}

		return dailyForecast;
	}
}
