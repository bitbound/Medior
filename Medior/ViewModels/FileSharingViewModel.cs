using Medior;
using Medior.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Medior.ViewModels
{
    [ObservableObject]
    public partial class FileSharingViewModel
    {
        private readonly IServerUriProvider _serverUriProvider;

        [ObservableProperty]
        private string _webViewUrl;

        public FileSharingViewModel(IServerUriProvider serverUriProvider)
        {
            _serverUriProvider = serverUriProvider;
            _webViewUrl = $"{_serverUriProvider.ServerUri}/file-sharing?embedded=true";
        }
    }
}
