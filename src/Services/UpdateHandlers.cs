using Telegram.Bot.Examples.WebHook.Data;
using Telegram.Bot.State;
using Telegram.Bot.Types;
using Google.Apis.Services;
using Telegram.Bot.Models;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Examples.WebHook.Google;
using Google.Apis.Util;
using Google.Apis.Docs.v1;
using Dat = Google.Apis.Docs.v1.Data;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Drive;
using Google.Apis.Drive.v3;
using G = Google.Apis.Drive;


using Google.Apis.Auth.OAuth2;

namespace Telegram.Bot.Services;

public class UpdateHandlers
{
    private readonly ApplicationDbConext conext;
    private readonly GoogleFlow flow;

    public UpdateHandlers(ApplicationDbConext dbConext, GoogleFlow googleFlow)
    {
        conext = dbConext;
        flow = googleFlow;
    }

    public static InlineKeyboardMarkup UploadKeyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Upload", "upload"));
    public static async Task HandlePanelUrlPrompt(ITelegramBotClient client, CallbackQuery query)
    {
        await client.EditMessageTextAsync(
            messageId: query.Message!.MessageId, chatId: query.From.Id, text: "Send Panel URL"
            );
    }

    public static async Task HandleUploadPrompt(ITelegramBotClient client, CallbackQuery query)
    {
        StateManager.SetState(query.From.Id, UserState.UploadUserId);
        await client.EditMessageTextAsync(
            messageId: query.Message!.MessageId, chatId: query.From.Id, text: "Send the uuid "
            );
    }
    public static async Task SaveUrl(ITelegramBotClient client, Message message)
    {
        StateManager.Data[message.From!.Id].Url = message.Text!.Trim();
        StateManager.SetState(message.From.Id, UserState.SharedSecret);
        await client.SendTextMessageAsync(
            message.Chat, text: "Send the Shared Secret");
    }

    public async Task Upload(ITelegramBotClient client, Message message)
    {
        var panel = await conext.Panels.Where(panel => panel.Owner.Id == message.From.Id).FirstAsync();
        using var httpClient = new HttpClient();
        try
        {
            var token = await flow.flow.LoadTokenAsync($"{message.From!.Id}", CancellationToken.None);
            if (token != null && !token.IsExpired(SystemClock.Default))
            {
                UserCredential credential = new(flow.flow, $"{message.From!.Id}", token);
                string text;
                try
                {
                    text = await panel.DownloadDocument(message.Text!.Trim(), httpClient);
                }
                catch(InvalidOperationException ex)
                {
                    await client.SendTextMessageAsync(message.Chat, $"Error:\n{ex.Message}");
                    return;
                }
                using var docService = new DocsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential
                }) ;
                var docCreateRequest = docService.Documents.Create(new Dat.Document()
                {
                    Title = "VPN"
                });
                
                var response = await docCreateRequest.ExecuteAsync();
                
                await conext.SaveChangesAsync();
                var updateRequest = docService.Documents.BatchUpdate(new Dat.BatchUpdateDocumentRequest()
                {
                    Requests = new List<Dat.Request>()
                    {
                        new Dat.Request()
                        {
                            InsertText = new Dat.InsertTextRequest()
                            {
                                Text = text,
                                Location = new Dat.Location()
                                {
                                    SegmentId = "",
                                    Index = 1
                                }
                            }
                        }
                    }
                    
                }, response.DocumentId);
                var user = conext.Users.Where(user => user.Id == message.From.Id).First();
                var doc = new Telegram.Bot.Models.Doc()
                {
                    DocId = response.DocumentId,
                    Owner = user
                };
                await conext.Docs.AddAsync(doc);
                await conext.SaveChangesAsync();
                var updateResponse = await updateRequest.ExecuteAsync();
                using var driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer= credential
                }) ;
                var permUpdate = driveService.Permissions.Create(new Permission() {
                    Role = "reader", Type = "anyone", AllowFileDiscovery = true
                }, updateResponse.DocumentId);
                
                var permRes = await permUpdate.ExecuteAsync();
                var webLink = driveService.Files.Get(updateResponse.DocumentId);
                
                var exportRequest = driveService.Files.Export(updateResponse.DocumentId, "text/plain");
                var exportResponse = await exportRequest.ExecuteAsync();
                webLink.Fields = "webContentLink";
                
                var linkRes = await webLink.ExecuteAsync();
                await client.SendTextMessageAsync(message.Chat,
                    $"Upload completed: {linkRes.WebContentLink}",
                    disableWebPagePreview: true);
            }

        }
        catch (Exception ex)
        {
            //await client.SendTextMessageAsync(message.Chat, $"Failed to upload: {ex.Message}");
            throw new Exception(ex.Message);
        }

    }
    public static async Task SaveSharedSecret(ITelegramBotClient client, Message message)
    {
        StateManager.Data[message.From!.Id].SharedSecret = message.Text!.Trim();
        StateManager.SetState(message.From.Id, UserState.AdminSecret);
        await client.SendTextMessageAsync(
            message.Chat, text: "Send the Admin Secret");
    }

    public async Task SaveAdminSecret(ITelegramBotClient client, Message message)
    {
        StateManager.Data[message.From!.Id].AdminSecret = message.Text!.Trim();
        StateManager.SetState(message.From.Id, UserState.UploadReady);
        var data = StateManager.Data[message.From!.Id];
        var user = conext.Users.Where(user => user.Id == message.From.Id).First();
        Panel panel = new()
        {
            AdminSecret = data.AdminSecret,
            SharedSecret = data.SharedSecret,
            Url = data.Url,
            Owner = user,
        };
        try
        {
            await conext.AddAsync(panel);
            await conext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            await client.SendTextMessageAsync(
            message.Chat, text: "Failed");
            return;
        }

        await client.SendTextMessageAsync(
            message.Chat, text: "Panel Information saved. You can now uplaod", replyMarkup: UploadKeyboard);

    }
}
