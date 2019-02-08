using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Console.Queues
{
    class Program
    {
        private const string nameQueue = "envia-email";
        static async Task Main(string[] args)
        {
            //criar o objeto da conta
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=cursoazure02;AccountKey=sGrQqYBaCn/NGpRCL522JyqcNyLWxBXt6L3P/aHRHReAmMzSrVQbHMxXbBZ+9jKUBVcOvad0FdJSyOIzdaLGZg==;EndpointSuffix=core.windows.net");

            //criar um queueClient
            var queueClient = storageAccount.CreateCloudQueueClient();

            //envia uma mensagem para fila
            //await ApiEnviaMensagem(queueClient);
            await ApiEnviaMensagemJson(queueClient);

            //recupera número de mensagens na fila
            await ContaMensagens(queueClient);

            //checar mensagem sem marcalas como lida
            await ChecaMensagens(queueClient);

            //processar mensagens
            //await WebJobProcessaMensagens(queueClient);
            await WebJobProcessaMensagensJson(queueClient);

            System.Console.ReadLine();
        }

        static async Task ChecaMensagens(CloudQueueClient queueClient)
        {
            var queue = queueClient.GetQueueReference(nameQueue);
            var messages = await queue.PeekMessagesAsync(10);

            foreach (var msg in messages)
            {
                var content = msg.AsString;
                System.Console.WriteLine($"Enviando email do cliente: {content}");
            }
        }

        static async Task WebJobProcessaMensagens(CloudQueueClient queueClient)
        {
            //a quantidade de operações que é cobrado no preço do azure é considerado uma leitura da mensagem como operação.
            //posso pegar uma mensagem ou vários, então é melhor pegar o lote de operação
            var queue = queueClient.GetQueueReference(nameQueue);

            var messages = await queue.GetMessagesAsync(10); //TimeSpan.FromSeconds(10)); //se passar de 10 segundos eu não deletar a mensagem ela volta para fila
            Parallel.ForEach(messages, async (msg) => await EnviarEmail(msg, queue));
        }

        static async Task EnviarEmail(CloudQueueMessage msg, CloudQueue queue)
        {
            //Envio de Email
            var content = msg.AsString;

            System.Console.WriteLine($"Enviando email do cliente: {content}");
            await Task.Delay(TimeSpan.FromSeconds(1));
            System.Console.WriteLine($"Mensagem enviada!");

            //apaga a mensagem da queue
            await queue.DeleteMessageAsync(msg);
        }

        static async Task ContaMensagens(CloudQueueClient queueClient)
        {
            var queue = queueClient.GetQueueReference(nameQueue);
            await queue.FetchAttributesAsync();
            System.Console.WriteLine($"Mensagens na fila {queue.ApproximateMessageCount}");
        }

        static async Task ApiEnviaMensagem(CloudQueueClient queueClient)
        {
            var queue = queueClient.GetQueueReference(nameQueue);
            //todo: sempre colocar este código no startup project
            await queue.CreateIfNotExistsAsync();

            var message = CriarMensagem();
            await queue.AddMessageAsync(message);

            System.Console.WriteLine($"Mensagem enviada para a fila: {message.Id}");
        }

        private static CloudQueueMessage CriarMensagem()
        {
            var rnd = new Random();
            var message = new CloudQueueMessage($"client-id-{rnd.Next(100)}");
            return message;
        }

        #region Json
        static async Task ApiEnviaMensagemJson(CloudQueueClient queueClient)
        {
            var queue = queueClient.GetQueueReference(nameQueue);
            //todo: sempre colocar este código no startup project
            await queue.CreateIfNotExistsAsync();

            //serializa json ou xml - conteúdo tem que ser string
            var emailModel = new EmailModel { UserId = 55, Email = "thalliscs@teste.com.br" };
            var messageJson = new CloudQueueMessage(JsonConvert.SerializeObject(emailModel));
            await queue.AddMessageAsync(messageJson);

            System.Console.WriteLine($"Mensagem enviada para a fila: {messageJson.Id}");
        }

        static async Task WebJobProcessaMensagensJson(CloudQueueClient queueClient)
        {
            var queue = queueClient.GetQueueReference(nameQueue);

            var messages = await queue.GetMessagesAsync(10);
            Parallel.ForEach(messages, async (msg) => await EnviaMensagemJson(msg, queue));
        }

        private static async Task EnviaMensagemJson(CloudQueueMessage msg, CloudQueue queue)
        {
            System.Console.WriteLine($"Próxima leitura disponível: {msg.NextVisibleTime.Value.ToLocalTime()}");

            var emailModel = JsonConvert.DeserializeObject<EmailModel>(msg.AsString);
            System.Console.WriteLine($"Enviando email do cliente: {emailModel.Email} de id: {emailModel.UserId}");
            await Task.Delay(TimeSpan.FromSeconds(1));
            System.Console.WriteLine($"Mensagem enviada!");

            await queue.DeleteMessageAsync(msg);
        }
        #endregion


    }
}
