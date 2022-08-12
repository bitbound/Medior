using Medior.Shared.Dtos;
using Medior.Shared.Helpers;
using Medior.Shared.Interfaces;
using Medior.Web.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Medior.Web.Server.Services
{
    public interface IClipboardSyncService
    {
        string GetAccessKey(byte[] content);
        string RegisterReceiver(string connectionId);

        bool TryGetContent(string accessKey, out byte[] content);
        Task<bool> SendToReceiver(byte[] content, string receiptToken);
    }
    public class ClipboardSyncService : IClipboardSyncService
    {
        private readonly MemoryCache _contentCache = new(new MemoryCacheOptions());
        private readonly TimeSpan _expirationDelay = TimeSpan.FromMinutes(10);
        private readonly MemoryCache _recipients = new(new MemoryCacheOptions());
        private readonly IHubContext<DesktopHub, IDesktopHubClient> _desktopHub;

        public ClipboardSyncService(IHubContext<DesktopHub, IDesktopHubClient> desktopHub)
        {
            _desktopHub = desktopHub;
        }

        public string GetAccessKey(byte[] content)
        {
            var accessKey = GetAccessKeyImpl();
            var cts = new CancellationTokenSource(_expirationDelay);
            var expirationToken = new CancellationChangeToken(cts.Token);
            _contentCache.Set(accessKey, content, expirationToken);
            return accessKey;
        }

        public string RegisterReceiver(string connectionId)
        {
            var accessKey = GetAccessKeyImpl();
            var cts = new CancellationTokenSource(_expirationDelay);
            var expirationToken = new CancellationChangeToken(cts.Token);
            _recipients.Set(accessKey, connectionId, expirationToken);
            return accessKey;
        }

        public async Task<bool> SendToReceiver(byte[] content, string receiptToken)
        {
            if (!_recipients.TryGetValue(receiptToken, out string connectionId))
            {
                
                return false;
            }

            var accessKey = GetAccessKeyImpl();
            var cts = new CancellationTokenSource(_expirationDelay);
            var expirationToken = new CancellationChangeToken(cts.Token);
            _contentCache.Set(accessKey, content, expirationToken);

            var dto = new ClipboardReadyDto() { AccessKey = accessKey };
            foreach (var chunk in DtoChunker.ChunkDto(dto, DtoType.ClipboardReady))
            {
                await _desktopHub.Clients.Client(connectionId).ReceiveDto(chunk);
            }

            return true;
        }

        public bool TryGetContent(string accessKey, out byte[] content)
        {
            if (_contentCache.TryGetValue(accessKey, out content))
            {
                _contentCache.Remove(accessKey);
                return true;
            }
            content = Array.Empty<byte>();
            return false;
        }

        private string GetAccessKeyImpl()
        {
            var accessKey = RandomGenerator.GenerateAccessKey();
            while (_contentCache.TryGetValue(accessKey, out _))
            {
                accessKey = RandomGenerator.GenerateAccessKey();
            }
            return accessKey;
        }
    }
}
