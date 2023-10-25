using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Examples.WebHook.Google;
using Telegram.Bot.Filters;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Google.Apis.Util;
using Telegram.Bot.Examples.WebHook.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.State;
using Telegram.Bot.Services;

namespace Telegram.Bot.Controllers;

public class BotController : ControllerBase
{
    private readonly ILogger<BotController> _logger;
    private readonly ApplicationDbConext conext;

    public BotController(ILogger<BotController> logger, ApplicationDbConext dbConext)
    {
       _logger = logger;
        conext = dbConext;
    }

    [HttpPost]
    [ValidateTelegramBot]
    public async Task<IActionResult> Post(
        [FromBody] Update update,[FromServices] ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
       
        //var handleUpdateService = HttpContext.RequestServices.GetRequiredService<UpdateHandlers>();
        //await handleUpdateService.HandleUpdateAsync(update, cancellationToken);
        if(update?.Message is Message msg)
        {
            if(msg.Text is string text)
            {
                if (text.StartsWith("/start", StringComparison.Ordinal))
                {
                    StateManager.Initialise(msg.From!.Id);
#pragma warning disable CA1305
                    var userId = long.Parse($"{msg.From!.Id}");
                    try
                    {
                        await conext.Users.AddAsync(new Models.User()
                        {
                            Id = userId
                        }, cancellationToken: cancellationToken);
                        await conext.SaveChangesAsync(cancellationToken: cancellationToken);
                    }
#pragma warning disable CA1031
                    catch 
                    {
                        // maybe use exists
                    }
                    var flow = HttpContext.RequestServices.GetRequiredService<GoogleFlow>();
                    var token = await flow.flow.LoadTokenAsync($"{msg.From!.Id}", cancellationToken);
                    if (token != null && !token.IsExpired(SystemClock.Default))
                    {
                        
                        var panel = await conext.Panels.Where(panel => panel.Owner.Id == userId).FirstOrDefaultAsync(
                            cancellationToken: cancellationToken);
                        if (panel == null)
                        {
                            var rm = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Panel", "panel_url"));
                            StateManager.SetState(msg.From.Id, UserState.Url);
                            await botClient.SendTextMessageAsync(msg.Chat, "Add Panel",
                                cancellationToken: cancellationToken, replyMarkup: rm);
                            return Ok();
                        }
                        await botClient.SendTextMessageAsync(msg.Chat, "You can now upload", cancellationToken: cancellationToken,
                            replyMarkup: UpdateHandlers.UploadKeyboard);
                        return Ok();
                    }
                    var codeequest = flow.flow.CreateAuthorizationCodeRequest(flow.RedirectUrl);
                    codeequest.State = $"{msg.From!.Id}";
                    var url = codeequest.Build().AbsoluteUri;
                    var kb = new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Google", url));
                    await botClient.SendTextMessageAsync(
                        update.Message.Chat, "Start your Google verification", cancellationToken: cancellationToken,
                        replyMarkup: kb
                        );
                }
                else if(StateManager.GetState(msg.From!.Id) == UserState.Url)
                {
                    await UpdateHandlers.SaveUrl(botClient, msg);
                }
                else if (StateManager.GetState(msg.From!.Id) == UserState.SharedSecret)
                {
                    await UpdateHandlers.SaveSharedSecret(botClient, msg);
                }

                else if (StateManager.GetState(msg.From!.Id) == UserState.AdminSecret)
                {
                    var handler = HttpContext.RequestServices.GetRequiredService<UpdateHandlers>();
                    await handler.SaveAdminSecret(botClient, msg);
                }

                else if(StateManager.GetState(msg.From!.Id) == UserState.UploadUserId)
                {
                    var handler = HttpContext.RequestServices.GetRequiredService<UpdateHandlers>();
                    await handler.Upload(botClient, msg);
                }
            }
        }

        else if(update?.CallbackQuery is CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data!.StartsWith("panel", StringComparison.Ordinal))
            {
                await UpdateHandlers.HandlePanelUrlPrompt(botClient, callbackQuery);
            }
            else if(callbackQuery.Data!.StartsWith("upload", StringComparison.Ordinal))
            {
                await UpdateHandlers.HandleUploadPrompt(botClient, callbackQuery);
            }
        }
        return Ok();
    }
}
