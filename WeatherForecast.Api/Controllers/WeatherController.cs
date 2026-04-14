using MediatR;
using Microsoft.AspNetCore.Mvc;
using WeatherForecast.Application.Features.GetCurrentWeather;
using WeatherForecast.Application.Features.GetDailyForecast;
using WeatherForecast.Application.Features.GetWeatherDashboard;
using WeatherForecast.Application.Features.GetHourlyForecast;
using WeatherForecast.Application.Models;

namespace WeatherForecast.Api.Controllers;

[ApiController]
[Route("api/weather")]
[Produces("application/json")]
public sealed class WeatherController : ControllerBase
{
	private readonly ISender _sender;

	public WeatherController(ISender sender)
	{
		_sender = sender;
	}

	/// <summary>
	/// Возвращает текущую погоду, почасовой прогноз на остаток дня и следующий день, а также прогноз на три дня.
	/// Геолокация фиксирована на Москве.
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(WeatherDashboardDto), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<WeatherDashboardDto>> GetAsync(CancellationToken cancellationToken)
	{
		WeatherDashboardDto response = await _sender.Send(new GetWeatherDashboardQuery(), cancellationToken);
		return Ok(response);
	}

	/// <summary>
	/// Возвращает текущую погоду и локальное время.
	/// </summary>
	[HttpGet("current")]
	[ProducesResponseType(typeof(CurrentWeatherDto), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<CurrentWeatherDto>> GetCurrentAsync(CancellationToken cancellationToken)
	{
		CurrentWeatherDto response = await _sender.Send(new GetCurrentWeatherQuery(), cancellationToken);
		return Ok(response);
	}

	/// <summary>
	/// Возвращает почасовой прогноз на остаток дня и следующий день.
	/// </summary>
	[HttpGet("hourly")]
	[ProducesResponseType(typeof(IReadOnlyList<HourlyWeatherDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<IReadOnlyList<HourlyWeatherDto>>> GetHourlyAsync(CancellationToken cancellationToken)
	{
		IReadOnlyList<HourlyWeatherDto> response = await _sender.Send(new GetHourlyForecastQuery(), cancellationToken);
		return Ok(response);
	}

	/// <summary>
	/// Возвращает прогноз по дням на три дня.
	/// </summary>
	[HttpGet("daily")]
	[ProducesResponseType(typeof(IReadOnlyList<DailyWeatherDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
	[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
	public async Task<ActionResult<IReadOnlyList<DailyWeatherDto>>> GetDailyAsync(CancellationToken cancellationToken)
	{
		IReadOnlyList<DailyWeatherDto> response = await _sender.Send(new GetDailyForecastQuery(), cancellationToken);
		return Ok(response);
	}
}
