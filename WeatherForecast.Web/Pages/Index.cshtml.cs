using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WeatherForecast.Web.Pages;

public sealed class IndexModel : PageModel
{
	public string ApiBaseUrl => string.Empty;

	public void OnGet()
	{
	}
}
