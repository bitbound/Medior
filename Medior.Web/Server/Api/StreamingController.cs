using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Medior.Web.Server.Services;
using Medior.Web.Server.Models;

namespace Medior.Web.Server.Api;

[Route("api/[controller]")]
[ApiController]
public class StreamingController : ControllerBase
{
    private readonly IDesktopStreamCache _streamCache;
    private readonly ILogger<StreamingController> _logger;

    public StreamingController(
        IDesktopStreamCache streamCache,
        ILogger<StreamingController> logger)
    {
        _streamCache = streamCache;
        _logger = logger;
    }

    [HttpGet("{streamId}")]
    public async Task Get(Guid streamId)
    {
        StreamSignaler? signaler;
        var sw = Stopwatch.StartNew();

        while (!_streamCache.TryRemove(streamId, out signaler))
        {
            if (sw.Elapsed > TimeSpan.FromSeconds(10))
            {
                return;
            }

            await Task.Delay(100);
        }

        await signaler.ReadySignal.WaitAsync();

        if (signaler.Stream is null)
        {
            return;
        }

        //Response.ContentType = "video/mp4";
        //Response.ContentType = "video/iso.segment";
        await Response.StartAsync();

        try
        {
            await foreach (var chunk in signaler.Stream)
            {
                var latency = DateTimeOffset.Now - chunk.Timestamp;

                _logger.LogDebug("Writing {length} bytes to video stream  Latency: {latency}.",
                    chunk.Buffer.Length,
                    latency);
                await Response.Body.WriteAsync(chunk.Buffer);
            }
        }
        catch (Exception ex) when (ex.Message.Contains("Stream canceled by client."))
        {
            // Ok
        }
    }
}
