using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinterRose.NetworkServer.Connections;

public class TunnelPair(NetworkConnection a, NetworkConnection b)
{
    public NetworkConnection A => a;
    public NetworkConnection B => b;

    private const string END_MARKER = "<TUNNEL.END>";
    private readonly byte[] END_MARKER_BYTES = Encoding.UTF8.GetBytes(END_MARKER);
    private const int BUFFER_SIZE = 8192;

    public bool IsAorB(NetworkConnection con)
    {
        if (a.Identifier != con.Identifier)
            return b.Identifier == con.Identifier;
        return true;
    }

    public async Task StartTunnelAsync(CancellationToken cancellationToken = default)
    {
        // Start both directions
        var taskAtoB = PipeStreamAsync(a.GetStream(), b.GetStream(), a, b, cancellationToken);
        var taskBtoA = PipeStreamAsync(b.GetStream(), a.GetStream(), b, a, cancellationToken);

        await Task.WhenAny(taskAtoB, taskBtoA);
    }

    private async Task PipeStreamAsync(Stream source, Stream destination, NetworkConnection sourceCon, NetworkConnection destCon, CancellationToken token)
    {
        var buffer = new byte[BUFFER_SIZE];
        int endMarkerMatch = 0;

        try
        {
            while (!token.IsCancellationRequested)
            {
                int bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, token);
                if (bytesRead == 0)
                    break;

                // Check for END_MARKER in the stream
                for (int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == END_MARKER_BYTES[endMarkerMatch])
                    {
                        endMarkerMatch++;
                        if (endMarkerMatch == END_MARKER_BYTES.Length)
                        {
                            // Tunnel close detected
                            break;
                        }
                    }
                    else
                    {
                        if (endMarkerMatch > 0)
                        {
                            await destination.WriteAsync(END_MARKER_BYTES, 0, endMarkerMatch, token);
                            endMarkerMatch = 0;
                        }
                        await destination.WriteAsync(buffer, i, 1, token);
                    }
                }

                if (endMarkerMatch == END_MARKER_BYTES.Length)
                {
                    // Let the other client know
                    await destination.WriteAsync(Encoding.UTF8.GetBytes(END_MARKER), token);
                    break;
                }

                await destination.FlushAsync(token);
            }
        }
        catch (IOException) { /* connection closed */ }
        catch (ObjectDisposedException) { /* stream was closed */ }
    }
}