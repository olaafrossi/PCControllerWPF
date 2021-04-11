﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCController.Console
{
    public class ReleaseFileManager
    {
        //_log
        private bool _success;
        private readonly static string _appPath = PCController.Core.Properties.Settings.Default.MonitoredAppPath;
        private readonly StringCollection _preserveList = PCController.Core.Properties.Settings.Default.MonitoredAppPreserveList;
        private readonly string _zippedRelease;

        public ReleaseFileManager()
        {
            //_log
            GitHubManager gitManager = new GitHubManager();
            System.Console.WriteLine(gitManager.HasDownLoadedLatestRelease.ToString());
            gitManager.GetRelease();
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine(gitManager.HasDownLoadedLatestRelease.ToString());
            System.Console.WriteLine();
            System.Console.WriteLine();

            System.Console.WriteLine(gitManager.DownloadedLatestReleaseFileAttributes);
            _zippedRelease = gitManager.DownloadedLatestReleaseFilePath;
            ExtractArchive(_zippedRelease);
        }

        public bool CleanTempPath(string path)
        {
            bool output = false;

            return output;
        }

        public bool BackupExisting(string path)
        {
            bool output = false;

            return output;
        }

        public bool ExtractArchive(string zippedFile)
        {
            _success = true;

            try
            {
                using var archive = ZipFile.OpenRead(_zippedRelease);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    var fullPath = Path.Combine(_appPath, entry.FullName);

                    if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                    {
                        //Log.Information($"Creating Directory {fullPath}");
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    }

                    bool shouldPreserve = false;

                    foreach (string s in _preserveList)
                    {
                        if (string.IsNullOrEmpty(s) == false && entry.FullName.StartsWith(s) && File.Exists(fullPath))
                        {
                            shouldPreserve = true;
                            break;
                        }
                    }

                    foreach (string s in _preserveList)
                    {
                        if (string.IsNullOrEmpty(s) == false && entry.FullName.StartsWith(s) && Directory.Exists(fullPath))
                        {
                            shouldPreserve = true;
                            break;
                        }
                    }

                    if (shouldPreserve)
                    {
                        // Log.Information($"Preserving file: {entry.FullName}");
                        System.Console.WriteLine($"Preserving file: {entry.FullName}");
                    }



                    else
                    {
                        if (fullPath.EndsWith(@"/") == false && fullPath.EndsWith(@"\") == false)
                        {
                            entry.ExtractToFile(fullPath, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _success = false;
               // Log.Error(e.ToString());
            }

            return _success;
        }
    }
}
