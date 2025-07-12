namespace uHand2.Contract;

public static class HandContracts
{
    public const byte PackageFlag = 0x55;
    public const byte PackageFlagLength = 2;
    public const byte PackageFixDataLength = 2;
    public const byte ServoUnitDataLength = 3;
    public const int BufferSize = 1024;
    public const int BaudRate = 9600;
    public const int IOTimeout = 200;

    public const byte ServosTotal = 6;
    public const ushort FingerAngleMin = 900; // 大拇指伸展，其余手指卷曲
    public const ushort FingerAngleDefault = 1500;
    public const ushort FingerAngleMax = 2000; // 大拇指卷曲，其余手指伸展
    public const ushort WristAngleMin = 500; // 手腕转向最左边
    public const ushort WristAngleDefault = 1500;
    public const ushort WristAngleMax = 2500; // 手腕转向最右边
    public const byte ArgumentsMaxLength = 30;
    public const byte ActionItemsMaxCount = 255;
    public const ushort TimeDefault = 1000;
    public const ushort TimeFast = 100;
    public const ushort TimeSlow = 5000;
}
