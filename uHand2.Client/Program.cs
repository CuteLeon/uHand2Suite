using uHand2.SDK;

namespace uHand2.Client;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var resetPacket = HandPacket.ResetPacket;
        var fuckPacket = new HandPacket() { Command = HandCommands.SingleServoMove, Time = HandContracts.DefaultTime, Servo = new Servo(HandServos.MiddleFinger, HandContracts.FingerAngleMax) };
        var openPacket = new HandPacket() { Command = HandCommands.MultipleServoMove, Time = HandContracts.DefaultTime, Servos = [.. Enum.GetValues<HandServos>().Select(x => new Servo(x, HandContracts.FingerAngleMax))], };
        var closePacket = new HandPacket() { Command = HandCommands.MultipleServoMove, Time = HandContracts.DefaultTime, Servos = [.. Enum.GetValues<HandServos>().Select(x => new Servo(x, HandContracts.FingerAngleMin))], };
        var stopPacket = new HandPacket() { Command = HandCommands.ServoStop, };

        using var communicator = new SerialPortCommunicator();
        var detectPosition = Console.GetCursorPosition();
        Console.WriteLine($"Detecting valid Serial Port...");
        while (!communicator.DetectCommunicatePort())
        {
            Console.SetCursorPosition(detectPosition.Left, detectPosition.Top);
            Console.WriteLine($"Didn't find valid Serial Port, retry ...");
            Thread.Sleep(1000);
        }
        // communicator.SendHandPacket(resetPacket);

        communicator.SendHandPacket(fuckPacket);

        Console.ReadLine();
        communicator.Dispose();
    }
}
