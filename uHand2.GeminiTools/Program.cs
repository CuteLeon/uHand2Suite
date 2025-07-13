using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;

namespace uHand2.GeminiTools;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var options = new GeminiClientOptions { ApiKey = GeminiContracts.APIKey, ModelId = GeminiModels.Gemini2Flash };

        IChatClient client = new GeminiChatClient(options);
        await foreach (var update in client.GetStreamingResponseAsync("What is AI?"))
        {
            Console.Write(update);
        }

        Console.ReadLine();
    }
}
