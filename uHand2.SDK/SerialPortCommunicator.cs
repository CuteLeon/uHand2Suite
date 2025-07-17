using System.Buffers;
using System.Diagnostics;
using System.IO.Ports;
using uHand2.Contract;

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
        Debug.Print($"{DateTime.Now:HH:mm:ss.fff} SerialPort PinChanged: {e.EventType}");
    }

    private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
        Debug.Print($"{DateTime.Now:HH:mm:ss.fff} SerialPort ErrorReceived: {e.EventType}");
    }

    private void CommunicatePort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        Debug.Print($"{DateTime.Now:HH:mm:ss.fff} SerialPort DataReceived: {e.EventType}");
        var serialPort = this.CommunicatePort;
        if (!serialPort.IsOpen) return;
        var readBuffer = ArrayPool<byte>.Shared.Rent(serialPort.ReadBufferSize);
        var readLength = serialPort.Read(readBuffer, 0, serialPort.ReadBufferSize);
        if (readLength > 0)
        {
            Debug.Print($"{DateTime.Now:HH:mm:ss.fff} Read: [{readLength}] {string.Join(",", readBuffer.Take(readLength))}");
        }
        ArrayPool<byte>.Shared.Return(readBuffer);
    }

    public bool DetectCommunicatePort()
    {
        var serialPort = this.CommunicatePort;
        var portNames = SerialPort.GetPortNames().OrderDescending();
        var detectPacket = new HandPacket(HandCommands.ActionDownload, actionId: byte.MaxValue, framesCount: 0, frameIndex: 0);
        var detectBytes = HandPacketConvertor.ToBytes(detectPacket);
        var readBuffer = ArrayPool<byte>.Shared.Rent(serialPort.ReadBufferSize);

        try
        {
            var (cursorLeft, cursorTop) = Console.GetCursorPosition();
            foreach (var portName in portNames)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Detecting port: {portName} ...");
                try
                {
                    serialPort.PortName = portName;
                    serialPort.Open();
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                    serialPort.Write(detectBytes, 0, detectBytes.Length);
                    var readLength = serialPort.Read(readBuffer, 0, serialPort.ReadBufferSize);
                    if (readLength > 0 && readBuffer[0] == HandContracts.PackageFlag)
                    {
                        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Detected Valid port: {portName}".PadRight(Console.BufferWidth, ' '));
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Exception] [{portName}] {ex.Message}");
                    if (serialPort.IsOpen)
                        serialPort.Close();
                }
            }
            Console.SetCursorPosition(cursorLeft, cursorTop);
            return false;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(readBuffer);
        }
    }

    public void SendHandPacket(HandPacket packet)
    {
        var serialPort = this.CommunicatePort;
        if (!serialPort.IsOpen) return;

        Debug.Print($"{DateTime.Now:HH:mm:ss.fff} Send HandPacket: {packet}");
        var bytes = HandPacketConvertor.ToBytes(packet);
        if (bytes is null) return;

        Debug.Print($"{DateTime.Now:HH:mm:ss.fff} Write: [{bytes.Length}] {string.Join(",", bytes)}");
        this.CommunicatePort.Write(bytes, 0, bytes.Length);
    }

    public void Dispose()
    {
        this.CommunicatePort.Dispose();
    }
}
