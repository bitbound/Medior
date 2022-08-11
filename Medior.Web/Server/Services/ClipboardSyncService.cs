using Medior.Shared.Dtos;
using Medior.Shared.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Medior.Web.Server.Services
{
    public interface IClipboardSyncService
    {
        string GetAccessKey(byte[] content);
        bool TryGetContent(string accessKey, out byte[] content);
    }
    public class ClipboardSyncService : IClipboardSyncService
    {
        private readonly MemoryCache _contentCache = new(new MemoryCacheOptions());
        private readonly TimeSpan _expirationDelay = TimeSpan.FromMinutes(10);

        public string GetAccessKey(byte[] content)
        {
            var accessKey = RandomGenerator.GenerateAccessKey();
            while (_contentCache.TryGetValue(accessKey, out _))
            {
                accessKey = RandomGenerator.GenerateAccessKey();
            }

            var cts = new CancellationTokenSource(_expirationDelay);
            var expirationToken = new CancellationChangeToken(cts.Token);
            _contentCache.Set(accessKey, content, expirationToken);
            return accessKey;
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
    }
}
