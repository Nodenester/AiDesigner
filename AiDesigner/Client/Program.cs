using AiDesigner.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register HttpClient for making HTTP requests to the server API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Commenting out IHttpClientFactory registration as it's not typically used in Blazor WASM apps
// builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("AiDesigner.ServerAPI"));

// Optional: Remove AddHttpMessageHandler if not using a custom message handler
// builder.Services.AddHttpClient("AiDesigner.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
//     .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();
