using Medior.Shared.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Medior.Web.Client.Services
{
    public class ServerUriProvider : IServerUriProvider
    {
        private readonly IWebAssemblyHostEnvironment _hostEnvironment;

        public ServerUriProvider(IWebAssemblyHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }
        public string ServerUri => _hostEnvironment.BaseAddress.TrimEnd('/');
    }
}
