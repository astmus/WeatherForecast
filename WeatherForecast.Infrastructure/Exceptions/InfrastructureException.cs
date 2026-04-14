namespace WeatherForecast.Infrastructure.Exceptions;

public sealed class InfrastructureException : Exception
{
	public InfrastructureException(string message, string? methodContext = default) : base(message)
	{
		Data["ExceptionMethod"] = methodContext;
	}

	public InfrastructureException(string message, Exception innerException, string? methodContext = default)
		: base(message, innerException)
	{
		Data["ExceptionMethod"] = methodContext;
	}
}
