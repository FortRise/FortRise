using System.Buffers;
using System.IO;

namespace FortRise;

public sealed class PooledMemoryStream(byte[] buffer, int index, int count) 
    : MemoryStream(buffer, index, count)
{
    private readonly byte[] buffer = buffer;
    private bool isDisposed;

    protected override void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            isDisposed = true;

            if (buffer is not null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        base.Dispose(disposing);
    }
}
