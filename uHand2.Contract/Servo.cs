namespace uHand2.Contract;

public record struct Servo
{
    public Servo(HandServos servoId, ushort angle)
    {
        this.ServoId = servoId;
        this.Angle = angle;
    }

    public HandServos ServoId;

    public ushort Angle;

    public override readonly string ToString() =>
        $"[{this.ServoId}]@{this.Angle:N0}";
}
