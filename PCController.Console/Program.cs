// Copyright © 2019 Transeric Solutions. All rights reserved.
// Author: Eric David Lynch
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Octokit;
using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;



namespace PCController.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine(AzureKeyManager.GetPassword());


        }
    }

}
