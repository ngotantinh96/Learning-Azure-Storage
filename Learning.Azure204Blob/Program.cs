﻿using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

Console.WriteLine("Azure Blob Storage exercise\n");

// Run the examples asynchronously, wait for the results before proceeding
ProcessAsync().GetAwaiter().GetResult();

Console.WriteLine("Press enter to exit the sample application.");
Console.ReadLine();

static async Task ProcessAsync()
{
    // Copy the connection string from the portal in the variable below.
    string storageConnectionString = "Storage Connection String";

    // Create a client that can authenticate with a connection string
    // And create container
    BlobContainerClient containerClient = await CreateContainer(storageConnectionString);

    await ReadContainerPropertiesAsync(containerClient);
    await AddContainerMetadataAsync(containerClient);
    await ReadContainerMetadataAsync(containerClient);

    // Create a local file in the ./data/ directory for uploading and downloading
    string localPath = "./data/";
    string fileName = "wtfile" + Guid.NewGuid().ToString() + ".txt";
    string localFilePath = Path.Combine(localPath, fileName);

    //Create blob client and upload file
    BlobClient blobClient = await UploadBlob(containerClient, fileName, localFilePath);

    await ListBlobsInContainer(containerClient);

    string downloadFilePath = await DownloadBlob(localFilePath, blobClient);

    await DeleteBlob(containerClient, localFilePath, downloadFilePath);
}

static async Task<BlobContainerClient> CreateContainer(string storageConnectionString)
{
    BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

    // COPY EXAMPLE CODE BELOW HERE
    //Create a unique name for the container
    string containerName = "wtblob" + Guid.NewGuid().ToString();

    // Create the container and return a container client object
    BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
    Console.WriteLine("A container named '" + containerName + "' has been created. " +
        "\nTake a minute and verify in the portal." +
        "\nNext a file will be created and uploaded to the container.");
    Console.WriteLine("Press 'Enter' to continue.");
    Console.ReadLine();
    return containerClient;
}

static async Task ReadContainerPropertiesAsync(BlobContainerClient containerClient)
{
    try
    {
        // Fetch some container properties and write out their values.
        var properties = await containerClient.GetPropertiesAsync();
        Console.WriteLine($"Properties for container {containerClient.Uri}");
        Console.WriteLine($"Public access level: {properties.Value.PublicAccess}");
        Console.WriteLine($"Last modified time in UTC: {properties.Value.LastModified}");
        Console.WriteLine("Press 'Enter' to continue.");
        Console.ReadLine();
    }
    catch (RequestFailedException e)
    {
        Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
        Console.WriteLine(e.Message);
        Console.WriteLine("Press 'Enter' to continue.");
        Console.ReadLine();
    }
}

static async Task AddContainerMetadataAsync(BlobContainerClient containerClient)
{
    try
    {
        Console.WriteLine("Adding container metadata...");
        IDictionary<string, string> metadata = new Dictionary<string, string>();

        // Add some metadata to the container.
        metadata.Add("docType", "textDocuments");
        metadata.Add("category", "guidance");

        // Set the container's metadata.
        await containerClient.SetMetadataAsync(metadata);
        Console.WriteLine("Press 'Enter' to continue.");
        Console.ReadLine();
    }
    catch (RequestFailedException e)
    {
        Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
        Console.WriteLine(e.Message);
        Console.WriteLine("Press 'Enter' to continue.");
        Console.ReadLine();
    }
}

static async Task ReadContainerMetadataAsync(BlobContainerClient containerClient)
{
    try
    {
        var properties = await containerClient.GetPropertiesAsync();

        // Enumerate the container's metadata.
        Console.WriteLine("Container metadata:");
        foreach (var metadataItem in properties.Value.Metadata)
        {
            Console.WriteLine($"\tKey: {metadataItem.Key} \tValue: {metadataItem.Value}");
        }

        Console.WriteLine("Press 'Enter' to continue.");
        Console.ReadLine();
    }
    catch (RequestFailedException e)
    {
        Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
        Console.WriteLine(e.Message);
        Console.WriteLine("Press 'Enter' to continue.");
        Console.ReadLine();
    }
}

static async Task<BlobClient> UploadBlob(BlobContainerClient containerClient, string fileName, string localFilePath) 
{
    // Write text to the file
    await File.WriteAllTextAsync(localFilePath, "Hello, World!");

    // Get a reference to the blob
    BlobClient blobClient = containerClient.GetBlobClient(fileName);

    Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

    // Open the file and upload its data
    using (FileStream uploadFileStream = File.OpenRead(localFilePath))
    {
        await blobClient.UploadAsync(uploadFileStream);
        uploadFileStream.Close();
    }

    Console.WriteLine("\nThe file was uploaded. We'll verify by listing" +
            " the blobs next.");
    Console.WriteLine("Press 'Enter' to continue.");
    Console.ReadLine();
    return blobClient;
}

static async Task ListBlobsInContainer(BlobContainerClient containerClient)
{
    // List blobs in the container
    Console.WriteLine("Listing blobs...");
    await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
    {
        Console.WriteLine("\t" + blobItem.Name);
    }

    Console.WriteLine("\nYou can also verify by looking inside the " +
            "container in the portal." +
            "\nNext the blob will be downloaded with an altered file name.");
    Console.WriteLine("Press 'Enter' to continue.");
    Console.ReadLine();
}

static async Task<string> DownloadBlob(string localFilePath, BlobClient blobClient)
{
    // Download the blob to a local file
    // Append the string "DOWNLOADED" before the .txt extension 
    string downloadFilePath = localFilePath.Replace(".txt", "DOWNLOADED.txt");

    Console.WriteLine("\nDownloading blob to\n\t{0}\n", downloadFilePath);

    // Download the blob's contents and save it to a file
    BlobDownloadInfo download = await blobClient.DownloadAsync();

    using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
    {
        await download.Content.CopyToAsync(downloadFileStream);
    }

    Console.WriteLine("\nLocate the local file in the data directory created earlier to verify it was downloaded.");
    Console.WriteLine("The next step is to delete the container and local files.");
    Console.WriteLine("Press 'Enter' to continue.");
    Console.ReadLine();
    return downloadFilePath;
}

static async Task DeleteBlob(BlobContainerClient containerClient, string localFilePath, string downloadFilePath)
{
    // Delete the container and clean up local files created
    Console.WriteLine("\n\nDeleting blob container...");
    await containerClient.DeleteAsync();

    Console.WriteLine("Deleting the local source and downloaded files...");
    File.Delete(localFilePath);
    File.Delete(downloadFilePath);

    Console.WriteLine("Finished cleaning up.");
}