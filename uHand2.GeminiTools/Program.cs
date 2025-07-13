using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;

namespace uHand2.GeminiTools;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var options = new GeminiClientOptions
        {
            ApiKey = GeminiContracts.APIKey,
            ModelId = GeminiModels.Gemini2Flash,
            ApiVersion = GeminiApiVersions.V1Beta,
        };

        IChatClient client = new GeminiChatClient(options);
        while (true)
        {
            Console.WriteLine("===================================================");
            var position = Console.GetCursorPosition();
            Console.Write("Leon: ");
            var userInput = Console.ReadLine();
            Console.SetCursorPosition(position.Left, position.Top);
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Leon:\n\t{userInput}");
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Gemini: ");
            await foreach (var update in client.GetStreamingResponseAsync(userInput))
            {
                Console.Write(update);
            }
        }

        Console.ReadLine();
    }
}
