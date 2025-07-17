using System.ComponentModel;
using System.Diagnostics;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;
using uHand2.Contract;
using uHand2.SDK;

internal class Program
{
    static SerialPortCommunicator communicator = default!;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        const string APIKey = "";

        communicator = new SerialPortCommunicator();
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Detecting valid Serial Port...");
        while (!communicator.DetectCommunicatePort())
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Didn't find valid Serial Port, retry ...");
            await Task.Delay(1000);
        }
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Serial Port Connected.");

        communicator.SendHandPacket(HandPacket.ResetPacket);

        var geminiClientOptions = new GeminiClientOptions
        {
            ApiKey = APIKey,
            ModelId = GeminiModels.Gemini2Flash,
            ApiVersion = GeminiApiVersions.V1Beta,
        };
        var chatOptions = new ChatOptions()
        {
            Tools = [AIFunctionFactory.Create(ControlHand, nameof(ControlHand))]
        };
        var geminiClient = new GeminiChatClient(geminiClientOptions);
        var client = new ChatClientBuilder(geminiClient).UseFunctionInvocation().Build();

        while (true)
        {
            Console.WriteLine("===================================================");
            (int cursorLeft, int cursorTop) = Console.GetCursorPosition();
            Console.Write("Leon: ");
            var userInput = Console.ReadLine();
            if (string.IsNullOrEmpty(userInput)) break;

            Console.SetCursorPosition(cursorLeft, cursorTop);
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Leon: {userInput}");
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Gemini: ");
            try
            {
                await foreach (var update in client.GetStreamingResponseAsync(userInput, chatOptions))
                {
                    Console.Write(update);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Exception] {ex.Message}");
            }
            Console.WriteLine();
        }

        Console.ReadLine();
        communicator.Dispose();
    }

    [Description(
        """
        控制外部右手机械手硬件，通过伺服电机驱动五根手指弯曲/伸直及手腕旋转。
        参数：
            durationMs (uint16): 动作执行时间(毫秒)，默认1000
            thumbAngle (uint16?): 大拇指角度，900(完全弯曲)~2000(完全伸直)，默认null(不动)
            indexFingerAngle (uint16?): 食指角度，900~2000，默认null
            middleFingerAngle (uint16?): 中指角度，900~2000，默认null
            ringFingerAngle (uint16?): 无名指角度，900~2000，默认null
            pinkyFingerAngle (uint16?): 小拇指角度，900~2000，默认null
            wristRotationAngle (uint16?): 手腕旋转角度，500(完全左转)~2500(完全右转)，默认null
        返回值： Boolean - true表示执行成功
        注意：
            角度值越大越伸直/右转，越小越弯曲/左转
            null值表示该部位保持当前姿势不动
            用户未指定参数时使用默认值
        """)]
    static bool ControlHand(ushort durationMs = 1000, ushort? thumbAngle = null, ushort? indexFingerAngle = null, ushort? middleFingerAngle = null, ushort? ringFingerAngle = null, ushort? pinkyFingerAngle = null, ushort? wristRotationAngle = null)
    {
        Debug.Print($"{DateTime.Now:HH:mm:ss.fff} [AIFunctionCalling] [ControlHand]: {durationMs:N0}ms, Thumb:{thumbAngle,-4:N0}, Index:{indexFingerAngle,-4:N0}, Middle:{middleFingerAngle,-4:N0}, Ring:{ringFingerAngle,-4:N0}, Pinky:{pinkyFingerAngle,-4:N0}, Wrist:{wristRotationAngle,-4:N0}");
        var handPacket = new HandPacket() { Command = HandCommands.MultipleServoMove, Time = durationMs, Servos = new List<Servo>(HandContracts.ServosTotal) };
        if (thumbAngle.HasValue) handPacket.Servos.Add(new Servo(HandServos.Thumb, thumbAngle.Value));
        if (indexFingerAngle.HasValue) handPacket.Servos.Add(new Servo(HandServos.IndexFinger, indexFingerAngle.Value));
        if (middleFingerAngle.HasValue) handPacket.Servos.Add(new Servo(HandServos.MiddleFinger, middleFingerAngle.Value));
        if (ringFingerAngle.HasValue) handPacket.Servos.Add(new Servo(HandServos.RingFinger, ringFingerAngle.Value));
        if (pinkyFingerAngle.HasValue) handPacket.Servos.Add(new Servo(HandServos.PinkyFinger, pinkyFingerAngle.Value));
        if (wristRotationAngle.HasValue) handPacket.Servos.Add(new Servo(HandServos.Wrist, wristRotationAngle.Value));
        try
        {
            Debug.Print($"{DateTime.Now:HH:mm:ss.fff} [AIFunctionCalling] [ControlHand]: Sending packet on communicator: {handPacket}");
            communicator.SendHandPacket(handPacket);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AIFunctionCalling] [ControlHand]: Failed to process command: {ex.Message}");
            return false;
        }
    }
}
