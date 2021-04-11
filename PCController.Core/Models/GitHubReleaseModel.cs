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
        public string ReleaseName { get; set; }
        public string ReleaseNameTagName { get; set; }
        public string ReleaseBody { get; set; }
        public IReadOnlyList<ReleaseAsset> ReleaseAssets { get; set; }
        public string ReleaseAssetsUrl { get; set; }
        public Author ReleaseAuthor { get; set; }
        public DateTimeOffset ReleaseCreatedAt { get; set; }
        public bool ReleaseIsDraft { get; set; }
        public string ReleaseHtmlUrl { get; set; }
        public int ReleaseId { get; set; }
        public string ReleaseNodeId { get; set; }
        public bool ReleaseIsPreRelease { get; set; }
        public DateTimeOffset? ReleasePublishedAt { get; set; }
        public string ReleaseZipBallUrl { get; set; }
        public string ReleaseUrl { get; set; }
        public string ReleaseAssetDownloadUrl { get; set; }
        public int ReleaseAssetDownloadId { get; set; }
    }
}