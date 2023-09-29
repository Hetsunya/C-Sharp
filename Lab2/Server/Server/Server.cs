using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Collections;
using MySharedLibrary;

class Server
{
    static PriorityQueue<Structure> priorityQueue = new PriorityQueue<Structure>(Comparer<Structure>.Create((x, y) => x.num.CompareTo(y.num)));
    static List<Structure> receivedDataBuffer = new List<Structure>();
    static object bufferLock = new object();

    static async Task Main()
    {
        using NamedPipeServerStream server = new NamedPipeServerStream("channel", PipeDirection.InOut);
        var cancellationTokenSource = new CancellationTokenSource();

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        var processDataTask = Task.Run(() => ProcessData(cancellationTokenSource.Token));

        server.WaitForConnection();

        try
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                Structure msg = new Structure
                {
                    num = 1,
                    flag = false,
                };

                EnqueueData(msg);

                byte[] bytes = new byte[Unsafe.SizeOf<Structure>()];
                Unsafe.As<byte, Structure>(ref bytes[0]) = msg;
                server.Write(bytes, 0, bytes.Length);

                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            await processDataTask;
        }
    }

    static void EnqueueData(Structure data)
    {
        priorityQueue.Enqueue(data);
    }

    static async Task ProcessData(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (priorityQueue.Count > 0)
            {
                Structure data = priorityQueue.Dequeue();

                lock (bufferLock)
                {
                    receivedDataBuffer.Add(data);
                }

                Console.WriteLine($"Processing data: num = {data.num}, flag = {data.flag}");

                // Здесь вы можете добавить дополнительную обработку данных, сохранение в файл, и т.д.
                // ...

                await Task.Delay(500);
            }
            else
            {
                await Task.Delay(100);
            }
        }

        // После завершения обработки данных, выведем их в консоль
        lock (bufferLock)
        {
            Console.WriteLine("Received data buffer:");
            foreach (var item in receivedDataBuffer)
            {
                Console.WriteLine($"num = {item.num}, flag = {item.flag}");
            }
        }
    }
}

