using Medior.Shared.Dtos;
using Medior.Web.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Medior.Web.Server.Api;

[Route("api/[controller]")]
[ApiController]
public class ClipboardController : ControllerBase
{
    private readonly IClipboardSyncService _clipboardSync;

    public ClipboardController(IClipboardSyncService clipboardSync)
    {
        _clipboardSync = clipboardSync;
    }

    [RequestSizeLimit(20_000_000)]
    [HttpPost]
    public async Task<ActionResult<ClipboardSaveDto>> Save(ClipboardContentDto content)
    {
        var result = await _clipboardSync.SaveClip(content);
        return new ClipboardSaveDto(result);
    }

    [RequestSizeLimit(20_000_000)]
    [HttpPost("{receiptToken}")]
    public async Task<IActionResult> SendToReceiver([FromBody]ClipboardContentDto content, [FromRoute]string receiptToken)
    {
        var result = await _clipboardSync.SendToReceiver(content, receiptToken);
        if (!result)
        {
            return NotFound();
        }
        return Ok();
    }

    [HttpGet("{clipId}/{accessToken}")]
    public async Task<ActionResult<ClipboardContentDto>> GetContent(Guid clipId, string accessToken)
    {
        var result = await _clipboardSync.GetSavedClip(clipId);

        if (!result.IsSuccess)
        {
            return NotFound();
        }

        if (accessToken != result.Value!.AccessTokenView &&
            accessToken != result.Value!.AccessTokenEdit)
        {
            return Unauthorized();
        }

        var dto = new ClipboardContentDto(result.Value.Content, result.Value.ContentType);
        return Ok(dto);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string accessToken)
    {
        var result = await _clipboardSync.GetSavedClip(id);

        if (!result.IsSuccess)
        {
            return NotFound();
        }

        if (result.Value!.AccessTokenEdit != accessToken)
        {
            return Unauthorized();
        }

        await _clipboardSync.Delete(id);
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> Update(ClipboardSaveDto dto)
    {
        var result = await _clipboardSync.UpdateClip(dto);

        return result switch
        {
            Data.DbActionResult.Success => NoContent(),
            Data.DbActionResult.NotFound => NotFound(),
            Data.DbActionResult.Unauthorized => Unauthorized(),
            _ => BadRequest(),
        };
    }
}
