// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 04 07
// by Olaaf Rossi

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MvvmCross.Logging;
using Octokit;
using PCController.Core.Models;
using PCController.Core.Properties;

// ReSharper disable CheckNamespace
// ReSharper disable once ArrangeModifiersOrder

// using code from Copyright © 2019 Transeric Solutions. All rights reserved.
// Author: Eric David Lynch

namespace PCController.Console
{
    public class GitHubManager
    {
        private static readonly string GitHubIdentity = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;

        //private readonly static string _githubPassword; //TODO create an instance of the Azure class to ge the secret
        private readonly static string _gitHubUser = "olaafrossi";
        private readonly static string _defaultRepo = "CrestronNetworkMonitor";
        private static string _monitoredAppPath;
        private static string _monitoredAppBackupPath;
        private static string _monitoredAppTempPath;

        //private readonly static IMvxLog _log;

        public GitHubManager(IMvxLogProvider logProvider)
        {
            //_log = logProvider.GetLogFor<GitHubManager>();
            //_log.Info("GitHubManager has been constructed");

            // setup fields from System settings to local variables

            //PCController.Core.Properties.Settings.Default.GitHubReleaseRepo
            //PCController.Core.Properties.Settings.Default.GitHubAccountOwner
            //PCController.Core.Properties.Settings.Default.GitHubAccountOwner
            //PCController.Core.Properties.Settings.Default.MonitoredAppPath
            //PCController.Core.Properties.Settings.Default.MonitoredAppBackupPath
            //PCController.Core.Properties.Settings.Default.MonitoredAppTempPath

            _monitoredAppPath = Settings.Default.MonitoredAppPath;
            _monitoredAppBackupPath = Settings.Default.MonitoredAppBackupPath;
            _monitoredAppTempPath = Settings.Default.MonitoredAppTempPath;

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

            _monitoredAppPath = Settings.Default.MonitoredAppPath;
            _monitoredAppBackupPath = Settings.Default.MonitoredAppBackupPath;
            _monitoredAppTempPath = Settings.Default.MonitoredAppTempPath;

            var productInformation = new ProductHeaderValue(GitHubIdentity);

            if (!TryGetClient(productInformation, out GitHubClient client))
            {
                return;
            }

            GetClient(productInformation);
            GitHubClient = client;
        }

        private static GitHubClient GitHubClient { get; set; }

        public bool HasDownLoadedLatestRelease { get; set; }

        public string DownloadedLatestReleaseFileAttributes { get; set; }

        public string DownloadedLatestReleaseFilePath { get; set; }

        public void GetLatestRelease()
        {
            GetGitHubReleaseAsync(_defaultRepo).GetAwaiter().GetResult();
        }

        public void GetRelease()
        {
            DownloadLatestGithubReleaseAsync().GetAwaiter().GetResult();
        }

        public void GetRepo()
        {
            GetGitHubRepoInfo(GitHubClient).GetAwaiter().GetResult();
        }

        private static GitHubClient AuthenticateBasic(ProductHeaderValue productInformation)
        {
            //_log.Info("trying to get a GitHub client{productInformation}", productInformation);
            System.Console.WriteLine(AzureKeyManager.GetPassword());
            return GetClient(productInformation, _gitHubUser, AzureKeyManager.GetPassword());
        }

        private static GitHubClient AuthenticateToken(ProductHeaderValue productionInformation, string token)
        {
            //_log.Info("trying to get a GitHub client {productInformation} with token {token}", productInformation, token);
            return GetClient(productionInformation, token);
        }

        private async Task<string> DownloadLatestGithubReleaseAsync()
        {
            try
            {
                var latestGitHubRelease = await GetGitHubReleaseAsync(_defaultRepo);

                string assetFilePath = $"{_monitoredAppTempPath}";
                string assetFilePathName = $"{assetFilePath}{latestGitHubRelease.ReleaseName}.zip";

                System.Console.WriteLine(assetFilePathName);

                var singleRelease = await GitHubClient.Repository.Release.GetAsset(_gitHubUser, _defaultRepo, latestGitHubRelease.ReleaseAssetDownloadId);
                var response = await GitHubClient.Connection.Get<object>(new Uri(singleRelease.Url), new Dictionary<string, string>(), "application/octet-stream");

                byte[] bytes = (byte[]) response.HttpResponse.Body;

                try
                {
                    File.WriteAllBytes(assetFilePathName, bytes);
                    bool fileExists = File.Exists(assetFilePathName);

                    try
                    {
                        System.Console.WriteLine(File.GetAttributes(assetFilePathName));
                        DownloadedLatestReleaseFileAttributes = File.GetAttributes(assetFilePathName).ToString();
                        System.Console.WriteLine(DownloadedLatestReleaseFilePath);
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine(e);
                    }

                    if (fileExists is true)
                    {
                        HasDownLoadedLatestRelease = true;
                        DownloadedLatestReleaseFilePath = assetFilePathName;
                        // TODO create event handler to set the prop so the releaseFile manager can take over
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e);
                    //_log
                }

                //ExtractBuild(assetFilePathName, path, persisPath);

                //update the current build number
                return latestGitHubRelease.ReleaseId.ToString();
            }
            catch (Exception e)
            {
                //_log.ErrorException("Getting the repo info from {client} failed", e);
                //throw ex;
                return null;
            }
        }

        private static GitHubClient GetClient(ProductHeaderValue productInformation, string username, string password)
        {
            // the basic auth- not working
            var credentials = new Credentials(username, password, AuthenticationType.Basic);
            var client = new GitHubClient(productInformation) {Credentials = credentials};
            return client;
        }

        private static GitHubClient GetClient(ProductHeaderValue productInformation, string token)
        {
            // the Oauth- reccomended
            var credentials = new Credentials(token);
            var client = new GitHubClient(productInformation) {Credentials = credentials};
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
                Release latestRelease = await GitHubClient.Repository.Release.GetLatest(_gitHubUser, _defaultRepo);
                GitHubReleaseModel output = new() {
                    ReleaseName = latestRelease.Name,
                    ReleaseNameTagName = latestRelease.TagName,
                    ReleaseBody = latestRelease.Body,
                    ReleaseAssets = latestRelease.Assets,
                    ReleaseAssetsUrl = latestRelease.AssetsUrl,
                    ReleaseAuthor = latestRelease.Author,
                    ReleaseCreatedAt = latestRelease.CreatedAt,
                    ReleaseIsDraft = latestRelease.Draft,
                    ReleaseHtmlUrl = latestRelease.HtmlUrl,
                    ReleaseId = latestRelease.Id,
                    ReleaseNodeId = latestRelease.NodeId,
                    ReleaseIsPreRelease = latestRelease.Prerelease,
                    ReleasePublishedAt = latestRelease.PublishedAt,
                    ReleaseZipBallUrl = latestRelease.ZipballUrl,
                    ReleaseUrl = latestRelease.Url,
                    ReleaseAssetDownloadUrl = latestRelease.Assets[0].BrowserDownloadUrl,
                    ReleaseAssetDownloadId = latestRelease.Assets[0].Id
                };

                System.Console.WriteLine(output.ReleaseName);
                System.Console.WriteLine(output.ReleaseAuthor);
                System.Console.WriteLine(output.ReleaseAssetsUrl);
                System.Console.WriteLine(latestRelease.Assets[0].BrowserDownloadUrl);
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
                var output = new GitHubRepoModel {
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