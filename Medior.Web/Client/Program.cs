using Medior.Shared.Interfaces;
using Medior.Shared.Services;
using Medior.Web.Client;
using Medior.Web.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IJsInterop, JsInterop>();
builder.Services.AddSingleton<IServerUriProvider, ServerUriProvider>();
builder.Services.AddSingleton<IModalService, ModalService>();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddSingleton<IToastService, ToastService>();
builder.Services.AddSingleton<ILoaderService, LoaderService>();
builder.Services.AddHttpClient();

await builder.Build().RunAsync();
