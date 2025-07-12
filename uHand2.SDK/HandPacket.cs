namespace uHand2.SDK;

public record class HandPacket
{
    public static readonly HandPacket ResetPacket = new()
    {
        Command = HandCommands.MultipleServoMove,
        ServoCount = HandContracts.ServosTotal,
        Time = 1000,
        Servos = [.. Enum.GetValues<HandServos>().Select(x => new Servo(x, HandContracts.FingerAngleDefault))],
    };

    public HandPacket(
        HandCommands command = HandCommands.SingleServoMove,
        byte servoCount = 1,
        ushort time = HandContracts.DefaultTime,
        List<Servo>? servos = default,
        Servo? servo = default,
        byte actionId = 0,
        byte framesCount = 0,
        byte frameIndex = 0)
    {
        this.Command = command;
        this.ServoCount = servoCount;
        this.Time = time;
        this.Servos = servos;
        this.Servo = servo;
        this.ActionId = actionId;
        this.FramesCount = framesCount;
        this.FrameIndex = frameIndex;
    }

    // BinaryPrimitives.WriteInt32BigEndian MemoryStream BinaryWriter stackalloc  Span<>
    public HandCommands Command { get; set; }
    public byte ServoCount { get; set; }
    public ushort Time { get; set; }
    public List<Servo>? Servos { get; set; }
    public Servo? Servo { get; set; }
    public byte ActionId { get; set; }
    public byte FramesCount { get; set; }
    public byte FrameIndex { get; set; }

    public override string ToString()
    {
        return $"[{this.Command}] {this.Time}ms, {this.ServoCount} servos (SNG={this.Servo}; MULT={string.Join(",", this.Servos ?? [])})";
    }
}
