using System;
using System.Collections.Generic;
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
        private string _zippedRelease;
        private readonly string _appPath;
        private readonly List<string> _preserveList;

        public ReleaseFileManager()
        {
            //_log
        }

        public bool ExtractArchive(string zippedFile)
        {
            _success = true;

            try
            {
                using var archive = ZipFile.OpenRead(_zippedRelease);
                foreach (var entry in archive.Entries)
                {
                    var fullPath = Path.Combine(_appPath, entry.FullName);

                    if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                    {
                        //Log.Information($"Creating Directory {fullPath}");
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    }

                    bool shouldPreserve = false;

                    foreach (var x in _preserveList)
                    {
                        if (string.IsNullOrEmpty(x) == false && entry.FullName.StartsWith(x) && File.Exists(fullPath))
                        {
                            shouldPreserve = true;
                            break;
                        }
                    }

                    if (shouldPreserve)
                    {
                       // Log.Information($"Preserving file: {entry.FullName}");
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
