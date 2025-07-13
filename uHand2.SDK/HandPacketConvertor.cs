using uHand2.Contract;

namespace uHand2.SDK;

public static class HandPacketConvertor
{
    public static byte[] ToBytes(HandPacket packet)
    {
        ushort GetAngle(Servo servo)
        {
            return servo.ServoId == HandServos.Thumb ? (ushort)(HandContracts.FingerAngleMax + HandContracts.FingerAngleMin - servo.Angle) : servo.Angle;
        }

        switch (packet.Command)
        {
            case HandCommands.MultipleServoMove:
                {
                    const byte CommandPrefixLength = 3;
                    var dataLength = HandContracts.PackageFixDataLength + CommandPrefixLength + HandContracts.ServoUnitDataLength * (packet.Servos?.Count ?? 0);
                    var bytes = new byte[HandContracts.PackageFlagLength + dataLength];
                    Array.Fill(bytes, HandContracts.PackageFlag, 0, HandContracts.PackageFlagLength);
                    bytes[2] = (byte)dataLength;
                    bytes[3] = (byte)packet.Command;
                    bytes[4] = (byte)(packet.Servos?.Count ?? 0);
                    bytes[5] = (byte)(packet.Time & 0x00FF);
                    bytes[6] = (byte)(packet.Time >> 8);
                    if (packet.Servos is not null)
                    {
                        var prefixLength = HandContracts.PackageFlagLength + HandContracts.PackageFixDataLength + CommandPrefixLength;
                        for (var index = 0; index < packet.Servos.Count; index++)
                        {
                            var byteIndex = prefixLength + HandContracts.ServoUnitDataLength * index;
                            var servo = packet.Servos[index];
                            var angle = GetAngle(servo);
                            bytes[byteIndex] = (byte)servo.ServoId;
                            bytes[byteIndex + 1] = (byte)(angle & 0x00ff);
                            bytes[byteIndex + 2] = (byte)(angle >> 8);
                        }
                    }
                    return bytes;
                }
            case HandCommands.ActionDownload:
                {
                    const byte CommandPrefixLength = 6;
                    var dataLength = HandContracts.PackageFixDataLength + CommandPrefixLength + HandContracts.ServoUnitDataLength * (packet.Servos?.Count ?? 0);
                    var bytes = new byte[HandContracts.PackageFlagLength + dataLength];
                    Array.Fill(bytes, HandContracts.PackageFlag, 0, HandContracts.PackageFlagLength);
                    bytes[2] = (byte)dataLength;
                    bytes[3] = (byte)packet.Command;
                    bytes[4] = (byte)(packet.ActionId);
                    bytes[5] = (byte)(packet.FramesCount);
                    bytes[6] = (byte)(packet.FrameIndex);
                    bytes[7] = (byte)(packet.Servos?.Count ?? 0);
                    bytes[8] = (byte)(packet.Time & 0x00FF);
                    bytes[9] = (byte)(packet.Time >> 8);
                    if (packet.Servos is not null)
                    {
                        var prefixLength = HandContracts.PackageFlagLength + HandContracts.PackageFixDataLength + CommandPrefixLength;
                        for (var index = 0; index < packet.Servos.Count; index++)
                        {
                            var byteIndex = prefixLength + HandContracts.ServoUnitDataLength * index;
                            var servo = packet.Servos[index];
                            var angle = GetAngle(servo);
                            bytes[byteIndex] = (byte)servo.ServoId;
                            bytes[byteIndex + 1] = (byte)(angle & 0x00ff);
                            bytes[byteIndex + 2] = (byte)(angle >> 8);
                        }
                    }
                    return bytes;
                }
            case HandCommands.FullActionStop:
                {
                    var dataLength = HandContracts.PackageFixDataLength;
                    var bytes = new byte[HandContracts.PackageFlagLength + dataLength];
                    Array.Fill(bytes, HandContracts.PackageFlag, 0, HandContracts.PackageFlagLength);
                    bytes[2] = (byte)dataLength;
                    bytes[3] = (byte)packet.Command;
                    return bytes;
                }
            default:
                throw new InvalidDataException($"Unknown HandPacket: {packet}");
        }
    }
}
