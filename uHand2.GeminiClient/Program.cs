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
        }

        Console.ReadLine();
        communicator.Dispose();
    }

    [Description(
        """
        此方法用于控制一个外部的右手机械手硬件。该机械手包含多个伺服传动装置。
        可选地传入时间参数以控制动作的快慢，可选地传入一个或多个角度参数以分别控制五根手指的伸直与弯曲和手腕的左右旋转。
        返回值为布尔类型，true表示执行成功，false表示执行失败。
        时间参数：参数名称为“durationMs”，为可选uint16类型，表示动作执行时间单位为毫秒，默认值为500，表示500毫秒。
        手指角度参数：可选uint16类型，所有手指参数范围为900至2000之间的整数，900表示手指完全弯曲，2000表示完全伸直，null表示保持当前位置，默认值为null。
        手指角度参数名称：参数“thumbAngle”控制大拇指伸直与卷曲，参数“indexFingerAngle”控制食指伸直与卷曲，参数“middleFingerAngle”控制中指伸直与卷曲，参数“ringFingerAngle”控制无名指伸直与卷曲，参数“pinkyFingerAngle”控制小拇指伸直与卷曲。
        手腕角度参数：参数名称为“wristRotationAngle”，为可选uint16类型，控制手腕旋转角度，范围为500至2500之间的整数，500表示完全左转，2500表示完全右转，1500表示中间位置，null表示保持当前位置，默认值为null。
        使用规则：用户未指定参数时使用默认值，无需询问；直接执行手势，无需确认；可自行推断合适的角度值。
        特别强调，不要询问和确认任何参数，直接执行手势。
        """)]
    static bool ControlHand(ushort durationMs = 500, ushort? thumbAngle = null, ushort? indexFingerAngle = null, ushort? middleFingerAngle = null, ushort? ringFingerAngle = null, ushort? pinkyFingerAngle = null, ushort? wristRotationAngle = null)
    {
        Debug.Print($"{DateTime.Now:HH:mm:ss.fff} [AIFunctionCalling]: {durationMs:N0}ms, Thumb:{thumbAngle,-4:N0}, Index:{indexFingerAngle,-4:N0}, Middle:{middleFingerAngle,-4:N0}, Ring:{ringFingerAngle,-4:N0}, Pinky:{pinkyFingerAngle,-4:N0}, Wrist:{wristRotationAngle,-4:N0}");
        var handPacket = new HandPacket() { Command = HandCommands.MultipleServoMove, Time = durationMs, Servos = new List<Servo>(HandContracts.ServosTotal) };
        if (thumbAngle.HasValue) handPacket.Servos.Add(new Servo(HandServos.Thumb, thumbAngle.Value));
        if (indexFingerAngle.HasValue) handPacket.Servos.Add(new Servo(HandServos.IndexFinger, indexFingerAngle.Value));
        if (middleFingerAngle.HasValue) handPacket.Servos.Add(new Servo(HandServos.MiddleFinger, middleFingerAngle.Value));
        if (ringFingerAngle.HasValue) handPacket.Servos.Add(new Servo(HandServos.RingFinger, ringFingerAngle.Value));
        if (pinkyFingerAngle.HasValue) handPacket.Servos.Add(new Servo(HandServos.PinkyFinger, pinkyFingerAngle.Value));
        if (wristRotationAngle.HasValue) handPacket.Servos.Add(new Servo(HandServos.Wrist, wristRotationAngle.Value));
        try
        {
            Debug.Print($"{DateTime.Now:HH:mm:ss.fff} [AIFunctionCalling] [ControlHand]: Sending packet on communicator: \n\t{handPacket}");
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
