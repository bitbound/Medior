using System;
using System.IO;

namespace Medior.Reactive;

public class ReactiveStream : MemoryStream
{
    public ReactiveStream()
    {

    }

    public ReactiveStream(byte[] buffer)
        : base(buffer)
    {    
    }

    private int _totalRead = 0;
    private int _totalWritten = 0;

    public event EventHandler<int>? TotalBytesReadChanged;
    public event EventHandler<int>? TotalBytesWrittenChanged;

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = base.Read(buffer, offset, count);
        _totalRead += read;
        TotalBytesReadChanged?.Invoke(this, _totalRead);
        return read;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        base.Write(buffer, offset, count);
        _totalWritten += count;
        TotalBytesWrittenChanged?.Invoke(this, _totalWritten);
    }
}
