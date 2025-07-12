using System.Diagnostics;
using System.IO.Ports;
using System.Security.Cryptography;

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
        var serialPort = this.CommunicatePort;
        serialPort.DataReceived += this.CommunicatePort_DataReceived;
        serialPort.ErrorReceived += this.SerialPort_ErrorReceived;
        serialPort.PinChanged += this.SerialPort_PinChanged;
        SerialPort.GetPortNames();
    }

    private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
    {
        Console.WriteLine($"SerialPort PinChanged: {e.EventType}");
    }

    private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
        Console.WriteLine($"SerialPort ErrorReceived: {e.EventType}");
    }

    private void CommunicatePort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        Console.WriteLine($"SerialPort DataReceived: {e.EventType}");
    }

    public bool DetectCommunicatePort()
    {
        var serialPort = this.CommunicatePort;
        var portNames = SerialPort.GetPortNames();
        var detectPacket = new HandPacket(HandCommands.ActionDownload, actionId: byte.MaxValue, framesCount: 0, frameIndex: 0);
        var detectBytes = HandPacketConvertor.ToBytes(detectPacket);

        foreach (var portName in portNames)
        {
            Console.WriteLine($"Detecting port: {portName} ...");
            try
            {
                serialPort.PortName = portName;
                serialPort.Open();
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();
                serialPort.Write(detectBytes, 0, detectBytes.Length);
                var readBuffer = new byte[serialPort.ReadBufferSize];
                var readLength = serialPort.Read(readBuffer, 0, serialPort.ReadBufferSize);
                if (readLength > 0 && readBuffer[0] == HandContracts.PackageFlag)
                {
                    Console.WriteLine($"Valid port: {portName}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to detect port name: {portName}\n{ex}");
            }
            finally
            {
                if (serialPort.IsOpen)
                    serialPort.Close();
            }
        }
        return false;
    }
}
