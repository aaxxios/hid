using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;

namespace Telegram.Bot.Google
{
    public class GoogleAuthorizor
    {
        private string user;
        private GoogleCodeReceiver codeReceiver;
        public GoogleAuthorizor(string user, GoogleCodeReceiver codeReceiver)
        {
            this.user = user;
            this.codeReceiver = codeReceiver;
        }


        public async Task<UserCredential> Authorize()
        {
            return await GoogleWebAuthorizationBroker.AuthorizeAsync(codeReceiver.Secrets, user: user,
                codeReceiver: codeReceiver as ICodeReceiver, taskCancellationToken: CancellationToken.None,
                scopes: new[]
                {
                    DocsService.Scope.DocumentsReadonly, DocsService.Scope.Documents,
                }
                );
        }
    }
}
