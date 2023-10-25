using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Options;
using Google.Apis.Util;
using Google.Apis.Drive.v3;

namespace Telegram.Bot.Examples.WebHook.Google
{
    public class GoogleFlow
    {
        private readonly BotConfiguration _botConfiguration;
        public readonly string RedirectUrl;
        public GoogleFlow(IOptions<BotConfiguration> configuration)
        {
            _botConfiguration = configuration?.Value;
            RedirectUrl = configuration?.Value.GoogleRedirecUrl;
        }


        public PkceGoogleAuthorizationCodeFlow flow
        {
            get =>
                new(new GoogleAuthorizationCodeFlow.Initializer()
                {
                    ClientSecrets = new ClientSecrets()
                    {
                        ClientId = _botConfiguration.GoogleClientId,
                        ClientSecret = _botConfiguration.GoogleClientSeceret
                    },
                    Clock = SystemClock.Default,

                    Scopes = new[]
         {
         DocsService.Scope.Documents, DriveService.Scope.Drive
     },
                    DataStore = new FileDataStore("TokenStore2")
                });

        }


    }
}
