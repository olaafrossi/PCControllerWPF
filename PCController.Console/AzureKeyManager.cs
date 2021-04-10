using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using MvvmCross.Logging;

namespace PCController.Console
{
    public static class AzureKeyManager
    {
        //private readonly static IMvxLog _log;
        // TODO add the MvX Log provider

        public static string GetPassword()
        {
            //_log = logProvider.GetLogFor<GitHubManager>();

            string keyVaultName = PCController.Core.Properties.Settings.Default.AzureKeyVaultName;
            string kvUri = $"https://{keyVaultName}.vault.azure.net";
            //_log.Info("Getting Secret from {kvUri}", kvUri);

            //_log.Info("Have logged something"):
            SecretClient client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

            //_log.Info("trying to get a GitHub client{productInformation}", productInformation);
            System.Console.WriteLine($"Retrieving your secret from {keyVaultName}.");
            Response<KeyVaultSecret> secret = client.GetSecret(PCController.Core.Properties.Settings.Default.AzureKeyVaultGitHubPassword);

            // async version- prob not a good idea
            //var secret = await client.GetSecretAsync(secretName);

            //_log.Info("have gotten the GitHubPassword/Token from Azure KeyVault I won't write the value to the log);
            return secret.Value.Value;
        }
    }
}
