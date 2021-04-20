// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 04 09
// by Olaaf Rossi

using System;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using MvvmCross.Logging;
using PCController.Core.Properties;

namespace PCController.Core.Managers
{
    public class AzureKeyManager
    {
        private readonly IMvxLog _log;

        public AzureKeyManager(IMvxLogProvider logProvider)
        {
            _log = logProvider.GetLogFor<AzureKeyManager>();
        }

        public string GetPassword()
        {
            string keyVaultName = Settings.Default.AzureKeyVaultName;
            string kvUri = $"https://{keyVaultName}.vault.azure.net";
            

            Response<KeyVaultSecret> secret;

            try
            {
                SecretClient client = new(new Uri(kvUri), new DefaultAzureCredential());
                secret = client.GetSecret(Settings.Default.AzureKeyVaultGitHubPassword);
                _log.Info("Got Azure secret!");
            }
            catch (Exception e)
            {
                _log.ErrorException("Getting Azure secret failed", e);
                secret = null;
            }

            // async version- prob not a good idea
            //var secret = await client.GetSecretAsync(secretName);

            return secret.Value.Value;
        }
    }
}