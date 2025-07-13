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
        var webSocketHost = args.ElementAtOrDefault(0) ?? "ws://localhost:21010/ws";
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} WebSocketHost: {webSocketHost}");

        using var communicator = new SerialPortCommunicator();
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Detecting valid Serial Port...");
        while (!communicator.DetectCommunicatePort())
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Didn't find valid Serial Port, retry ...");
            await Task.Delay(1000);
        }
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Serial Port Connected.");

        var webSocket = default(ClientWebSocket)!;
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Connecting WebSocket...");
        do
        {
            try
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Didn't connect WebSocket [{webSocketHost}], retry ...");
                await Task.Delay(1000);
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri(webSocketHost), CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Failed to connect WebSocket: {ex}");
                webSocket?.Dispose();
            }
        } while (webSocket is null || webSocket.State != WebSocketState.Open);
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} WebSocket Connected.");
        var webSocketCancellation = new CancellationTokenSource();
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
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Starts receiving WebSocket loop...");
        while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Received WebSocket Message: [{result.MessageType}] {result.Count}");
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} WebSocket closed by server: [{result.CloseStatus}] {result.CloseStatusDescription}");
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", cancellationToken);
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [WebSocket] Received Text: {msg}");
                    var handPacket = JsonSerializer.Deserialize<HandPacket>(msg, JsonSerializerOptions.Web);
                    if (handPacket is not null)
                        communicator.SendHandPacket(handPacket);
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [WebSocket] Received binary data: {result.Count} bytes");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} WebSocket receive error: {ex.Message}");
            }
        }
    }
}
