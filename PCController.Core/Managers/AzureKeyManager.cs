﻿// Created by Three Byte Intemedia, Inc. | project: PCController |
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
            _log.Info("Getting Secret from {kvUri}", kvUri);

            //_log.Info("Have logged something"):
            SecretClient client = new(new Uri(kvUri), new DefaultAzureCredential());

            //_log.Info("trying to get a GitHub client{productInformation}", productInformation);
            System.Console.WriteLine($"Retrieving your secret from {keyVaultName}.");
            Response<KeyVaultSecret> secret = client.GetSecret(Settings.Default.AzureKeyVaultGitHubPassword);

            // async version- prob not a good idea
            //var secret = await client.GetSecretAsync(secretName);

            //_log.Info("have gotten the GitHubPassword/Token from Azure KeyVault I won't write the value to the log);
            return secret.Value.Value;
        }
    }
}