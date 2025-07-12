namespace uHand2.SDK;

public record class HandPacket
{
    // BinaryPrimitives.WriteInt32BigEndian MemoryStream BinaryWriter stackalloc  Span<>
    public HandCommands Commands { get; set; }
    public byte ServoCount { get; set; }
    public ushort Time { get; set; }
    public List<Servo>? Servos { get; set; }
    public Servo? Servo { get; set; }
}
