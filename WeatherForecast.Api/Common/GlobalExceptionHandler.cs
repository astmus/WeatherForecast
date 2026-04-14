using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WeatherForecast.Infrastructure.Exceptions;
using WeatherForecast.Application.Exceptions;

namespace WeatherForecast.Api.Infrastructure;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
	private readonly ILogger<GlobalExceptionHandler> _logger;

	public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
	{
		_logger = logger;
	}

	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		_logger.LogError(exception, "Unhandled exception");

		var problem = exception switch
		{
			InfrastructureException ex => new ProblemDetails
			{
				Title = "Погода временно недоступна",
				Instance = "Внешний погодный сервис сейчас недоступен. Попробуйте повторить запрос позже.",
				Detail = ex.Message,
				Status = StatusCodes.Status503ServiceUnavailable,
				Type = "https://httpstatuses.com/503"
			},
			WeatherApplicationException ex => new ProblemDetails
			{
				Title = "Не удалось собрать погодный дашборд",
				Instance = "Погодный сервис вернул неполные или некорректные данные.",
				Detail = ex.Message,
				Status = StatusCodes.Status502BadGateway,
				Type = "https://httpstatuses.com/502"
			},
			ArgumentException ex => new ProblemDetails
			{
				Title = "Некорректный запрос",
				Detail = ex.Message,
				Status = StatusCodes.Status400BadRequest,
				Type = "https://httpstatuses.com/400"
			},

			_ => new ProblemDetails
			{
				Title = "Внутренняя ошибка сервера",
				Detail = "Произошла непредвиденная ошибка.",
				Status = StatusCodes.Status500InternalServerError,
				Type = "https://httpstatuses.com/500"
			}
		};

		httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

		await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
		return true;
	}
}
