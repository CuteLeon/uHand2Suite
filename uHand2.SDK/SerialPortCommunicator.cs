using System.Diagnostics;
using System.IO.Ports;
using System.Security.Cryptography;

namespace uHand2.SDK;

public class SerialPortCommunicator : IDisposable
{
    protected SerialPort CommunicatePort { get; init; } = new SerialPort()
    {
        ReadTimeout = HandContracts.IOTimeout,
        WriteTimeout = HandContracts.IOTimeout,
        ReadBufferSize = HandContracts.BufferSize,
        WriteBufferSize = HandContracts.BufferSize,
        BaudRate = HandContracts.BaudRate,
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
        var serialPort = this.CommunicatePort;
        if (!serialPort.IsOpen) return;
        var readBuffer = new byte[serialPort.ReadBufferSize];
        var readLength = serialPort.Read(readBuffer, 0, serialPort.ReadBufferSize);
        if (readLength > 0)
        {
            Console.WriteLine(string.Join(",", readBuffer));
        }
    }

    public bool DetectCommunicatePort()
    {
        var serialPort = this.CommunicatePort;
        var portNames = SerialPort.GetPortNames().OrderDescending();
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
                if (serialPort.IsOpen)
                    serialPort.Close();
            }
        }
        return false;
    }

    public void SendHandPacket(HandPacket packet)
    {
        var serialPort = this.CommunicatePort;
        if (!serialPort.IsOpen) return;

        Console.WriteLine($"Send HandPacket: {packet}");
        var bytes = HandPacketConvertor.ToBytes(packet);
        this.CommunicatePort.Write(bytes, 0, bytes.Length);
    }

    public void Dispose()
    {
        this.CommunicatePort.Dispose();
    }
}
