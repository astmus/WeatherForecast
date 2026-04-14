using WeatherForecast.Application.Abstractions;
using WeatherForecast.Application.Exceptions;
using WeatherForecast.Application.Features.GetWeatherDashboard;
using WeatherForecast.Application.Models;
using WeatherForecast.Domain.ValueObjects;

namespace WeatherForecast.Tests;

public sealed class GetWeatherDashboardQueryHandlerTests
{
	[Fact]
	public async Task Handle_ReturnsAggregatedDashboard_WhenSnapshotIsValid()
	{
		WeatherSnapshot snapshot = CreateSnapshot();
		GetWeatherDashboardQueryHandler handler = new(new FakeWeatherForecastProvider(snapshot));

		WeatherDashboardDto result = await handler.Handle(new GetWeatherDashboardQuery(), CancellationToken.None);

		Assert.Equal("Москва", result.LocationName);
		Assert.Equal(snapshot.LocalTime, result.LocalTime);
		Assert.Equal(snapshot.Current.TemperatureC, result.Current.TemperatureC);
		Assert.Equal(3, result.DailyForecast.Count);
		Assert.Equal(3, result.HourlyForecast.Count);
	}

	[Fact]
	public async Task Handle_FiltersOnlyRemainingTodayAndWholeTomorrow()
	{
		WeatherSnapshot snapshot = CreateSnapshot();
		GetWeatherDashboardQueryHandler handler = new(new FakeWeatherForecastProvider(snapshot));

		WeatherDashboardDto result = await handler.Handle(new GetWeatherDashboardQuery(), CancellationToken.None);
		DateTime[] actualHours = result.HourlyForecast.Select(hour => hour.Time).ToArray();
		DateTime[] expectedHours =
		[
			new DateTime(2026, 4, 13, 22, 0, 0),
			new DateTime(2026, 4, 14, 9, 0, 0),
			new DateTime(2026, 4, 14, 18, 0, 0)
		];

		Assert.True(expectedHours.SequenceEqual(actualHours));
	}

	[Fact]
	public async Task Handle_ThrowsWeatherApplicationException_WhenThreeDayForecastIsIncomplete()
	{
		WeatherSnapshot sourceSnapshot = CreateSnapshot();
		WeatherSnapshot incompleteSnapshot = new()
		{
			LocationName = sourceSnapshot.LocationName,
			LocalTime = sourceSnapshot.LocalTime,
			Current = sourceSnapshot.Current,
			ForecastDays = sourceSnapshot.ForecastDays.Take(2).ToArray()
		};
		GetWeatherDashboardQueryHandler handler = new(new FakeWeatherForecastProvider(incompleteSnapshot));

		await Assert.ThrowsAsync<WeatherApplicationException>(() => handler.Handle(new GetWeatherDashboardQuery(), CancellationToken.None));
	}

	private static WeatherSnapshot CreateSnapshot()
	{
		return new WeatherSnapshot
		{
			LocationName = "Москва",
			LocalTime = new DateTime(2026, 4, 13, 17, 35, 0),
			Current = new CurrentWeather
			{
				TemperatureC = 12,
				FeelsLikeC = 10,
				HumidityPercentage = 67,
				WindKph = 18,
				LastUpdatedAt = new DateTime(2026, 4, 13, 17, 20, 0),
				IsDay = true,
				Condition = new WeatherCondition
				{
					Text = "Переменная облачность",
					IconUrl = "https://cdn.weatherapi.com/weather/64x64/day/116.png"
				}
			},
			ForecastDays =
			[
				new ForecastDay
				{
					Date = new DateOnly(2026, 4, 13),
					Summary = CreateDailyForecast(13, 7, 20, "Облачно"),
					Hours =
					[
						CreateHourlyForecast(2026, 4, 13, 15, 9, 10, "Пасмурно"),
						CreateHourlyForecast(2026, 4, 13, 17, 12, 15, "Переменная облачность"),
						CreateHourlyForecast(2026, 4, 13, 22, 8, 28, "Небольшой дождь")
					]
				},
				new ForecastDay
				{
					Date = new DateOnly(2026, 4, 14),
					Summary = CreateDailyForecast(14, 6, 35, "Небольшой дождь"),
					Hours =
					[
						CreateHourlyForecast(2026, 4, 14, 9, 11, 22, "Облачно"),
						CreateHourlyForecast(2026, 4, 14, 18, 14, 37, "Дождь")
					]
				},
				new ForecastDay
				{
					Date = new DateOnly(2026, 4, 15),
					Summary = CreateDailyForecast(16, 8, 12, "Ясно"),
					Hours =
					[
						CreateHourlyForecast(2026, 4, 15, 10, 15, 10, "Солнечно")
					]
				}
			]
		};
	}

	private static DailyForecast CreateDailyForecast(double maxTemperature, double minTemperature, int rainChancePercentage, string condition)
	{
		return new DailyForecast
		{
			MaxTemperatureC = maxTemperature,
			MinTemperatureC = minTemperature,
			RainChancePercentage = rainChancePercentage,
			Condition = new WeatherCondition
			{
				Text = condition,
				IconUrl = "https://example.test/icon.png"
			}
		};
	}

	private static HourlyForecast CreateHourlyForecast(int year, int month, int day, int hour, double temperature, int rainChancePercentage, string condition)
	{
		return new HourlyForecast
		{
			Time = new DateTime(year, month, day, hour, 0, 0),
			TemperatureC = temperature,
			RainChancePercentage = rainChancePercentage,
			Condition = new WeatherCondition
			{
				Text = condition,
				IconUrl = "https://example.test/icon.png"
			}
		};
	}

	private sealed class FakeWeatherForecastProvider : IWeatherForecastProvider
	{
		private readonly WeatherSnapshot _weatherSnapshot;

		public FakeWeatherForecastProvider(WeatherSnapshot weatherSnapshot)
		{
			_weatherSnapshot = weatherSnapshot;
		}

		public Task<WeatherSnapshot> GetCurrentWeatherAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult(new WeatherSnapshot
			{
				LocationName = _weatherSnapshot.LocationName,
				LocalTime = _weatherSnapshot.LocalTime,
				Current = _weatherSnapshot.Current,
				ForecastDays = []
			});
		}

		public Task<IReadOnlyList<HourlyForecast>> GetHourlyForecastAsync(CancellationToken cancellationToken)
		{
			IReadOnlyList<HourlyForecast> hourlyForecast = _weatherSnapshot.ForecastDays
				.SelectMany(forecastDay => forecastDay.Hours)
				.ToArray();

			return Task.FromResult(hourlyForecast);
		}

		public Task<IReadOnlyList<ForecastDay>> GetDailyForecastAsync(CancellationToken cancellationToken)
		{
			IReadOnlyList<ForecastDay> dailyForecast = _weatherSnapshot.ForecastDays
				.Select(forecastDay => new ForecastDay
				{
					Date = forecastDay.Date,
					Summary = forecastDay.Summary,
					Hours = []
				})
				.ToArray();

			return Task.FromResult(dailyForecast);
		}
	}
}
