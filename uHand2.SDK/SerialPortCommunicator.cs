using System.IO.Ports;

namespace uHand2.SDK;

public class SerialPortCommunicator
{
    protected SerialPort CommunicatePort { get; init; } = new SerialPort()
    {
        ReadTimeout = 200,
        WriteTimeout = 200,
        ReadBufferSize = 1024,
        WriteBufferSize = 1024,
        BaudRate = 9600,
    };

    public SerialPortCommunicator()
    {
        this.CommunicatePort = new SerialPort();
        SerialPort.GetPortNames();
    }

    public void DetectCommunicatePort()
    {
        var serialPort = this.CommunicatePort;
        var portNames = SerialPort.GetPortNames();
        var detectPacket = new HandPacket(HandCommands.ActionDownload, actionId: byte.MaxValue, framesCount: 0, frameIndex: 0);
        foreach (var portName in portNames)
        {
            try
            {
                serialPort.PortName = portName;
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();
                serialPort.Open();
                // TODO: 遍历发送探测信息寻找目标串口，并获取当前各个舵机状态
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to detect port name: {portName}\n{ex}");
            }
        }
    }
}
