using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util;
using Microsoft.Extensions.Options;

namespace Telegram.Bot.Google;

#pragma warning disable CA2201
#pragma warning disable CA1062
#pragma warning disable CS1998

public class GoogleCodeReceiver: ICodeReceiver
{
    
    public GoogleCodeReceiver(IOptions<BotConfiguration> options)
    {
        options.ThrowIfNull(nameof(options));
        RedirectUri = options.Value.GoogleRedirecUrl;
        Secrets = new()
        {
            ClientId = options.Value.GoogleClientId,
            ClientSecret = options.Value.GoogleClientSeceret
        };
        
    }

    public string RedirectUri { get; init; }
    public ClientSecrets Secrets { get; init; }

    private string State = "";

    public void SetState(string state)
    {
        State = state;
    }

    public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url, CancellationToken taskCancellationToken)
    {
        url.State = State;
        var authUrl = url.Build();
        throw new Exception(authUrl.AbsoluteUri);
    }
  
}


