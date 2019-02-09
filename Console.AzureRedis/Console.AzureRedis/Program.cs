using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Console.AzureRedis
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var pedidos = await RecuperaPedidos();
            foreach (var pedido in pedidos)
            {
                System.Console.WriteLine($"{pedido.Id} - {pedido.Valor.ToString("C")}");
            }

            System.Console.ReadLine();
        }

        static async Task<IEnumerable<Pedido>> RecuperaPedidos()
        {
            //verificar se existe em memoria
            var redis = await ConnectionMultiplexer.ConnectAsync("cursoaz.redis.cache.windows.net:6380,password=9YMa3IODrcwFoltnzs8rJmY3Jp0Bd4cOUQtdzfAwfNc=,ssl=True,abortConnect=False");
            var db = redis.GetDatabase();

            var pedidosJson = await db.StringGetAsync("pedidos");
            if(!pedidosJson.IsNull)
            {
                //deserializa e retorna
                return JsonConvert.DeserializeObject<List<Pedido>>(pedidosJson);
            }

            //recupera pedidos do banco
            await Task.Delay(TimeSpan.FromSeconds(3));
            var pedidos =  new List<Pedido>()
            {
                new Pedido{ Id = 1, Valor = 150 },
                new Pedido{ Id = 2, Valor = 534 },
                new Pedido{ Id = 3, Valor = 515 },
                new Pedido{ Id = 4, Valor = 313 },
            };

            //armazena em cache
            db.StringSetAsync("pedidos", JsonConvert.SerializeObject(pedidos));

            return pedidos;
        }
    }
}
