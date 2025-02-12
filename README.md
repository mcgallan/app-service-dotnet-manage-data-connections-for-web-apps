---
page_type: sample
languages:
- csharp
products:
- azure
extensions:
  services: App-Service
  platforms: dotnet
---

# Getting started on managing data connections (such as SQL Database and Redis Cache) for Web Apps in C# #

 Azure App Service basic sample for managing web apps.
  - Create a SQL database in a new SQL server
  - Create a web app deployed with Project Nami (WordPress's SQL Server variant)
      that contains the app settings to connect to the SQL database
  - Update the SQL server's firewall rules to allow the web app to access
  - Clean up


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/app-service-dotnet-manage-data-connections-for-web-apps.git

    cd app-service-dotnet-manage-data-connections-for-web-apps

    dotnet build

    bin\Debug\net452\ManageWebAppSqlConnection.exe

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.