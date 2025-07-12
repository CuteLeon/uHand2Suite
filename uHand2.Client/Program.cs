using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using uHand2.Contract;
using uHand2.SDK;

namespace uHand2.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        using var communicator = new SerialPortCommunicator();
        var detectPosition = Console.GetCursorPosition();
        Console.WriteLine($"Detecting valid Serial Port...");
        while (!communicator.DetectCommunicatePort())
        {
            Console.SetCursorPosition(detectPosition.Left, detectPosition.Top);
            Console.WriteLine($"Didn't find valid Serial Port, retry ...");
            Thread.Sleep(1000);
        }

        var webSocket = new ClientWebSocket();
        var webSocketCancellation = new CancellationTokenSource();
        while (webSocket.State != WebSocketState.Open)
        {
            try
            {
                await Task.Delay(1000);
                await webSocket.ConnectAsync(new Uri("ws://localhost:10010/ws"), webSocketCancellation.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect WebSocket: {ex}");
            }
        }
        _ = Task.Run(() => ReceiveWebSocketLoopAsync(webSocket, communicator, webSocketCancellation.Token));

        communicator.SendHandPacket(HandPacket.ResetPacket);
        /*
        Thread.Sleep(100);
        communicator.SendHandPacket(HandPacket.FistPacket);
        Thread.Sleep(1000);
        Thread.Sleep(1000);
        communicator.SendHandPacket(HandPacket.FuckPacket);
        Thread.Sleep(100);
        Thread.Sleep(1000);
        communicator.SendHandPacket(HandPacket.OpenPacket);
        Thread.Sleep(1000);
        Thread.Sleep(1000);
        communicator.SendHandPacket(HandPacket.ResetPacket);
         */

        Console.ReadLine();
        webSocketCancellation.Cancel();
        communicator.Dispose();
        webSocket.Dispose();
    }

    private static async Task ReceiveWebSocketLoopAsync(WebSocket webSocket, SerialPortCommunicator communicator, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("WebSocket closed by server.");
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", cancellationToken);
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"[WebSocket] Received: {msg}");
                    var handPacket = JsonSerializer.Deserialize<HandPacket>(msg);
                    communicator.SendHandPacket(handPacket);
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    Console.WriteLine($"[WebSocket] Received binary data: {result.Count} bytes");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket receive error: {ex.Message}");
            }
        }
    }
}
