using Medior.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    [ObservableObject]
    public partial class RemoteControlViewModel
    {
        private readonly IServerUriProvider _serverUri;
        private Guid _streamId;

        [ObservableProperty]
        private string _streamingUri = "https://localhost:7162/api/streaming/d38d4b24-f0b8-4955-a048-a9c173fbed8c";

        public RemoteControlViewModel(IServerUriProvider serverUri)
        {
            _serverUri = serverUri;
        }

        public Guid StreamId
        {
            get => _streamId;
            set
            {
                _streamId = value;
                StreamingUri = $"{_serverUri.ServerUri}/api/streaming/{value}";
            }
        }
    }
}
