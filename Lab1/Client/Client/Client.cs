using System.IO.Pipes;
using System.Runtime.CompilerServices;
using MySharedLibrary;

class Client
{
    static void Main()
    {
        // Открытие канала
        using NamedPipeClientStream Client = new(".", "channel", PipeDirection.InOut);
        Client.Connect();

        // Получение данных от сервера
        byte[] bytes = new byte[Unsafe.SizeOf<Structure>()];
        Client.Read(bytes, 0, bytes.Length);
        Structure received_data = Unsafe.As<byte, Structure>(ref bytes[0]);
        Console.WriteLine($"Received data: num = {received_data.num}, flag = {received_data.flag}");

        // Изменение флага
        received_data.flag = true;

        // Отправка измененных данных обратно на сервер
        byte[] modified_bytes = new byte[Unsafe.SizeOf<Structure>()];
        Unsafe.As<byte, Structure>(ref modified_bytes[0]) = received_data;
        Client.Write(modified_bytes, 0, modified_bytes.Length);
    }
}