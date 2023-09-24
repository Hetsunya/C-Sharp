using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Server
{
    private static readonly Queue<string> dataQueue = new Queue<string>();
    private static readonly object lockObject = new object();
    private static bool isRunning = true;
    private static readonly string logFilePath = "log.txt"; // Путь к файлу для сохранения данных

    static async Task Main()
    {
        Console.CancelKeyPress += async (sender, e) =>
        {
            e.Cancel = true;
            isRunning = false;

            // Ожидаем завершения обработки данных из очереди
            while (dataQueue.Count > 0)
            {
                await Task.Delay(100);
            }

            // Выводим данные на экран или записываем их в файл
            DisplayOrSaveData();
        };

        // Запускаем асинхронный поток для обработки данных из очереди
        _ = ProcessDataAsync();

        try
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("MyPipeServer", PipeDirection.InOut))
            {
                Console.WriteLine("Сервер ожидает подключения...");
                pipeServer.WaitForConnection();

                byte[] receiveData = new byte[1024 * 10];
                int bytesRead;
                while (isRunning && (bytesRead = await pipeServer.ReadAsync(receiveData, 0, receiveData.Length)) > 0)
                {
                    string receivedData = System.Text.Encoding.UTF8.GetString(receiveData, 0, bytesRead);
                    Console.WriteLine("Сервер получил данные: {0}", receivedData);

                    // Добавляем полученные данные в очередь с учетом приоритета
                    lock (lockObject)
                    {
                        dataQueue.Enqueue(receivedData);
                    }
                }
            }
        }
        finally
        {
            // Закрыть канал при завершении
        }
    }

    // Асинхронный метод для обработки данных из очереди
    static async Task ProcessDataAsync()
    {
        while (isRunning)
        {
            string data = null;
            lock (lockObject)
            {
                if (dataQueue.Count > 0)
                {
                    data = dataQueue.Dequeue();
                }
            }

            if (data != null)
            {
                // Ваша логика обработки данных здесь

                // Пример: Отправка ответа клиенту
                await SendResponseToClient(data);

                // Сохранение полученных данных в буфере
                SaveDataToBuffer(data);
            }

            await Task.Delay(100); // Пауза между обработкой данными
        }
    }

    // Пример: Отправка ответа клиенту
    static async Task SendResponseToClient(string response)
    {
        byte[] sendData = System.Text.Encoding.UTF8.GetBytes(response);
        using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "MyPipeClient", PipeDirection.InOut))
        {
            await pipeClient.ConnectAsync();
            await pipeClient.WriteAsync(sendData, 0, sendData.Length);
        }
    }

    // Сохранение данных в буфере
    static void SaveDataToBuffer(string data)
    {
        // Просто добавляем полученные данные в очередь буфера
        lock (lockObject)
        {
            dataQueue.Enqueue(data);
        }
    }

    // Вывод данных на экран или запись их в файл
    static void DisplayOrSaveData()
    {
        // В данном примере просто выводим данные на экран
        Console.WriteLine("Вывод данных из буфера:");
        while (dataQueue.Count > 0)
        {
            string data = null;
            lock (lockObject)
            {
                if (dataQueue.Count > 0)
                {
                    data = dataQueue.Dequeue();
                }
            }
            if (data != null)
            {
                Console.WriteLine(data);
            }
        }

        // Если нужно записать данные в файл, используйте следующий код:
        // File.WriteAllLines(logFilePath, dataQueue);
    }
}