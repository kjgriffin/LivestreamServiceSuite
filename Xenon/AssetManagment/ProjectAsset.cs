using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.RightsManagement;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Xenon.AssetManagment
{
    public class ProjectAsset
    {
        public string CurrentPath
        {
            get
            {
                if (LoadedTempPath != null)
                {
                    return LoadedTempPath;
                }
                return OriginalPath;
            }
        }
        public string OriginalPath { get; set; }
        public string LoadedTempPath { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Group { get; set; } = "default";
        public string OriginalFilename { get => Path.GetFileName(CurrentPath); }
        public AssetType Type { get; set; }

        public string Extension { get; set; }

        public string InternalDisplayName { get; set; }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(InternalDisplayName))
                {
                    return InternalDisplayName;
                }
                return Name;
            }
        }

        public void UpdateTmpLocation(string newtempdirectory)
        {
            LoadedTempPath = Path.Combine(newtempdirectory, OriginalFilename);
        }

    }
}
