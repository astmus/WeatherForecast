using MediatR;
using WeatherForecast.Application.Models;

namespace WeatherForecast.Application.Features.GetHourlyForecast;

public sealed record GetHourlyForecastQuery : IRequest<IReadOnlyList<HourlyWeatherDto>>;
