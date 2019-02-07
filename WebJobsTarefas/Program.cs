using System;
using System.Threading.Tasks;

namespace WebJobsTarefas
{
    class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enviando e-mail para novo usuário!");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
