namespace uHand2.SDK;

public static class HandContracts
{
    public const int ServosTotal = 6;
    public const int FingerAngleMin = 900; // 大拇指伸展，其余手指卷曲
    public const int FingerAngleDefault = 1500;
    public const int FingerAngleMax = 2000; // 大拇指卷曲，其余手指伸展
    public const int WristAngleMin = 500; // 手腕转向最左边
    public const int WristAngleDefault = 1500;
    public const int WristAngleMax = 2500; // 手腕转向最右边
    public const int ArgumentsMaxLength = 30;
    public const int ActionItemsMaxCount = 255;
    public const ushort DefaultBuffer16 = UInt16.MaxValue;
    public const ushort DefaultTime = 1000;
}
