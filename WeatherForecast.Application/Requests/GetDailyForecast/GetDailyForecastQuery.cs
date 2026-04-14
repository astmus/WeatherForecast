using MediatR;
using WeatherForecast.Application.Models;

namespace WeatherForecast.Application.Features.GetDailyForecast;

public sealed record GetDailyForecastQuery : IRequest<IReadOnlyList<DailyWeatherDto>>;
