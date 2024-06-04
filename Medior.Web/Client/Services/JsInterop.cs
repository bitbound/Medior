using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Medior.Web.Client.Services;

public interface IJsInterop
{
    ValueTask AddBeforeUnloadHandler();
    ValueTask AddClassName(ElementReference element, string className);
    ValueTask Alert(string message);
    ValueTask<bool> Confirm(string message);
    ValueTask DisposeAsync();
    ValueTask DownloadFile(string url, string fileName);
    ValueTask<string> GetClipboardText();
    ValueTask<byte[]> GetClipboardImage();

    ValueTask<int> GetCursorIndex(ElementReference inputElement);
    ValueTask InvokeClick(string elementId);
    ValueTask OpenWindow(string url, string target);
    ValueTask PreventTabOut(ElementReference terminalInput);
    ValueTask<string> Prompt(string message);
    ValueTask Reload();
    ValueTask ScrollToElement(ElementReference element);

    ValueTask ScrollToEnd(ElementReference element);

    ValueTask SetClipboard(string content);
    ValueTask SetStyleProperty(ElementReference element, string propertyName, string value);
    ValueTask StartDraggingY(ElementReference element, double clientY);
}

public class JsInterop : IAsyncDisposable, IJsInterop
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    public JsInterop(IJSRuntime jsRuntime)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./interop.js").AsTask());
    }

    public async ValueTask AddBeforeUnloadHandler()
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("addBeforeUnloadHandler");
    }

    public async ValueTask AddClassName(ElementReference element, string className)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("addClassName", element, className);
    }

    public async ValueTask Alert(string message)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("invokeAlert", message);
    }

    public async ValueTask<bool> Confirm(string message)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<bool>("invokeConfirm", message);
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
    public async ValueTask DownloadFile(string url, string fileName)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("downloadFile", url, fileName);
    }

    public async ValueTask<byte[]> GetClipboardImage()
    {
        var module = await _moduleTask.Value;
        var base64 = await module.InvokeAsync<string>("getClipboardImage");
        if (string.IsNullOrWhiteSpace(base64))
        {
            return Array.Empty<byte>();
        }
        return Convert.FromBase64String(base64);
    }

    public async ValueTask<string> GetClipboardText()
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("getClipboardText");
    }

    public async ValueTask<int> GetCursorIndex(ElementReference inputElement)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<int>("getSelectionStart", inputElement);
    }

    public async ValueTask InvokeClick(string elementId)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("invokeClick", elementId);
    }

    public async ValueTask OpenWindow(string url, string target)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("openWindow", url, target);
    }

    public async ValueTask PreventTabOut(ElementReference terminalInput)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("preventTabOut", terminalInput);
    }

    public async ValueTask<string> Prompt(string message)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("invokePrompt", message);
    }

    public async ValueTask Reload()
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("reload");
    }

    public async ValueTask ScrollToElement(ElementReference element)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("scrollToElement", element);
    }

    public async ValueTask ScrollToEnd(ElementReference element)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("scrollToEnd", element);
    }
    public async ValueTask SetClipboard(string content)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("setClipboard", content);
    }

    public async ValueTask SetStyleProperty(ElementReference element, string propertyName, string value)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("setStyleProperty", element, propertyName, value);
    }
    public async ValueTask StartDraggingY(ElementReference element, double clientY)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("startDraggingY", element, clientY);
    }
}
