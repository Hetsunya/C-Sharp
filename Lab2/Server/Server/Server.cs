using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace NamedPipeServer
{
    class Program
    {
        private static Queue<string> dataQueue = new Queue<string>();
        private static CancellationTokenSource cts = new CancellationTokenSource();

        static void Main(string[] args)
        {
            using (NamedPipeServerStream serverStream = new NamedPipeServerStream("MyPipe", PipeDirection.Out))
            {
                Console.WriteLine("Сервер запущен. Ожидание клиента...");
                serverStream.WaitForConnection();

                // Создаем поток для отправки данных на клиент
                Thread sendThread = new Thread(() => SendData(serverStream));
                sendThread.Start();

                Console.WriteLine("Нажмите Ctrl+C для завершения.");
                Console.CancelKeyPress += (s, e) =>
                {
                    cts.Cancel();
                    sendThread.Join();
                    serverStream.Close();
                };

                while (!cts.Token.IsCancellationRequested)
                {
                    Console.WriteLine("Введите данные для отправки:");
                    string input = Console.ReadLine();
                    dataQueue.Enqueue(input);
                }
            }
        }

        private static void SendData(NamedPipeServerStream serverStream)
        {
            using (StreamWriter writer = new StreamWriter(serverStream))
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (dataQueue.Count > 0)
                    {
                        string dataToSend = dataQueue.Dequeue();
                        writer.WriteLine(dataToSend);
                        writer.Flush();
                        Console.WriteLine($"Отправлено: {dataToSend}");
                    }
                    else
                    {
                        Thread.Sleep(100); // Ждем, если очередь пуста
                    }
                }
            }
        }
    }
}