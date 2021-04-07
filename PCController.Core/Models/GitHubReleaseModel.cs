// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 04 04
// by Olaaf Rossi

using System;
using System.Collections.Generic;
using Octokit;

namespace PCController.Core.Models
{
    public class GitHubReleaseModel
    {
        public string LatestReleaseName { get; set; }
        public string LatestReleaseNameTagName { get; set; }
        public string LatestReleaseBody { get; set; }
        public IReadOnlyList<ReleaseAsset> LatestReleaseAssets { get; set; }
        public string LatestReleaseAssetsUrl { get; set; }
        public Author LatestReleaseAuthor { get; set; }
        public DateTimeOffset LatestReleaseCreatedAt { get; set; }
        public bool LatestReleaseIsDraft { get; set; }
        public string LatestReleaseHtmlUrl { get; set; }
        public int LatestReleaseId { get; set; }
        public string LatestReleaseNodeId { get; set; }
        public bool LatestReleaseIsPreRelease { get; set; }
        public DateTimeOffset? LatestReleasePublishedAt { get; set; }
        public string LatestReleaseZipBallUrl { get; set; }
        public string LatestReleaseUrl { get; set; }
    }
}