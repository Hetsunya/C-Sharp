using System.IO.Pipes;
using System.Runtime.CompilerServices;
using MySharedLibrary;

class Server
{
    static void Main()
    {
        // Открытие канала
        using NamedPipeServerStream Server = new("channel", PipeDirection.InOut);
        Server.WaitForConnection();

        // Создание сообщения
        Structure msg = new()
        {
            num = 1,
            flag = false,
        };

        // Преобразование в байты
        byte[] bytes = new byte[Unsafe.SizeOf<Structure>()];
        Unsafe.As<byte, Structure>(ref bytes[0]) = msg;
        Server.Write(bytes, 0, bytes.Length);

        // Получение измененных данных от клиента
        byte[] received_bytes = new byte[Unsafe.SizeOf<Structure>()];
        Server.Read(received_bytes, 0, received_bytes.Length);
        Structure received_data = Unsafe.As<byte, Structure>(ref received_bytes[0]);
        Console.WriteLine($"Received data: num = {received_data.num}, flag = {received_data.flag}");
    }
}