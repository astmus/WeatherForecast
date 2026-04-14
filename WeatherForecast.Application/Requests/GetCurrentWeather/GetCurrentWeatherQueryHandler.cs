using MediatR;
using WeatherForecast.Application.Abstractions;
using WeatherForecast.Application.Models;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Application.Features.GetCurrentWeather;

public sealed class GetCurrentWeatherQueryHandler : IRequestHandler<GetCurrentWeatherQuery, CurrentWeatherDto>
{
	private readonly IWeatherForecastProvider _weatherForecastProvider;

	public GetCurrentWeatherQueryHandler(IWeatherForecastProvider weatherForecastProvider)
	{
		_weatherForecastProvider = weatherForecastProvider;
	}

	public async Task<CurrentWeatherDto> Handle(GetCurrentWeatherQuery request, CancellationToken cancellationToken)
	{
		WeatherSnapshot weatherSnapshot = await _weatherForecastProvider.GetCurrentWeatherAsync(cancellationToken);

		return new CurrentWeatherDto
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
			}
		};
	}
}
