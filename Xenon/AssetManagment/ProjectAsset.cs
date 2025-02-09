﻿using System;
using System.IO;

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

        public string KindString
        {
            get
            {
                switch (Type)
                {
                    case AssetType.Image:
                        return "image";
                    case AssetType.Video:
                        return "video";
                    case AssetType.Audio:
                        return "audio";
                    default:
                        return "";
                }
            }
        }

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
