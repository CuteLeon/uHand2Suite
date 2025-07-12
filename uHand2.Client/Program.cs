using System.Text.Json;
using uHand2.SDK;

namespace uHand2.Client;

internal class Program
{
    static void Main(string[] args)
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

        communicator.SendHandPacket(HandPacket.ResetPacket);
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

        Console.ReadLine();
        communicator.Dispose();
    }
}
