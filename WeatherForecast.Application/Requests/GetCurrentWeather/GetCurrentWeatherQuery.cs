using MediatR;
using WeatherForecast.Application.Models;

namespace WeatherForecast.Application.Features.GetCurrentWeather;

public sealed record GetCurrentWeatherQuery : IRequest<CurrentWeatherDto>;
