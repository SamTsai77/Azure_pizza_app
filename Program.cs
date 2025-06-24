using Microsoft.Azure.Cosmos;
using Azure.Identity;
using Azure.Storage.Blobs;
using CosmosWebApi.Models;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration.GetSection("CosmosDb");
string? account = config["Account"];
string? key = config["Key"];
string? dbName = config["DatabaseName"];
string? containerName = config["ContainerName"];

var blobStorage = builder.Configuration.GetSection("BlobStorage");
string? blobUrl = blobStorage["Url"];


// Use Managed Identity if key is not provided
CosmosClient cosmosClient = string.IsNullOrEmpty(key)
    ? new CosmosClient(account, new DefaultAzureCredential())
    : new CosmosClient(account, key);

builder.Services.AddSingleton(cosmosClient);
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<CosmosClient>();
    return client.GetContainer(dbName, containerName);
});


Console.Out.WriteLine($"BlobStorage URL: {blobUrl}");

if (string.IsNullOrEmpty(blobUrl))
{
    throw new InvalidOperationException("BlobStorage URL is missing or empty in configuration.");
}

// Use DefaultAzureCredential (works with Managed Identity)
var blobServiceClient = new BlobServiceClient(new Uri(blobUrl), new DefaultAzureCredential());
var containerClient = blobServiceClient.GetBlobContainerClient("");
builder.Services.AddSingleton(containerClient);

builder.Services.AddSingleton(blobServiceClient);
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<BlobServiceClient>();
    return client.GetBlobContainerClient("menu");
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
