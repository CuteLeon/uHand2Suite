namespace uHand2.SDK;

public record class HandPacket
{
    public static readonly HandPacket ResetPacket = new()
    {
        Command = HandCommands.MultipleServoMove,
        ServoCount = HandContracts.ServosTotal,
        Time = 1000,
        Servos = [.. Enum.GetValues<HandServos>().Select(x => new Servo(x, HandContracts.AngleDefault))],
    };

    // BinaryPrimitives.WriteInt32BigEndian MemoryStream BinaryWriter stackalloc  Span<>
    public HandCommands Command { get; set; }
    public byte ServoCount { get; set; }
    public ushort Time { get; set; }
    public List<Servo>? Servos { get; set; }
    public Servo? Servo { get; set; }

    public override string ToString()
    {
        return $"[{this.Command}] {this.Time}ms, {this.ServoCount} servos (SNG={this.Servo}; MULT={string.Join(",", this.Servos ?? [])})";
    }
}
