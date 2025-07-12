namespace uHand2.SDK;

public enum HandCommands : byte
{
    SingleServoMove = 2,
    MultipleServoMove = 3,
    ServoStop = 4,
    FullActionRun = 6,
    FullActionStop = 7,
    FullActionErase = 8,
    ActionDownload = 25,
}
