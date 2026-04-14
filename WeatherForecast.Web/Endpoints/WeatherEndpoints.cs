using System.Net;
using Microsoft.AspNetCore.Mvc;
using Refit;
using ProblemDetails = Refit.ProblemDetails;
using WeatherForecast.Application.Models;
using WeatherForecast.Web.Clients;

namespace WeatherForecast.Web.Endpoints;

public static class WeatherEndpoints
{
	public static IEndpointRouteBuilder MapWeatherEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapGet("/api/weather/current", GetCurrentWeatherAsync)
			.WithName("GetCurrentWeather")
			.Produces<CurrentWeatherDto>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status500InternalServerError)
			.ProducesProblem(StatusCodes.Status502BadGateway)
			.ProducesProblem(StatusCodes.Status503ServiceUnavailable)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status429TooManyRequests);

		endpoints.MapGet("/api/weather/hourly", GetHourlyForecastAsync)
			.WithName("GetHourlyForecast")
			.Produces<IReadOnlyList<HourlyWeatherDto>>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status500InternalServerError)
			.ProducesProblem(StatusCodes.Status502BadGateway)
			.ProducesProblem(StatusCodes.Status503ServiceUnavailable)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status429TooManyRequests);

		endpoints.MapGet("/api/weather/daily", GetDailyForecastAsync)
			.WithName("GetDailyForecast")
			.Produces<IReadOnlyList<DailyWeatherDto>>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status500InternalServerError)
			.ProducesProblem(StatusCodes.Status502BadGateway)
			.ProducesProblem(StatusCodes.Status503ServiceUnavailable)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status429TooManyRequests);

		return endpoints;
	}

	private static Task<IResult> GetCurrentWeatherAsync(IWeatherApiClient weatherApiClient, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
	{
		return ExecuteAsync(() => weatherApiClient.GetCurrentWeatherAsync(cancellationToken), loggerFactory, cancellationToken);
	}

	private static Task<IResult> GetHourlyForecastAsync(IWeatherApiClient weatherApiClient, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
	{
		return ExecuteAsync(() => weatherApiClient.GetHourlyForecastAsync(cancellationToken), loggerFactory, cancellationToken);
	}

	private static Task<IResult> GetDailyForecastAsync(IWeatherApiClient weatherApiClient, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
	{
		return ExecuteAsync(() => weatherApiClient.GetDailyForecastAsync(cancellationToken), loggerFactory, cancellationToken);
	}

	private static async Task<IResult> ExecuteAsync<TResponse>(Func<Task<TResponse>> request, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
	{
		var logger = loggerFactory.CreateLogger("WeatherEndpoints");

		try
		{
			TResponse response = await request();
			return Results.Ok(response);
		}
		catch (ApiException exception)
		{
			logger.LogWarning(exception, "Weather API returned status code {StatusCode}.", (int)exception.StatusCode);
			return await CreateProblemResultAsync(exception, cancellationToken);
		}
		catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
		{
			logger.LogWarning(exception, "Weather API request timed out.");

			return Results.Problem(
				title: "Погода временно недоступна",
				detail: "API погоды не ответил вовремя. Попробуйте повторить запрос через несколько секунд.",
				statusCode: StatusCodes.Status503ServiceUnavailable);
		}
		catch (HttpRequestException exception)
		{
			logger.LogError(exception, "Weather API request failed.");

			return Results.Problem(
				title: "Погода временно недоступна",
				detail: "Не удалось связаться с API погоды.",
				statusCode: StatusCodes.Status503ServiceUnavailable);
		}
	}

	private static async Task<IResult> CreateProblemResultAsync(ApiException exception, CancellationToken cancellationToken)
	{
		ProblemDetails? problem = null;

		if (exception.HasContent)
		{
			try
			{
				problem = await exception.GetContentAsAsync<ProblemDetails>();
			}
			catch
			{
				problem = null;
			}
		}

		var statusCode = exception.StatusCode switch
		{
			HttpStatusCode.BadGateway => StatusCodes.Status502BadGateway,
			HttpStatusCode.ServiceUnavailable => StatusCodes.Status503ServiceUnavailable,
			HttpStatusCode.GatewayTimeout => StatusCodes.Status503ServiceUnavailable,
			_ => (int)exception.StatusCode is >= 400 and <= 599
				? (int)exception.StatusCode
				: StatusCodes.Status502BadGateway
		};

		return Results.Problem(
			title: problem?.Title ?? "Не удалось получить прогноз",
			detail: problem?.Detail ?? "API погоды вернул ошибку.",
			statusCode: statusCode);
	}
}
