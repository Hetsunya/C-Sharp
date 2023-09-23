using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace NamedPipeClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (NamedPipeClientStream clientStream = new NamedPipeClientStream(".", "MyPipe", PipeDirection.In))
            {
                Console.WriteLine("Ожидание подключения к серверу...");
                clientStream.Connect();

                using (StreamReader reader = new StreamReader(clientStream))
                {
                    Console.WriteLine("Соединение установлено. Ожидание данных...");

                    while (true)
                    {
                        string receivedData = reader.ReadLine();
                        if (receivedData != null)
                        {
                            Console.WriteLine($"Получено: {receivedData}");
                            // Здесь можно сохранить данные в буфер или выполнять другие действия
                        }
                    }
                }
            }
        }
    }
}