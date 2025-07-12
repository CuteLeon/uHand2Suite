using System.Text.Json.Serialization;

namespace uHand2.Contract;

public record class HandPacket
{
    public static readonly HandPacket ResetPacket = new()
    {
        Command = HandCommands.MultipleServoMove,
        Time = HandContracts.TimeFast,
        Servos = [.. Enum.GetValues<HandServos>().Select(x => new Servo(x, HandContracts.FingerAngleDefault))],
    };
    public static readonly HandPacket FuckPacket = new()
    {
        Command = HandCommands.MultipleServoMove,
        Time = HandContracts.TimeFast,
        Servos = [.. Enum.GetValues<HandServos>().Select(x => new Servo(x, x == HandServos.Wrist ? HandContracts.WristAngleDefault : x == HandServos.MiddleFinger ? GetAngleStraighten(x) : GetAngleCurl(x)))],
    };
    public static readonly HandPacket OpenPacket = new()
    {
        Command = HandCommands.MultipleServoMove,
        Time = HandContracts.TimeDefault,
        Servos = [.. Enum.GetValues<HandServos>().Select(x => new Servo(x, x == HandServos.Wrist ? HandContracts.WristAngleDefault : GetAngleStraighten(x)))],
    };
    public static readonly HandPacket FistPacket = new()
    {
        Command = HandCommands.MultipleServoMove,
        Time = HandContracts.TimeDefault,
        Servos = [.. Enum.GetValues<HandServos>().Select(x => new Servo(x, x == HandServos.Wrist ? HandContracts.WristAngleDefault : GetAngleCurl(x)))],
    };

    public static ushort GetAngleStraighten(HandServos servo)
        => servo == HandServos.Thumb ? HandContracts.FingerAngleMin : HandContracts.FingerAngleMax;

    public static ushort GetAngleCurl(HandServos servo)
        => servo == HandServos.Thumb ? HandContracts.FingerAngleMax : HandContracts.FingerAngleMin;

    public HandPacket(
        HandCommands command = HandCommands.MultipleServoMove,
        ushort time = HandContracts.TimeDefault,
        List<Servo>? servos = default,
        Servo? servo = default,
        byte actionId = 0,
        byte framesCount = 0,
        byte frameIndex = 0)
    {
        this.Command = command;
        this.Time = time;
        this.Servos = servos;
        this.Servo = servo;
        this.ActionId = actionId;
        this.FramesCount = framesCount;
        this.FrameIndex = frameIndex;
    }

    // BinaryPrimitives.WriteInt32BigEndian MemoryStream BinaryWriter stackalloc  Span<>
    public HandCommands Command { get; set; }
    public ushort Time { get; set; }
    public List<Servo>? Servos { get; set; }
    public Servo? Servo { get; set; }
    public byte ActionId { get; set; }
    public byte FramesCount { get; set; }
    public byte FrameIndex { get; set; }

    public override string ToString()
    {
        return $"[{this.Command}] {this.Time}ms (SNG={this.Servo}; MULT={string.Join(",", this.Servos ?? [])})";
    }
}
