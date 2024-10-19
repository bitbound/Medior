using Medior.Shared.Models;
using Bitbound.ConcurrentList;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Medior.Shared.IO;

public class EmittableVideoStream : Stream
{
    private readonly ConcurrentList<StreamListener> _listeners = new();
    private long _length;
    private long _position;

    public event EventHandler<VideoChunk>? BytesWritten;

    public override bool CanRead => false;

    public override bool CanSeek => true;

    public override bool CanWrite => true;

    public override long Length => _length;

    public override long Position
    {
        get => _position;
        set => _position = value;
    }

    public override void Flush()
    {

    }

    public async IAsyncEnumerable<VideoChunk> GetRedirectedStream([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var listener = new StreamListener();
        _listeners.Add(listener);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var next = await listener.GetNext(cancellationToken);
                if (next is not null)
                {
                    yield return next;
                }
            }
        }
        finally
        {
            _listeners.Remove(listener);
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                _position = offset;
                break;
            case SeekOrigin.Current:
                _position += offset;
                break;
            case SeekOrigin.End:
                _position = _length - offset;
                break;
            default:
                break;
        }
        return _position;
    }

    public override void SetLength(long value)
    {
        _length = value;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        var bytesToWrite = buffer
            .Skip(offset)
            .Take(count)
            .ToArray();

        var chunk = new VideoChunk(bytesToWrite, DateTimeOffset.Now);

        _position += bytesToWrite.Length;
        if (_position >= _length)
        {
            _length = _position + 1;
        }

        try
        {
            BytesWritten?.Invoke(this, chunk);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        foreach (var listener in _listeners)
        {
            try
            {
                listener.SetNext(chunk);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var listener in _listeners)
            {
                try
                {
                    listener.Dispose();
                }
                catch { }
            }
        }
        base.Dispose(disposing);
    }

    private class StreamListener : IDisposable
    {
        private readonly AutoResetEvent _readWait = new(false);
        private readonly AutoResetEvent _writeWait = new(true);
        private VideoChunk? _next;

        public void Dispose()
        {
            _readWait.Dispose();
            _writeWait.Dispose();
        }

        public async Task<VideoChunk?> GetNext(CancellationToken cancellationToken)
        {
            await Task.Run(_readWait.WaitOne, cancellationToken);
            var next = Interlocked.Exchange(ref _next, null);
            _writeWait.Set();
            return next;
        }

        public void SetNext(VideoChunk next)
        {
            _writeWait.WaitOne();
            Interlocked.Exchange(ref _next, next);
            _readWait.Set();
        }
    }
}
