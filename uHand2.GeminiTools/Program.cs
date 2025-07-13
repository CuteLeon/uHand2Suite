using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http.Json;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;
using uHand2.Contract;

namespace uHand2.GeminiTools;

internal class Program
{
    static HttpClient httpClient = default!;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var remoteHost = args.ElementAtOrDefault(0) ?? "http://localhost:21010";
        httpClient = new HttpClient() { BaseAddress = new Uri(remoteHost) };
        Console.WriteLine($"机械手远程控制端点：{remoteHost}");
        var geminiClientOptions = new GeminiClientOptions
        {
            ApiKey = GeminiContracts.APIKey,
            ModelId = GeminiModels.Gemini2Flash,
            ApiVersion = GeminiApiVersions.V1Beta,
        };
        var chatOptions = new ChatOptions()
        {
            Tools = [AIFunctionFactory.Create(ControlHand, nameof(ControlHand))]
        };
        var geminiClient = new GeminiChatClient(geminiClientOptions);
        var client = new ChatClientBuilder(geminiClient)
            .UseFunctionInvocation()
            .Build();

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
            await foreach (var update in client.GetStreamingResponseAsync(userInput, chatOptions))
            {
                Console.Write(update);
            }
            Console.WriteLine();
        }

        Console.ReadLine();
    }

    [Description(
        """
        此工具用于控制一个外部的右手机械手硬件。该机械手包含多个伺服传动装置，能够分别控制五根手指的伸直与弯曲，以及手腕的左右旋转。
        通过传入不同的角度参数，可以使机械手摆出各种手势，同时还能通过时间参数控制动作的快慢。
        返回参数为Boolean类型，true表示姿势执行成功。
        参数说明：
        1. durationMs
            类型: uint16
            用途: 指定机械手完成姿势变化所需耗费的时间，单位为毫秒。这个参数控制动作的速度，值越大动作越慢，值越小动作越快。
            用户未明确说明时，可以静默使用1000毫秒作为此参数的默认值传入此方法中。
            示例值: 1000 (表示1秒), 5000 (表示5秒)
        2. thumbAngle
            类型: 可以为空的uint16
            用途: 控制机械手大拇指的开合角度。
            取值范围: 
                可以为空(null)表示大拇指不做任何移动，保持当前姿势。
                也可以为900~2000之间的任意整数，数值越大表示大拇指越伸直，数值越小表示大拇指越卷曲。
                2000: 大拇指完全伸直。
                900: 大拇指完全卷曲。
            用户未明确说明时，可以静默使用null作为此参数的默认值传入此方法中。
            示例值: 1500 (表示半开合)
        3. indexFingerAngle
            类型: 可以为空的uint16
            用途: 控制机械手食指的开合角度。
            取值范围:
                可以为空(null)表示食指不做任何移动，保持当前姿势。
                也可以为900~2000之间的任意整数，数值越大表示食指越伸直，数值越小表示食指越卷曲。
                2000: 食指完全伸直。
                900: 食指完全卷曲。
            用户未明确说明时，可以静默使用null作为此参数的默认值传入此方法中。
            示例值: 1000
        4. middleFingerAngle
            类型: 可以为空的uint16
            用途: 控制机械手中指的开合角度。
            取值范围:
                可以为空(null)表示中指不做任何移动，保持当前姿势。
                也可以为900~2000之间的任意整数，数值越大表示中指越伸直，数值越小表示中指越卷曲。
                2000: 中指完全伸直。
                900: 中指完全卷曲。
            用户未明确说明时，可以静默使用null作为此参数的默认值传入此方法中。
            示例值: 2000
        5. ringFingerAngle
            类型: 可以为空的uint16
            用途: 控制机械手无名指的开合角度。
            取值范围:
                可以为空(null)表示无名指不做任何移动，保持当前姿势。
                也可以为900~2000之间的任意整数，数值越大表示无名指越伸直，数值越小表示无名指越卷曲。
                2000: 无名指完全伸直。
                900: 无名指完全卷曲。
            用户未明确说明时，可以静默使用null作为此参数的默认值传入此方法中。
            示例值: 1200
        6. pinkyFingerAngle
            类型: 可以为空的uint16
            用途: 控制机械手小拇指的开合角度。
            取值范围:
                可以为空(null)表示小拇指不做任何移动，保持当前姿势。
                也可以为900~2000之间的任意整数，数值越大表示小拇指越伸直，数值越小表示小拇指越卷曲。
                2000: 小拇指完全伸直。
                900: 小拇指完全卷曲。
            用户未明确说明时，可以静默使用null作为此参数的默认值传入此方法中。
            示例值: 900
        7. wristRotationAngle
            类型: 可以为空的uint16
            用途: 控制机械手手腕的旋转角度。
            取值范围:
                可以为空(null)表示手腕不做任何移动，保持当前姿势。
                也可以为500~2500之间的任意整数，数值越大表示手腕越向右旋转，数值越小表示手腕越向左旋转。
                2500: 手腕完全向右旋转。
                500: 手腕完全向左旋转。
            用户未明确说明时，可以静默使用null作为此参数的默认值传入此方法中。
            示例值: 1500 (表示中间位置)
        """)]
    public static async Task<bool> ControlHand(ushort durationMs = 1000, ushort? thumbAngle = null, ushort? indexFingerAngle = null, ushort? middleFingerAngle = null, ushort? ringFingerAngle = null, ushort? pinkyFingerAngle = null, ushort? wristRotationAngle = null)
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
            Debug.Print($"{DateTime.Now:HH:mm:ss.fff} [AIFunctionCalling] [ControlHand]: Sending command to remote: {handPacket}");
            var response = await httpClient.PostAsJsonAsync("/forward", handPacket);
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [AIFunctionCalling] [ControlHand]: Sent command and Received response: [{response.IsSuccessStatusCode}] {response.StatusCode} {response.ReasonPhrase}: {content}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AIFunctionCalling] [ControlHand]: Failed to process command: {ex.Message}");
            return false;
        }
    }
}
