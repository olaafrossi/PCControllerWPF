// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 04 07
// by Olaaf Rossi

using System;
using System.Reflection;
using System.Threading.Tasks;
using MvvmCross.Logging;
using Octokit;
using PCController.Core.Models;

// ReSharper disable CheckNamespace
// ReSharper disable once ArrangeModifiersOrder

// using code from Copyright © 2019 Transeric Solutions. All rights reserved.
// Author: Eric David Lynch

namespace PCController.Console
{
    public class GitHubManager
    {
        private static readonly string GitHubIdentity = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
        private readonly static string _githubPassword = "Safron1234!!";
        private readonly static string _gitHubUser = "olaafrossi";
        private readonly static string _defaultRepo = "CrestronNetworkMonitor";
        //private readonly static IMvxLog _log;

        public GitHubManager(IMvxLogProvider logProvider)
        {
            //_log = logProvider.GetLogFor<GitHubManager>();
            //_log.Info("GitHubManager has been constructed");

            // setup fields from System settings to local variables

            //PCController.Core.Properties.Settings.Default.GitHubReleaseRepo
            //PCController.Core.Properties.Settings.Default.GitHubAccountOwner
            //PCController.Core.Properties.Settings.Default.GitHubAccountOwner

            var productInformation = new ProductHeaderValue(GitHubIdentity);

            if (!TryGetClient(productInformation, out GitHubClient client))
            {
                return;
            }

            GetClient(productInformation);
            GitHubClient = client;
        }

        public GitHubManager()
        {
            // empty ctor just for local testing TODO delete me :)

            var productInformation = new ProductHeaderValue(GitHubIdentity);

            if (!TryGetClient(productInformation, out GitHubClient client))
            {
                return;
            }

            GetClient(productInformation);
            GitHubClient = client;
        }

        private static GitHubClient GitHubClient { get; set; }

        public void GetLatestRelease()
        {
            GetGitHubReleaseAsync(_defaultRepo).GetAwaiter().GetResult();
        }

        public void GetRepo()
        {
            GetGitHubRepoInfo(GitHubClient).GetAwaiter().GetResult();
        }

        private static GitHubClient AuthenticateBasic(ProductHeaderValue productInformation)
        {
            //_log.Info("trying to get a GitHub client{productInformation}", productInformation);
            return GetClient(productInformation, _gitHubUser, _githubPassword);
        }

        private static GitHubClient AuthenticateToken(ProductHeaderValue productionInformation, string token)
        {

            return GetClient(productionInformation, token);
        }

        private static GitHubClient GetClient(ProductHeaderValue productInformation, string username, string password)
        {
            // the basic auth- not working
            var credentials = new Credentials(username, password, AuthenticationType.Basic);
            var client = new GitHubClient(productInformation) { Credentials = credentials };
            return client;
        }

        private static GitHubClient GetClient(ProductHeaderValue productInformation, string token)
        {
            // the Oauth- reccomended
            var credentials = new Credentials(token);
            var client = new GitHubClient(productInformation) { Credentials = credentials };
            return client;
        }

        private static GitHubClient GetClient(ProductHeaderValue productInformation)
        {
            //unauthenticated- only for private repo's
            var client = new GitHubClient(productInformation);
            return client;
        }

        private static async Task<GitHubReleaseModel> GetGitHubReleaseAsync(string repo)
        {
            try
            {
                //_log.Info("Getting the latest release from {repo} and mapping to our model", repo);
                var latestRelease = await GitHubClient.Repository.Release.GetLatest(_gitHubUser, _defaultRepo);
                var output = new GitHubReleaseModel
                {
                    LatestReleaseName = latestRelease.Name,
                    LatestReleaseNameTagName = latestRelease.TagName,
                    LatestReleaseBody = latestRelease.Body,
                    LatestReleaseAssets = latestRelease.Assets,
                    LatestReleaseAssetsUrl = latestRelease.AssetsUrl,
                    LatestReleaseAuthor = latestRelease.Author,
                    LatestReleaseCreatedAt = latestRelease.CreatedAt,
                    LatestReleaseIsDraft = latestRelease.Draft,
                    LatestReleaseHtmlUrl = latestRelease.HtmlUrl,
                    LatestReleaseId = latestRelease.Id,
                    LatestReleaseNodeId = latestRelease.NodeId,
                    LatestReleaseIsPreRelease = latestRelease.Prerelease,
                    LatestReleasePublishedAt = latestRelease.PublishedAt,
                    LatestReleaseZipBallUrl = latestRelease.ZipballUrl,
                    LatestReleaseUrl = latestRelease.Url
                };

                System.Console.WriteLine(output.LatestReleaseName);
                System.Console.WriteLine(output.LatestReleaseAuthor);
                System.Console.WriteLine(output.LatestReleaseAssetsUrl);
                return output;
            }
            catch (Exception e)
            {
                //_log.ErrorException("Getting the latest release from {repo} failed", e);
                return null;
            }
        }

        private static async Task<GitHubRepoModel> GetGitHubRepoInfo(GitHubClient client)
        {
            try
            {
                //_log.Info("Getting the latest info from {client} and mapping to our model", client);
                var repoInfo = await GitHubClient.Repository.Get(_gitHubUser, _defaultRepo);
                var output = new GitHubRepoModel
                {
                    RepoName = repoInfo.Name,
                    RepoGitUrl = repoInfo.GitUrl,
                    Description = repoInfo.Description,
                    HtmlUrl = repoInfo.HtmlUrl,
                    Homepage = repoInfo.Homepage,
                    HasPages = repoInfo.HasPages,
                    HasWiki = repoInfo.HasWiki
                };
                System.Console.WriteLine(output.RepoName);
                System.Console.WriteLine(output.RepoGitUrl);
                System.Console.WriteLine(output.Description);
                System.Console.WriteLine(output.HtmlUrl);
                System.Console.WriteLine(output.Homepage);
                System.Console.WriteLine(output.HasPages.ToString());
                System.Console.WriteLine(output.HasWiki.ToString());
                return output;

            }
            catch (Exception e)
            {
                //_log.ErrorException("Getting the repo info from {client} failed", e);
                return null;
            }
        }

        private static bool TryGetClient(ProductHeaderValue productInformation, out GitHubClient client)
        {
            client = AuthenticateBasic(productInformation);
            return client != null;
        }
    }
}