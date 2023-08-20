// See https://aka.ms/new-console-template for more information
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

string connectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
string containerName = "local-container-tn204";
string blobName = "hello-world.txt";

BlobServiceClient blobServiceClient = new(connectionString);

BlobContainerClient blobContainerClient = await GetBlobContainer(containerName, blobServiceClient);

Uri blobUri = await UploadBlobFile(blobContainerClient, blobName);

await GetListBlobs(containerName, blobContainerClient);

await DownloadBlobByName(connectionString, containerName, blobName);

await DownloadBlobByUri(connectionString, containerName, blobUri);

await DeletContainer(blobServiceClient, containerName);

Console.ReadKey();

#region hmm functions
static async Task DownloadBlobByName(string connectionString, string containerName, string blobName)
{
    string downloadFileName = "downloaded-byname.txt";
    Console.WriteLine($"Downloading blob by name to: {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, downloadFileName)}");

    BlobClient blobClient = new(connectionString, containerName, blobName);

    if(await blobClient.ExistsAsync())
    {
        await blobClient.DownloadToAsync(downloadFileName);
    }
}

static async Task DeletContainer(BlobServiceClient blobServiceClient, string containerName)
{
    Console.WriteLine($"Please press Enter to delete container: {containerName}");
    Console.ReadKey();
    await blobServiceClient.DeleteBlobContainerAsync(containerName);
    Console.WriteLine($"Deleted container: {containerName}");
}

static async Task DownloadBlobByUri(string connectionString, string containerName, Uri blobUri)
{
    string downloadFileName = "downloaded-byuri.txt";
    Console.WriteLine($"Downloading blob by name to: {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, downloadFileName)}");

    BlobClient blobClient = new(blobUri);

    if (await blobClient.ExistsAsync())
    {
        await blobClient.DownloadToAsync(downloadFileName);
    }
}

static async Task<BlobContainerClient> GetBlobContainer(string containerName, BlobServiceClient blobServiceClient)
{
    Console.WriteLine($"Fetching or creating container: {containerName}...");

    BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
    await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);
    return blobContainerClient;
}

static async Task<Uri> UploadBlobFile(BlobContainerClient blobContainerClient, string fileName)
{
    Console.WriteLine($"Uploading blob file...");
    using FileStream fileStream = File.OpenRead("hello-world.txt");
    BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);

    var uploadResult = await blobClient.UploadAsync(fileStream, new BlobHttpHeaders
    {
        ContentType = "text/plain"
    });

    if (uploadResult.Value != null)
    {
        var blobContentInfo = uploadResult.Value;
        Console.WriteLine($"Uploaded blob file name: {blobClient.Name} | uri: {blobClient.Uri} | date: {blobContentInfo.LastModified}");
    }

    return blobClient.Uri;
}

static async Task GetListBlobs(string containerName, BlobContainerClient blobContainerClient)
{
    Console.WriteLine($"Retrieving blobs from container {containerName}");

    await foreach (BlobItem item in blobContainerClient.GetBlobsAsync())
    {
        Console.WriteLine($"Blob file name {item.Name}");
    }
}
#endregion