// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 13
// by Olaaf Rossi

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using OSVersionHelper;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;

namespace PCController.Core.Managers
{
    public class AppInfoManager
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Uri GetAppInstallerInfoUri(Package p)
        {
            AppInstallerInfo aiInfo = p.GetAppInstallerInfo();
            if (aiInfo != null)
            {
                return aiInfo.Uri;
            }

            return null;
        }

        public string GetAppInstallerUri()
        {
            string result;

            if (!WindowsVersionHelper.HasPackageIdentity)
            {
                return "Not packaged";
            }

            if (ApiInformation.IsMethodPresent("Windows.ApplicationModel.Package", "GetAppInstallerInfo"))
            {
                Uri aiUri = GetAppInstallerInfoUri(Package.Current);
                if (aiUri != null)
                {
                    result = $"MSIX Installer URL: {aiUri}";
                }
                else
                {
                    result = "not present";
                }
            }
            else
            {
                result = "Not Available";
            }

            return result;
        }

        public string GetAssemblyFileVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string output = fileVersionInfo.FileVersion;
            
            return $"{output}";
        }

        public string GetAssemblyInformationVersion()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string output = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                
                return $"{output}";
            }
            catch (Exception e)
            {
                return e.ToString();
                // throw;
            }
        }

        public string GetAssemblyVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version? assemblyVersion = assembly.GetName().Version;
            string output = assemblyVersion.ToString();

            return $"{output}";
        }

        public string GetDisplayName()
        {
            if (WindowsVersionHelper.HasPackageIdentity)
            {
                return $"{Package.Current.DisplayName}";
            }

            return "No Display Name, Not MSIX packaged";
        }

        public string GetDotNetInfo()
        {
            FileInfo runTimeDir = new FileInfo(typeof(string).Assembly.Location);
            FileInfo entryDir = new FileInfo(Assembly.GetEntryAssembly().Location);
            bool selfContained = runTimeDir.DirectoryName == entryDir.DirectoryName;

            string result = ".NET Framework Install Type - ";
            if (selfContained)
            {
                result += "Self Contained Deployment";
            }
            else
            {
                result += "Framework Dependent Deployment";
            }

            return result;
        }

        public string GetInstallLocation()
        {
            return $"{Assembly.GetExecutingAssembly().Location}";
        }

        public Version GetMsixPackageVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string manifestPath = assembly.Location.Replace(assembly.ManifestModule.Name, string.Empty) + @"..\AppxManifest.xml";
            if (File.Exists(manifestPath))
            {
                XDocument xDoc = XDocument.Load(manifestPath);
                return new Version(xDoc.Descendants().First(e => e.Name.LocalName == "Identity").Attributes().First(a => a.Name.LocalName == "Version").Value);
            }

            return new Version(0, 0, 0, 0);
        }

        public string GetPackageChannel()
        {
            if (WindowsVersionHelper.HasPackageIdentity)
            {
                return $"{Package.Current.Id.Name.Substring(Package.Current.Id.Name.LastIndexOf('.') + 1)}";
            }

            return "No Package Channel";
        }

        public string GetPackageVersion()
        {
            if (WindowsVersionHelper.HasPackageIdentity)
            {
                return
                    $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";
            }

            return "Not MSIX packaged";
        }
    }
}