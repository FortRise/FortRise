using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FortLauncher;

namespace FortRise.GameBanana;

internal sealed class CompressedHttpClient : HttpClient
{
    private static readonly SocketsHttpHandler socketOnlyCompressed = new SocketsHttpHandler
    {
        AutomaticDecompression = DecompressionMethods.None,
        ConnectCallback = OnConnect
    };

    public CompressedHttpClient()
        : base(socketOnlyCompressed)
    {
        DefaultRequestHeaders.Add("User-Agent", $"FortRise/{FortRiseConstants.Version}");
    }

    private static async ValueTask<Stream> OnConnect(
        SocketsHttpConnectionContext ctx, 
        CancellationToken token
    )
    {
        Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };

        try 
        {
            await socket.ConnectAsync(
                new DnsEndPoint(
                    ctx.DnsEndPoint.Host, 
                    ctx.DnsEndPoint.Port,
                    AddressFamily.InterNetwork),
                token
            );

            return new NetworkStream(socket, true);
        }
        catch (Exception)
        {
            socket.Dispose();
            throw;
        }
    }
}



