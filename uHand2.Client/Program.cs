using uHand2.SDK;

namespace uHand2.Client;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var resetPacket = HandPacket.ResetPacket;
        var fuckPacket = new HandPacket() { Command = HandCommands.SingleServoMove, Time = HandContracts.DefaultTime, ServoCount = 1, Servo = new Servo(HandServos.MiddleFinger, HandContracts.FingerAngleMax) };
        var openPacket = new HandPacket() { Command = HandCommands.MultipleServoMove, Time = HandContracts.DefaultTime, ServoCount = HandContracts.ServosTotal, Servos = [.. Enum.GetValues<HandServos>().Select(x => new Servo(x, HandContracts.FingerAngleMax))], };
        var closePacket = new HandPacket() { Command = HandCommands.MultipleServoMove, Time = HandContracts.DefaultTime, ServoCount = HandContracts.ServosTotal, Servos = [.. Enum.GetValues<HandServos>().Select(x => new Servo(x, HandContracts.FingerAngleMin))], };
        var stopPacket = new HandPacket() { Command = HandCommands.ServoStop, };
    }
}
