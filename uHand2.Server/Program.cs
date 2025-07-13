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
            var connection = context.Connection;
            var request = context.Request;
            Console.WriteLine($"""
                    {DateTime.Now:HH:mm:ss.fff} /ws: Request: 
                        Connection: {connection.Id} [{connection.RemoteIpAddress}:{connection.RemotePort}]
                        EndPoint: {context.GetEndpoint()?.DisplayName}
                        Request: {request.Scheme} {request.Method} {request.PathBase} {request.Path} {request.QueryString} [{request.ContentLength}] {request.ContentType}
                    """);
            if (context.WebSockets.IsWebSocketRequest)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} /ws: Starts WebSocket ...");
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Program.forwardWebSocket = webSocket;
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} /ws: Connected: [{webSocket.SubProtocol}] {webSocket.State}");

                var buffer = new byte[1024 * 4];
                while (webSocket.State == WebSocketState.Open)
                {
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} /ws: Starts receiving WebSocket loop ...");
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} /ws: Received WebSocket Message: [{result.MessageType}] {result.Count}");
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                        Program.forwardWebSocket = null;
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} /ws: WebSocket Disconnect: [{result.CloseStatus}] {result.CloseStatusDescription}");
                    }
                    else
                    {
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} /ws: ReceiveData: [{result.MessageType}] [{result.Count}] {string.Join(",", buffer.Take(result.Count))}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} /ws: Invalid Request: [{string.Join(",", context.WebSockets.WebSocketRequestedProtocols)}]");
                context.Response.StatusCode = 400;
            }
        });

        app.MapPost("/forward", async (HttpContext context) =>
        {
            try
            {
                var connection = context.Connection;
                var request = context.Request;
                Console.WriteLine($"""
                    {DateTime.Now:HH:mm:ss.fff} /forward: Request: 
                        Connection: {connection.Id} [{connection.RemoteIpAddress}:{connection.RemotePort}]
                        EndPoint: {context.GetEndpoint()?.DisplayName}
                        Request: {request.Scheme} {request.Method} {request.PathBase} {request.Path} {request.QueryString} [{request.ContentLength}] {request.ContentType}
                    """);
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                var body = await reader.ReadToEndAsync();
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} /forward: Received Request Body: [{body.Length}] \n-----------------------------------\n{body}\n====================================");

                if (forwardWebSocket is not null)
                {
                    var bytes = Encoding.UTF8.GetBytes(body);
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} /forward: Forward request body to WebSocket: {bytes.Length:N0} bytes");
                    await forwardWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }

                return Results.Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} /forward: Failed to handle Request: {ex}");
                return Results.BadRequest(ex.Message);
            }
        });
        app.MapGet("/forward", async (HttpContext context) =>
        {
            try
            {
                var connection = context.Connection;
                var request = context.Request;
                Console.WriteLine($"""
                    {DateTime.Now:HH:mm:ss.fff} /forward: Request: 
                        Connection: {connection.Id} [{connection.RemoteIpAddress}:{connection.RemotePort}]
                        EndPoint: {context.GetEndpoint()?.DisplayName}
                        Request: {request.Scheme} {request.Method} {request.PathBase} {request.Path} {request.QueryString} [{request.ContentLength}] {request.ContentType}
                    """);

                var handPacket = context.Request.Query["HandPacket"].ToString();
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} /forward: Received Request Body: [{handPacket.Length}] \n-----------------------------------\n{handPacket}\n====================================");

                if (forwardWebSocket is not null)
                {
                    var bytes = Encoding.UTF8.GetBytes(handPacket);
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} /forward: Forward request body to WebSocket: {bytes.Length:N0} bytes");
                    await forwardWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }

                return Results.Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} /forward: Failed to handle Request: {ex}");
                return Results.BadRequest(ex.Message);
            }
        });

        app.UseWebSockets();
        app.Run();
    }
}
