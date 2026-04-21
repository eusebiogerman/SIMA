using Microsoft.Identity.Client;
using System.Linq;
using System.Threading.Tasks;

namespace SIMA.Presentation.Repository
{
    public class AuthService
    {
        private readonly IPublicClientApplication _authApp;
        private AuthenticationResult _authResult;

        public bool IsAuthenticated => _authResult != null;
        public IPublicClientApplication AuthApp => _authApp;

        public AuthService()
        {
            _authApp = PublicClientApplicationBuilder.Create("019972b7-264b-4a7d-a96b-c69eecd5e67a")
                      .WithRedirectUri("http://localhost")
                      .Build();
        }

        public async Task<bool> LoginAsync()
        {
            string[] scopes = { "User.Read" };

            try
            {
                var accounts = await _authApp.GetAccountsAsync();
                _authResult = await _authApp.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                                        .ExecuteAsync();
            }
            catch
            {
                _authResult = await _authApp.AcquireTokenInteractive(scopes)
                                        .WithPrompt(Prompt.SelectAccount)
                                        .ExecuteAsync();
            }

            return _authResult != null;
        }
        public string GetUserName()
        {
            return _authResult?.Account?.Username ?? "";
        }
    }

}
