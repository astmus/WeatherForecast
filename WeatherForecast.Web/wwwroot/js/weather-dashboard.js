const elements = {
	weatherShell: document.getElementById("weatherShell"),
	refreshButton: document.getElementById("refreshButton"),
	loadingState: document.getElementById("loadingState"),
	errorInline: document.getElementById("errorInline"),
	weatherContent: document.getElementById("weatherContent"),
	locationName: document.getElementById("locationName"),
	localTimeLabel: document.getElementById("localTimeLabel"),
	currentTemperature: document.getElementById("currentTemperature"),
	currentCondition: document.getElementById("currentCondition"),
	currentIcon: document.getElementById("currentIcon"),
	feelsLikeValue: document.getElementById("feelsLikeValue"),
	humidityValue: document.getElementById("humidityValue"),
	windValue: document.getElementById("windValue"),
	updatedAtValue: document.getElementById("updatedAtValue"),
	hourlyForecast: document.getElementById("hourlyForecast"),
	dailyForecast: document.getElementById("dailyForecast")
};

let activeRequestController = null;
const apiBaseUrl = elements.weatherShell?.dataset.apiBaseUrl?.replace(/\/$/, "") ?? "";

document.addEventListener("DOMContentLoaded", () =>
{
	elements.refreshButton?.addEventListener("click", () =>
	{
		void loadWeatherAsync();
	});

	void loadWeatherAsync();
});

async function loadWeatherAsync()
{
	if (activeRequestController !== null)
	{
		activeRequestController.abort();
	}

	activeRequestController = new AbortController();
	const hasVisibleContent = elements.weatherContent !== null && elements.weatherContent.hidden === false;
	setLoadingState(hasVisibleContent);

	try
	{
		const [currentWeather, hourlyForecast, dailyForecast] = await Promise.all([
			fetchJsonAsync("/api/weather/current", activeRequestController.signal),
			fetchJsonAsync("/api/weather/hourly", activeRequestController.signal),
			fetchJsonAsync("/api/weather/daily", activeRequestController.signal)
		]);
		const weather = {
			locationName: currentWeather.locationName,
			localTime: currentWeather.localTime,
			current: currentWeather.current,
			hourlyForecast,
			dailyForecast
		};

		renderWeather(weather);
		setContentState();
	}
	catch (error)
	{
		if (isAbortError(error))
		{
			return;
		}

		console.error(error);
		setErrorState(error instanceof Error ? error.message : "Во время загрузки прогноза произошла ошибка.");
	}
	finally
	{
		activeRequestController = null;
	}
}

function setLoadingState(hasVisibleContent)
{
	if (elements.loadingState !== null)
	{
		elements.loadingState.hidden = hasVisibleContent;
	}

	if (elements.errorInline !== null)
	{
		elements.errorInline.hidden = true;
	}

	if (elements.refreshButton !== null)
	{
		elements.refreshButton.disabled = true;
		elements.refreshButton.textContent = "Обновляем...";
	}
}

function setContentState()
{
	if (elements.loadingState !== null)
	{
		elements.loadingState.hidden = true;
	}

	if (elements.errorInline !== null)
	{
		elements.errorInline.hidden = true;
	}

	if (elements.weatherContent !== null)
	{
		elements.weatherContent.hidden = false;
	}

	if (elements.refreshButton !== null)
	{
		elements.refreshButton.disabled = false;
		elements.refreshButton.textContent = "Обновить";
	}
}

function setErrorState(message)
{
	if (elements.loadingState !== null)
	{
		elements.loadingState.hidden = true;
	}

	if (elements.errorInline !== null)
	{
		elements.errorInline.hidden = false;
		elements.errorInline.textContent = message;
	}

	if (elements.refreshButton !== null)
	{
		elements.refreshButton.disabled = false;
		elements.refreshButton.textContent = "Обновить";
	}
}

function renderWeather(weather)
{
	if (elements.loadingState !== null)
	{
		elements.loadingState.hidden = true;
	}

	if (elements.weatherShell !== null)
	{
		elements.weatherShell.dataset.theme = weather.current.isDay ? "day" : "night";
	}

	if (elements.locationName !== null)
	{
		elements.locationName.textContent = weather.locationName;
	}

	if (elements.localTimeLabel !== null)
	{
		elements.localTimeLabel.textContent = formatDateTime(weather.localTime);
	}

	if (elements.currentTemperature !== null)
	{
		elements.currentTemperature.textContent = formatTemperature(weather.current.temperatureC);
	}

	if (elements.currentCondition !== null)
	{
		elements.currentCondition.textContent = weather.current.condition;
	}

	if (elements.feelsLikeValue !== null)
	{
		elements.feelsLikeValue.textContent = formatTemperature(weather.current.feelsLikeC);
	}

	if (elements.humidityValue !== null)
	{
		elements.humidityValue.textContent = `${Math.round(weather.current.humidityPercentage)}%`;
	}

	if (elements.windValue !== null)
	{
		elements.windValue.textContent = `${Math.round(weather.current.windKph)} км/ч`;
	}

	if (elements.updatedAtValue !== null)
	{
		elements.updatedAtValue.textContent = `Обновлено в ${formatTime(weather.current.lastUpdatedAt)}`;
	}

	updateCurrentIcon(weather.current);
	renderHourlyForecast(weather.hourlyForecast);
	renderDailyForecast(weather.dailyForecast);
}

