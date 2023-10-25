using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Examples.WebHook.Google;

namespace Telegram.Bot.Controllers
{
    public class AuthorizeController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] string code, [FromQuery] string scope, [FromQuery] string state,
        [FromServices] ITelegramBotClient botClient)
        {
            try
            {
                var flow = HttpContext.RequestServices.GetRequiredService<GoogleFlow>();
                await flow.flow.ExchangeCodeForTokenAsync(state, code, flow.RedirectUrl, CancellationToken.None);
                await botClient.SendTextMessageAsync(long.Parse(state), "Your verification was complete");
                return Ok("Verified");
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
