using System.ComponentModel;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;

namespace uHand2.GeminiTools;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var geminiClientOptions = new GeminiClientOptions
        {
            ApiKey = GeminiContracts.APIKey,
            ModelId = GeminiModels.Gemini2Flash,
            ApiVersion = GeminiApiVersions.V1Beta,
        };
        var chatOptions = new ChatOptions()
        {
            Tools = [AIFunctionFactory.Create(ControlHand), AIFunctionFactory.Create(GetWeather)]
        };
        var geminiClient = new GeminiChatClient(geminiClientOptions);
        var client = new ChatClientBuilder(geminiClient)
            .UseFunctionInvocation()
            .Build();

        while (true)
        {
            Console.WriteLine("===================================================");
            (int cursorLeft, int cursorTop) = Console.GetCursorPosition();
            Console.Write("Leon: ");
            var userInput = Console.ReadLine();
            if (string.IsNullOrEmpty(userInput)) break;

            Console.SetCursorPosition(cursorLeft, cursorTop);
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Leon: {userInput}");
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Gemini: ");
            await foreach (var update in client.GetStreamingResponseAsync(userInput, chatOptions))
            {
                Console.Write(update);
            }
        }

        Console.ReadLine();
    }

    [Description("Control a hand and all its fingers")]
    public static bool ControlHand(int ActionDuration)
    {
        Console.WriteLine($"ControlHand: {ActionDuration}");
        return true;
    }

    [Description("Get current weather")]
    public static string GetWeather()
    {
        return "Sunny and hot.";
    }
}