function updateCurrentIcon(currentWeather)
{
	if (elements.currentIcon === null)
	{
		return;
	}

	if (!currentWeather.iconUrl)
	{
		elements.currentIcon.hidden = true;
		elements.currentIcon.removeAttribute("src");
		elements.currentIcon.alt = "";

		return;
	}

	elements.currentIcon.hidden = false;
	elements.currentIcon.src = currentWeather.iconUrl;
	elements.currentIcon.alt = currentWeather.condition;
}

function renderHourlyForecast(hourlyForecast)
{
	if (elements.hourlyForecast === null)
	{
		return;
	}

	elements.hourlyForecast.innerHTML = hourlyForecast
		.map(hour => {
			const condition = escapeHtml(hour.condition);
			const iconUrl = escapeHtml(hour.iconUrl);

			return `
				<article class="hour-card">
					<header class="hour-card__header">
						<div>
							<p class="hour-card__meta">Время</p>
							<h3 class="hour-card__time">${escapeHtml(formatTime(hour.time))}</h3>
						</div>
						<img class="hour-card__icon" src="${iconUrl}" alt="${condition}" loading="lazy" />
					</header>
					<p class="hour-card__temperature">${formatTemperature(hour.temperatureC)}</p>
					<p class="hour-card__condition">${condition}</p>
					<p class="hour-card__meta">Вероятность дождя ${Math.round(hour.rainChancePercentage)}%</p>
				</article>
			`;
		})
		.join("");
}

function renderDailyForecast(dailyForecast)
{
	if (elements.dailyForecast === null)
	{
		return;
	}

	elements.dailyForecast.innerHTML = dailyForecast
		.map(day => {
			const condition = escapeHtml(day.condition);
			const iconUrl = escapeHtml(day.iconUrl);

			return `
				<article class="day-card">
					<header class="day-card__header">
						<div>
							<p class="day-card__date">${escapeHtml(formatDate(day.date))}</p>
							<h3 class="day-card__day">${escapeHtml(formatWeekday(day.date))}</h3>
						</div>
						<img class="day-card__icon" src="${iconUrl}" alt="${condition}" loading="lazy" />
					</header>
					<p class="day-card__range">${formatTemperature(day.maxTemperatureC)} / ${formatTemperature(day.minTemperatureC)}</p>
					<p class="day-card__condition">${condition}</p>
					<div class="day-card__meta">
						<span>Осадки</span>
						<span>${Math.round(day.rainChancePercentage)}%</span>
					</div>
				</article>
			`;
		})
		.join("");
}

function formatTemperature(value)
{
	return `${Math.round(value)}°`;
}

function formatDateTime(value)
{
	return new Intl.DateTimeFormat("ru-RU", {
		weekday: "long",
		day: "numeric",
		month: "long",
		hour: "2-digit",
		minute: "2-digit"
	}).format(parseWeatherDate(value));
}

function formatTime(value)
{
	return new Intl.DateTimeFormat("ru-RU", {
		hour: "2-digit",
		minute: "2-digit"
	}).format(parseWeatherDate(value));
}

function formatDate(value)
{
	return new Intl.DateTimeFormat("ru-RU", {
		day: "numeric",
		month: "long"
	}).format(parseWeatherDate(value));
}

function formatWeekday(value)
{
	const formattedValue = new Intl.DateTimeFormat("ru-RU", {
		weekday: "long"
	}).format(parseWeatherDate(value));

	return formattedValue.charAt(0).toUpperCase() + formattedValue.slice(1);
}

async function fetchJsonAsync(path, signal)
{
	const response = await fetch(buildWeatherUrl(path), {
		headers: {
			Accept: "application/json"
		},
		signal
	});

	if (!response.ok)
	{
		const problemDetails = await tryReadProblemDetailsAsync(response);
		throw new Error(problemDetails?.detail ?? "Не удалось получить прогноз погоды.");
	}

	return await response.json();
}

function buildWeatherUrl(path)
{
	return `${apiBaseUrl}${path}`;
}

function parseWeatherDate(value)
{
	if (typeof value === "string" && /^\d{4}-\d{2}-\d{2}$/.test(value))
	{
		const [year, month, day] = value.split("-").map(Number);

		return new Date(year, month - 1, day);
	}

	return new Date(value);
}

function escapeHtml(value)
{
	return String(value ?? "")
		.replaceAll("&", "&amp;")
		.replaceAll("<", "&lt;")
		.replaceAll(">", "&gt;")
		.replaceAll("\"", "&quot;")
		.replaceAll("'", "&#39;");
}

function isAbortError(error)
{
	return error instanceof DOMException && error.name === "AbortError";
}

async function tryReadProblemDetailsAsync(response)
{
	try
	{
		return await response.json();
	}
	catch
	{
		return null;
	}
}
