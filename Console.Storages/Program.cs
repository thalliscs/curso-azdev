using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading.Tasks;

namespace Console.Storages
{
    class Program
    {
        const string name = "comprovantes";

        static async Task Main(string[] args)
        {
            //criar o objeto da conta
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=cursoazure02;AccountKey=sGrQqYBaCn/NGpRCL522JyqcNyLWxBXt6L3P/aHRHReAmMzSrVQbHMxXbBZ+9jKUBVcOvad0FdJSyOIzdaLGZg==;EndpointSuffix=core.windows.net");

            //criar um blob client
            var blobClient = storageAccount.CreateCloudBlobClient();

            //listart contaier
            await ListaContainers(blobClient);

            //criar container
            await CriaContainer(blobClient);

            //criar blob e fazer upload
            await CriaBlob(blobClient);

            //download blob
            await Downloadblob(blobClient);

            //gerar sas token para acesso público
            CriaSasToken(blobClient);

            //por boas práticas todas as criações de pastas e containers colocar no startup project da aplicação, por que senão vai chamar uma api externa toda vez que fizer um upload
            System.Console.ReadLine();
        }

        static void CriaSasToken(CloudBlobClient blobClient)
        {
            var container = blobClient.GetContainerReference(name);
            var blob = container.GetBlockBlobReference("05-2018.png");

            var sasToken = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = DateTime.Today.AddDays(-1),
                SharedAccessExpiryTime = DateTime.Today.AddDays(+1),
                Permissions = SharedAccessBlobPermissions.Read //| SharedAccessBlobPermissions.Write
            });

            System.Console.WriteLine($"Sas Token : {sasToken}");
        }

        static async Task Downloadblob(CloudBlobClient blobClient)
        {
            var container = blobClient.GetContainerReference(name);
            var blob = container.GetBlockBlobReference("05-2018.png");

            var nameFile = $"05-2018-{DateTime.Now.Second}.png";
            await blob.DownloadToFileAsync($"C:\\Users\\THALLIS\\Desktop\\Introducao Azure\\ApiCurso\\curso-azdev\\Console.Storages\\bin\\Debug\\netcoreapp2.1\\{nameFile}", System.IO.FileMode.CreateNew);
        }

        static async Task CriaBlob(CloudBlobClient blobClient)
        {
            var container = blobClient.GetContainerReference(name);
            var blob = container.GetBlockBlobReference("05-2018.png");

            var exists = await blob.ExistsAsync();

            if (!exists)
                await blob.UploadFromFileAsync(@"C:\Users\THALLIS\Desktop\comprovantes\05-2018.png");

            System.Console.WriteLine($"Url para download {blob.Uri}");
        }

        static async Task CriaContainer(CloudBlobClient blobClient)
        {
            var container = blobClient.GetContainerReference(name);
            await container.CreateIfNotExistsAsync();

            System.Console.WriteLine($"Container {name} created");
        }

        static async Task ListaContainers(CloudBlobClient blobClient)
        {
            //listar containers 
            var containers = await blobClient.ListContainersSegmentedAsync(null);

            //retorna paginado entao nesse caso poderia passar um novo token para retornar as demais informações
            foreach (var container in containers.Results)
            {
                System.Console.WriteLine(container.Name);
            }
        }
    }
}
