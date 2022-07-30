using Medior.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface ILoaderService
    {
        internal event EventHandler<string>? LoaderShown;
        internal event EventHandler? LoaderHidden;

        IDisposable Show(string loaderText);
        void Hide();
    }

    public class LoaderService : ILoaderService
    {
        private readonly ILogger<LoaderService> _logger;

        public LoaderService(ILogger<LoaderService> logger)
        {
            _logger = logger;
        }

        public event EventHandler<string>? LoaderShown;
        public event EventHandler? LoaderHidden;

        public void Hide()
        {
            try
            {
                LoaderHidden?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while hiding loader.");
            }
        }

        public IDisposable Show(string loaderText)
        {
            try
            {
                LoaderShown?.Invoke(this, loaderText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while showing loader.");
            }
            return new CallbackDisposable(() => LoaderHidden?.Invoke(this, EventArgs.Empty));
        }

    }
}
