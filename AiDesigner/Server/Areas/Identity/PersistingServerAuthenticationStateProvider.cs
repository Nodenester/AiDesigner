using AiDesigner.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace AiDesigner.Areas.Identity
{
    internal sealed class PersistingServerAuthenticationStateProvider : ServerAuthenticationStateProvider, IDisposable
    {
        private readonly PersistentComponentState state;
        private readonly IdentityOptions options;

        private readonly PersistingComponentStateSubscription subscription;

        private Task<AuthenticationState>? authenticationStateTask;

        public PersistingServerAuthenticationStateProvider(
            PersistentComponentState persistentComponentState,
            IOptions<IdentityOptions> optionsAccessor)
        {
            state = persistentComponentState;
            options = optionsAccessor.Value;

            // Subscribe to authentication state changes.
            AuthenticationStateChanged += OnAuthenticationStateChanged;

            // Register for persisting state, but consider checking here if it's appropriate to persist (e.g., client vs. server).
            subscription = state.RegisterOnPersisting(OnPersistingAsync);
        }

        private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            authenticationStateTask = task;
        }


        // if this fucntion is of server auth works but if ts on client works?
        private async Task OnPersistingAsync()
        {
            if (authenticationStateTask == null)
            {
                Debug.WriteLine($"Authentication state not set in {nameof(OnPersistingAsync)}().");
                return; // Early return to avoid throwing an exception.
            }

            var authenticationState = await authenticationStateTask;
            var principal = authenticationState.User;

            if (principal.Identity?.IsAuthenticated == true)
            {
                var userId = principal.FindFirst(options.ClaimsIdentity.UserIdClaimType)?.Value;
                var email = principal.FindFirst(options.ClaimsIdentity.EmailClaimType)?.Value;
                var name = principal.FindFirst(options.ClaimsIdentity.UserNameClaimType)?.Value;
                var role = principal.FindFirst(options.ClaimsIdentity.RoleClaimType)?.Value;

                // Only persist user info if there's meaningful data to persist.
                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(email))
                {
                    state.PersistAsJson(nameof(UserInfo), new UserInfo
                    {
                        UserId = userId,
                        Name = name,
                        Email = email,
                        Role = role,
                    });
                }
            }
        }

        public void Dispose()
        {
            // Clean up by disposing the subscription and detaching the event handler.
            subscription.Dispose();
            AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
    }
}
