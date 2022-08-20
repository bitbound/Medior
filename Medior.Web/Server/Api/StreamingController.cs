using Medior.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System;
using Medior.Web.Server.Services;

namespace Medior.Web.Server.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamingController : ControllerBase
    {
        private static readonly ConcurrentDictionary<Guid, Stream> _streams = new();
        private readonly IDesktopStreamCache _streamCache;
        private readonly ILogger<StreamingController> _logger;

        public StreamingController(IDesktopStreamCache streamCache, ILogger<StreamingController> logger)
        {
            _streamCache = streamCache;
            _logger = logger;
        }

        //[HttpGet("{streamId}")]
        //public async Task<IActionResult> Get(Guid streamId)
        //{
        //    var result = await _streamCache.WaitForStreamSession(streamId, TimeSpan.FromSeconds(30));

        //    if (!result.IsSuccess || result.Value?.Stream is null)
        //    {
        //        return BadRequest();
        //    }

        //    try
        //    {
        //        await foreach (var chunk in result.Value.Stream)
        //        {
        //            await Request.Body.WriteAsync(chunk);
        //        }
        //    }
        //    finally
        //    {
        //        result.Value.EndSignal.Release();
        //        _logger.LogInformation("Streaming session ended for stream ID {streamId}.",
        //            streamId);
        //    }

        //    return Ok();
        //}



        [HttpGet("{streamId}")]
        public async Task<IActionResult> Get(Guid streamId)
        {
            if (!_streams.TryGetValue(streamId, out var stream))
            {
                return BadRequest();
            }

            var buffer = new byte[32_000];

            while (stream.CanRead && Response.Body.CanWrite)
            {
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, 32_000));
                if (bytesRead == 0)
                {
                    await Task.Delay(10);
                    continue;
                }

                await Response.Body.WriteAsync(buffer.AsMemory(0, bytesRead));
            }

            return Ok();
        }

        [HttpPost("{streamId}")]
        public async Task<IActionResult> Post(Guid streamId)
        {
            if (!_streams.TryAdd(streamId, Request.Body))
            {
                return BadRequest();
            }

            await Task.Delay(Timeout.InfiniteTimeSpan, Request.HttpContext.RequestAborted);

            return Ok();
        }
    }
}
