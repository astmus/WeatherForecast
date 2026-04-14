using MediatR;
using WeatherForecast.Application.Models;

namespace WeatherForecast.Application.Features.GetWeatherDashboard;

public sealed record GetWeatherDashboardQuery : IRequest<WeatherDashboardDto>;
