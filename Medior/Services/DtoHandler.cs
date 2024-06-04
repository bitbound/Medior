using Medior.Shared.Dtos;
using Medior.Shared.Helpers;
using Medior.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Medior.Services;

public class DtoHandler : IDtoHandler
{
    private readonly ILogger<DtoHandler> _logger;
    private readonly IMessenger _messenger;

    public DtoHandler(IMessenger messenger, ILogger<DtoHandler> logger)
    {
        _messenger = messenger;
        _logger = logger;
    }

    public Task ReceiveDto(DtoWrapper dto)
    {
        try
        {
            switch (dto.DtoType)
            {
                case DtoType.ClipboardReady:
                    {
                        if (DtoChunker.TryComplete<ClipboardReadyDto>(dto, out var result) &&
                            result is not null)
                        {
                            _messenger.Send(result);
                        }

                        break;
                    }
                case DtoType.Unknown:
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while receiving DtoWrapper.");
        }
        return Task.CompletedTask;
    }

    //private void TryInvoke(Action action)
    //{
    //    try
    //    {
    //        action();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error while invoking DTO handler.");
    //    }
    //}
}
