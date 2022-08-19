using Medior.Shared;
using Medior.Shared.Dtos;
using Medior.Shared.Entities;
using Medior.Shared.Helpers;
using Medior.Shared.Interfaces;
using Medior.Shared.Services;
using Medior.Web.Server.Data;
using Medior.Web.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Medior.Web.Server.Services
{
    public interface IClipboardSyncService
    {
        Task Delete(Guid id);
        Task<Result<ClipboardSave>> GetSavedClip(Guid id);
        string RegisterReceiver(string connectionId);
        Task<ClipboardSave> SaveClip(ClipboardContentDto content, Guid? ownerId = null);
        Task<bool> SendToReceiver(ClipboardContentDto content, string receiptToken);
        Task<DbActionResult> UpdateClip(ClipboardSaveDto dto);
    }

    public class ClipboardSyncService : IClipboardSyncService
    {
        private static readonly MemoryCache _recipients = new(new MemoryCacheOptions());

        private readonly AppDb _appDb;
        private readonly IHubContext<DesktopHub, IDesktopHubClient> _desktopHub;
        private readonly TimeSpan _expirationDelay = TimeSpan.FromMinutes(10);
        private readonly ISystemTime _systemTime;
        public ClipboardSyncService(
            AppDb appDb,
            ISystemTime systemTime,
            IHubContext<DesktopHub, IDesktopHubClient> desktopHub)
        {
            _appDb = appDb;
            _systemTime = systemTime;
            _desktopHub = desktopHub;
        }

        public async Task Delete(Guid id)
        {
            var savedClip = await _appDb.ClipboardSaves.FindAsync(id);

            if (savedClip is null)
            {
                return;
            }

            _appDb.ClipboardSaves.Remove(savedClip);
            await _appDb.SaveChangesAsync();
        }

        public async Task<Result<ClipboardSave>> GetSavedClip(Guid id)
        {
            var result = await _appDb.ClipboardSaves.FindAsync(id);
            if (result is null)
            {
                return Result.Fail<ClipboardSave>("Not found");
            }

            result.LastAccessed = _systemTime.Now;
            await _appDb.SaveChangesAsync();

            return Result.Ok(result);
        }

        public string RegisterReceiver(string connectionId)
        {
            var accessKey = RandomGenerator.GenerateAccessKey();
            var cts = new CancellationTokenSource(_expirationDelay);
            var expirationToken = new CancellationChangeToken(cts.Token);
            _recipients.Set(accessKey, connectionId, expirationToken);
            return accessKey;
        }

        public async Task<ClipboardSave> SaveClip(ClipboardContentDto content, Guid? ownerId = null)
        {
            return await SaveClipImpl(content, ownerId);
        }

        public async Task<bool> SendToReceiver(ClipboardContentDto content, string receiptToken)
        {

            if (string.IsNullOrWhiteSpace(receiptToken))
            {
                return false;
            }

            if (!_recipients.TryGetValue(receiptToken, out string connectionId))
            {

                return false;
            }

            var entity = await SaveClipImpl(content);

            var dto = new ClipboardReadyDto(new ClipboardSaveDto(entity), receiptToken);
            foreach (var chunk in DtoChunker.ChunkDto(dto, DtoType.ClipboardReady))
            {
                await _desktopHub.Clients.Client(connectionId).ReceiveDto(chunk);
            }

            return true;
        }

        public async Task<DbActionResult> UpdateClip(ClipboardSaveDto dto)
        {
            var entity = await _appDb.ClipboardSaves.FindAsync(dto.Id);

            if (entity is null)
            {
                return DbActionResult.NotFound;
            }

            if (dto.AccessTokenEdit != dto.AccessTokenEdit)
            {
                return DbActionResult.Unauthorized;
            }

            entity.FriendlyName = dto.FriendlyName;
            entity.LastAccessed = _systemTime.Now;
            await _appDb.SaveChangesAsync();

            return DbActionResult.Success;
        }

        private async Task<ClipboardSave> SaveClipImpl(ClipboardContentDto content, Guid? ownerId = null)
        {
            var binaryContent = Convert.FromBase64String(content.Base64Content);
            var clipEntity = new ClipboardSave()
            {
                Content = binaryContent,
                CreatedAt = _systemTime.Now,
                LastAccessed = _systemTime.Now,
                ContentSize = binaryContent.Length,
                ContentType = content.Type,
                AccessTokenEdit = RandomGenerator.GenerateAccessKey(),
                AccessTokenView = RandomGenerator.GenerateAccessKey(),
                UserAccountId = ownerId
            };

            _appDb.ClipboardSaves.Add(clipEntity);
            await _appDb.SaveChangesAsync();
            return clipEntity;
        }
    }
}
