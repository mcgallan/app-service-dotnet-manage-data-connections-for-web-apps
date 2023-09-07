// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.Sql;
using Azure.ResourceManager.Sql.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ManageWebAppSqlConnection
{

    public class Program
    {
        private const string Suffix = ".azurewebsites.net";
        private const string Admin = "jsdkadmin";
        private static readonly string Password = Utilities.CreatePassword();

        /**
         * Azure App Service basic sample for managing web apps.
         *  - Create a SQL database in a new SQL server
         *  - Create a web app deployed with Project Nami (WordPress's SQL Server variant)
         *      that contains the app settings to connect to the SQL database
         *  - Update the SQL server's firewall rules to allow the web app to access
         *  - Clean up
         */

        public static async Task RunSample(ArmClient client)
        {
            AzureLocation region = AzureLocation.EastUS;
            string appName = Utilities.CreateRandomName("webapp1");
            string appUrl = appName + Suffix;
            string sqlServerName = Utilities.CreateRandomName("jsdkserver");
            string sqlDbName = Utilities.CreateRandomName("jsdkdb");
            string rgName = Utilities.CreateRandomName("rg1NEMV_");
            string firewallName = Utilities.CreateRandomName("firewall_");
            var lro = await client.GetDefaultSubscription().GetResourceGroups().CreateOrUpdateAsync(Azure.WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.EastUS));
            var resourceGroup = lro.Value;

            try
            {
                //============================================================
                // Create a sql server

                Utilities.Log("Creating SQL server " + sqlServerName + "...");

                var sqlServerCollection = resourceGroup.GetSqlServers();
                var sqlServerData = new SqlServerData(region)
                {
                    AdministratorLogin = Admin,
                    AdministratorLoginPassword = Password,
                };
                var sqlServer_lro = await sqlServerCollection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, sqlServerName, sqlServerData);
                var sqlServer = sqlServer_lro.Value;

                Utilities.Log("Created SQL server " + sqlServer.Data.Name);

                //============================================================
                // Create a sql database for the web app to use

                Utilities.Log("Creating SQL database " + sqlDbName + "...");

                var database_lro = sqlServer.GetSqlDatabase(sqlDbName);
                var database = database_lro.Value;

                Utilities.Log("Created SQL database " + database.Data.Name);

                //============================================================
                // Create a web app with a new app service plan

                Utilities.Log("Creating web app " + appName + "...");

                var webSiteCollection = resourceGroup.GetWebSites();
                var webSiteData = new WebSiteData(region)
                {
                    SiteConfig = new Azure.ResourceManager.AppService.Models.SiteConfigProperties()
                    {
                        WindowsFxVersion = "PricingTier.StandardS1",
                        NetFrameworkVersion = "NetFrameworkVersion.V4_6",
                        PhpVersion = "PhpVersion.V5_6",
                        AppSettings =
                        {
                            new AppServiceNameValuePair()
                            {
                                Name = "ProjectNami.DBHost",
                                Value = sqlServer.Data.FullyQualifiedDomainName
                            },
                            new AppServiceNameValuePair()
                            {
                                Name = "ProjectNami.DBName",
                                Value = database.Data.Name
                            },
                            new AppServiceNameValuePair()
                            {
                                Name = "ProjectNami.DBUser",
                                Value = Admin
                            },
                            new AppServiceNameValuePair()
                            {
                                Name = "ProjectNami.DBPass",
                                Value = Password
                            },
                        }
                    },

                };
                var webSite_lro = await webSiteCollection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, appName, webSiteData);
                var webSite = webSite_lro.Value;
                Utilities.Log("Created web app " + webSite.Data.Name);
                Utilities.Print(webSite);

                //============================================================
                // Allow web app to access the SQL server

                Utilities.Log("Allowing web app " + appName + " to access SQL server...");

                await sqlServer.UpdateAsync( WaitUntil.Completed, new SqlServerPatch());
                //Microsoft.Azure.Management.Sql.Fluent.SqlServer.Update.IUpdate update = server.Update();
                var firstIp = webSite.Data.OutboundIPAddresses.FirstOrDefault().ToString();
                var lastIp = webSite.Data.OutboundIPAddresses.LastOrDefault().ToString();
                var firewallRuleCollection = sqlServer.GetSqlFirewallRules();
                var firewallRuleData = new SqlFirewallRuleData()
                {
                    StartIPAddress = firstIp,
                    EndIPAddress = lastIp,
                };
                var firewallRule_lro = await firewallRuleCollection.CreateOrUpdateAsync(WaitUntil.Completed, firewallName, firewallRuleData);
                var firewallRule = firewallRule_lro.Value;

                Utilities.Log("Firewall rules added for web app " + appName);
                Utilities.PrintSqlServer(sqlServer);

                Utilities.Log("Your WordPress app is ready.");
                Utilities.Log("Please navigate to http://" + appUrl + " to finish the GUI setup. Press enter to exit.");
                Utilities.ReadLine();

            }
            finally
            {
                try
                {
                    Utilities.Log("Deleting Resource Group: " + rgName);
                    await resourceGroup.DeleteAsync(WaitUntil.Completed);
                    Utilities.Log("Deleted Resource Group: " + rgName);
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception g)
                {
                    Utilities.Log(g);
                }
            }
        }

        public static async Task Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);

                // Print selected subscription
                Utilities.Log("Selected subscription: " + client.GetSubscriptions().Id);

                await RunSample(client);
            }
            catch (Exception e)
            {
                Utilities.Log(e);
            }
        }
    }
}