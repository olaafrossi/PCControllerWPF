// Copyright © 2019 Transeric Solutions. All rights reserved.
// Author: Eric David Lynch
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Octokit;
// ReSharper disable CheckNamespace
// ReSharper disable once ArrangeModifiersOrder

namespace PCController.Console
{
    public class Program
    {
        //private static readonly string GitHubIdentity = Assembly
        //    .GetEntryAssembly()
        //    .GetCustomAttribute<AssemblyProductAttribute>()
        //    .Product;

        public static void Main(string[] args)
        {
            GitHubManager manager = new GitHubManager();
            
            manager.GetRepo();
            manager.GetLatestRelease();
            manager.GetRelease();
        }
    }
}
