using System;
using System.IO.Pipes;
using System.Text;

class ClientApp
{
    static void Main()
    {
        try
        {
            while (true)
            {
                string inputData = Console.ReadLine(); // Получаем данные от сервера

                if (inputData == null) break; // Пользователь нажал Ctrl+C

                // Разбираем входные данные (предполагаем, что сервер отправляет значение x)
                double x = double.Parse(inputData);

                // Вычисляем значение функции -2 * sin(x)
                double result = -2 * Math.Sin(x);

                // Выводим результат на стандартный вывод (консоль)
                Console.WriteLine(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }
}

