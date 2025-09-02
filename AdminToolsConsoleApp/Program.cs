using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

// Azure Storage
var connectionString = configuration.GetConnectionString("AzureStorage");
var tableClient = new TableClient(connectionString, "AlertEmails");
