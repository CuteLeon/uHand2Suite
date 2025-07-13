using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;

namespace uHand2.GeminiTools;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var clientOptions = new GeminiClientOptions
        {
            ApiKey = GeminiContracts.APIKey,
            ModelId = GeminiModels.Gemini2Flash,
            ApiVersion = GeminiApiVersions.V1Beta,
        };
        var chatOptions = new ChatOptions()
        {
        };
        var client = new GeminiChatClient(clientOptions);
        while (true)
        {
            Console.WriteLine("===================================================");
            (int cursorLeft, int cursorTop) = Console.GetCursorPosition();
            Console.Write("Leon: ");
            var userInput = Console.ReadLine();
            if (string.IsNullOrEmpty(userInput)) break;

            Console.SetCursorPosition(cursorLeft, cursorTop);
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Leon:\n\t{userInput}");
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Gemini: ");
            await foreach (var update in client.GetStreamingResponseAsync(userInput, chatOptions))
            {
                Console.Write(update);
            }
        }

        Console.ReadLine();
    }
}
