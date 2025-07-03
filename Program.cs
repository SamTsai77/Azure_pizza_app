using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration.GetSection("CosmosDb");
string? account = config["Account"];
string? dbName = config["DatabaseName"];
string? containerName = config["ContainerName"];

var blobStorage = builder.Configuration.GetSection("BlobStorage");
string? blobUrl = blobStorage["Url"];

// Use Managed Identity if key is not provided

Console.Out.WriteLine($"account: {account}");
CosmosClient cosmosClient = new CosmosClient(account, new DefaultAzureCredential());
Console.Out.WriteLine($"cosmosClient: {cosmosClient}");

builder.Services.AddSingleton(cosmosClient);
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<CosmosClient>();
    Console.Out.WriteLine($"dbName: {dbName}");
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





// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
