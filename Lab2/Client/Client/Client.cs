using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

class Client
{
    static async Task Main()
    {
        using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "MyPipeServer", PipeDirection.InOut))
        {
            Console.WriteLine("Клиент пытается подключиться к серверу...");
            await pipeClient.ConnectAsync();

            try
            {
                while (true)
                {
                    Console.Write("Введите сообщение (или нажмите Ctrl+C для выхода): ");
                    string input = await Console.In.ReadLineAsync(); // Асинхронный ввод

                    if (input == null) break; // Пользователь нажал Ctrl+C

                    string message = $"({DateTime.Now.ToString("HH:mm:ss")}) {input}"; // Генерируем сообщение с текущим временем
                    byte[] sendData = Encoding.UTF8.GetBytes(message);

                    await pipeClient.WriteAsync(sendData, 0, sendData.Length);
                    Console.WriteLine("Клиент отправил данные: {0}", message);

                    byte[] receiveData = new byte[1024 * 10];
                    int bytesRead = await pipeClient.ReadAsync(receiveData, 0, receiveData.Length);
                    string receivedData = Encoding.UTF8.GetString(receiveData, 0, bytesRead);
                    Console.WriteLine("Клиент получил ответ от сервера: {0}", receivedData);
                }
            }
            catch (IOException)
            {
                // Исключение происходит при нажатии Ctrl+C, игнорируем его и закрываем канал
            }
            finally
            {
                pipeClient.Close();
            }
        }
    }
}
