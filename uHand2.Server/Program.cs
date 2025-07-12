using System.Net.WebSockets;
using System.Text;

namespace uHand2.Server;
public class Program
{
    private static WebSocket? forwardWebSocket;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        var app = builder.Build();

        app.Map("/ws", async context =>
        {
            Console.WriteLine($"/ws: Request: {context.Connection.Id}");
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Program.forwardWebSocket = webSocket;
                Console.WriteLine($"/ws: Connected.");

                var buffer = new byte[1024 * 4];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                        Program.forwardWebSocket = null;
                        Console.WriteLine($"/ws: Disconnected.");
                    }
                    else
                    {
                        Console.WriteLine($"/ws: ReceiveData: [{result.MessageType}] [{result.Count}] {string.Join(",", buffer.Take(result.Count))}");
                    }
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        });

        app.MapPost("/forward", async (HttpContext context) =>
        {
            Console.WriteLine($"/forward: Request: {context.Connection.Id}");
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();
            var bytes = Encoding.UTF8.GetBytes(body);

            if (forwardWebSocket is not null)
            {
                Console.WriteLine($"/forward: Forward data to WebSocket...");
                await forwardWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }

            return Results.Ok();
        });

        app.UseWebSockets();
        app.Run();
    }
}
