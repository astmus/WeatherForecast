namespace WeatherForecast.Application.Exceptions;

public sealed class WeatherApplicationException : Exception
{
	public WeatherApplicationException(string message, string? methodContext = default) : base(message)
	{
		Data["ExceptionMethod"] = methodContext;
	}

	public WeatherApplicationException(string message, Exception innerException, string? methodContext = default)
		: base(message, innerException)
	{
		Data["ExceptionMethod"] = methodContext;
	}
}
