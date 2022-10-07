using IntegratedPresenterAPIInterop;

using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xaml;

using Xenon.Compiler;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    public class SharedMemoryRenderer
    {

        public class SharedMemoryPresentation
        {
            public MirrorPresentationDescription Info { get; set; }
            public List<MemoryMappedFile> MFiles { get; set; }
        }

        public static SharedMemoryPresentation ExportSlides(Project proj, List<RenderedSlide> slides)
        {
            MirrorPresentationDescription presDescription = new MirrorPresentationDescription();
            List<MemoryMappedFile> mfiles = new List<MemoryMappedFile>();

            foreach (var rs in slides.Where(x => x.Number >= 0).OrderBy(x => x.Number))
            {
                MirrorSlide sinfo = new MirrorSlide();
                sinfo.Num = rs.Number;
                if (rs.MediaType == MediaType.Image)
                {
                    string fname = $"SC-Pres-{rs.Number}_{rs.RenderedAs}-png";
                    string kname = $"SC-Pres-Key_{rs.Number}-png";
                    sinfo.PrimaryResource = fname;
                    sinfo.KeyResource = kname;
                    sinfo.HasKey = true;
                    // type here is liturgy/full/ override type
                    var nmatch = Regex.Match(rs.RenderedAs, "(?<type>\\w+)(-(?<action>\\w+))?");
                    string type = nmatch.Groups["type"].Value;

                    if (nmatch.Groups["action"].Success)
                    {
                        sinfo.PreAction = nmatch.Groups["action"].Value;
                    }

                    if (rs.OverridingBehaviour?.ForceOverrideExport == true)
                    {
                        type = rs.OverridingBehaviour.OverrideExportName;
                    }
                    if (type == "Liturgy")
                    {
                        sinfo.SlideType = SlideType.Liturgy;
                    }
                    else if (type == "Full")
                    {
                        sinfo.SlideType = SlideType.Full;
                    }
                    else
                    {
                        sinfo.SlideType = SlideType.Full; // perhaps?
                    }

                    var mfile = MemoryMappedFile.CreateNew(fname, rs.BitmapPNGMS.Length);
                    mfiles.Add(mfile);
                    using (var stream = mfile.CreateViewStream())
                    {
                        // this will probably be slow and use oodles of memory
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Write(rs.BitmapPNGMS.GetBuffer(), 0, (int)rs.BitmapPNGMS.Length);
                    }

                    var kfile = MemoryMappedFile.CreateNew(kname, rs.KeyPNGMS.Length);
                    mfiles.Add(kfile);
                    using (var stream = kfile.CreateViewStream())
                    {
                        // this will probably be slow and use oodles of memory
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Write(rs.KeyPNGMS.GetBuffer(), 0, (int)rs.KeyPNGMS.Length);
                    }
                }
                else if (rs.MediaType == MediaType.Text)
                {
                    string aname = $"SC-Pres-{rs.Number}_{rs.RenderedAs}-txt";
                    sinfo.ActionInfo = aname;
                    sinfo.SlideType = SlideType.Action;

                    // urrrg... need to peek it here and pull out the resources
                    // orrrrrrrrr when we use the resource file just attach it then...?

                    // for action files that have referenced real slides then we've got work to do here
                    if (ActionLoader.TryLoadActions(rs.Text, "", out var lres, checkRealMedia: false))
                    {
                        if (lres.AltSources)
                        {

                            var match = Regex.Match(lres.AltSource, "^(?<num>\\d+)_Liturgy\\.png$");
                            if (match.Success)
                            {
                                sinfo.HasOverridePrimary = true;
                                sinfo.PrimaryResource = $"SC-Pres-{match.Groups["num"].Value}_Liturgy-png";
                            }
                            match = Regex.Match(lres.AltSource, "^Key_(?<num>\\d+)\\.png$");
                            if (match.Success)
                            {
                                sinfo.HasOverridePrimary = true;
                                sinfo.KeyResource = $"SC-Pres-Key_{match.Groups["num"].Value}-png";
                            }
                        }
                    }

                    string ctext = rs.Text;
                    // replace audio file references to proj temp folder since they aren't rendered out to memeory
                    var fmatch = Regex.Match(ctext, @"arg1:LoadAudioFile\((?<file>.*)\)");
                    while (fmatch.Success)
                    {
                        var fname = proj.Assets.FirstOrDefault(x => $"Resource_{x.Name}{x.Extension}" == fmatch.Groups["file"].Value);
                        var file = fname?.CurrentPath ?? "";
                        ctext = Regex.Replace(ctext, @"arg1:LoadAudioFile\(.*\)", $"arg1:LoadAudioFile({file})");

                        fmatch = Regex.Match(ctext, @"arg1:LoadAudio\((?<file>.*)\)");
                    }

                    var afile = MemoryMappedFile.CreateNew(aname, ctext.Length * 8 + 1024);
                    mfiles.Add(afile);
                    using (var stream = afile.CreateViewStream())
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(ctext);
                    }
                }
                else if (rs.MediaType == MediaType.Video)
                {
                    string fname = rs.AssetPath;
                    string kname = $"SC-Pres-Key_{rs.Number}-png";
                    sinfo.PrimaryResource = fname;
                    sinfo.KeyResource = kname;
                    sinfo.HasKey = true;
                    // type here is liturgy/full/ override type
                    var nmatch = Regex.Match(rs.RenderedAs, "(?<type>\\w+)(-(?<action>\\w+))?");
                    string type = nmatch.Groups["type"].Value;

                    if (nmatch.Groups["action"].Success)
                    {
                        sinfo.PreAction = nmatch.Groups["action"].Value;
                    }

                    if (rs.OverridingBehaviour?.ForceOverrideExport == true)
                    {
                        type = rs.OverridingBehaviour.OverrideExportName;
                    }
                    else
                    {
                        sinfo.SlideType = SlideType.Video;
                    }

                    var kfile = MemoryMappedFile.CreateNew(kname, rs.KeyPNGMS.Length);
                    mfiles.Add(kfile);
                    using (var stream = kfile.CreateViewStream())
                    {
                        // this will probably be slow and use oodles of memory
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Write(rs.KeyPNGMS.GetBuffer(), 0, (int)rs.KeyPNGMS.Length);
                    }
                }


                if (rs.IsPostset)
                {
                    sinfo.HasPostset = true;
                    sinfo.PostsetInfo = rs.Postset;
                }

                if (rs.HasPilot)
                {
                    string pname = $"SC-Pres-Pilot_{rs.Number}-txt";
                    var pfile = MemoryMappedFile.CreateNew(pname, rs.Pilot.Length * 8 + 1024);
                    mfiles.Add(pfile);
                    using (var stream = pfile.CreateViewStream())
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(rs.Pilot);
                    }

                    sinfo.HasPilot = true;
                    sinfo.PilotInfo = pname;

                }

                // drive?
                sinfo.AutomationEnabled = true;


                presDescription.Slides.Add(sinfo);
            }

            foreach (var rs in slides.Where(x => x.Number == -1))
            {
                // create a ref for it and attach to slides as needed
                if (rs.MediaType == MediaType.Image && rs.OverridingBehaviour?.ForceOverrideExport == true)
                {
                    string sname = $"SC-Res-{rs.OverridingBehaviour.OverrideExportName}-png";
                    string kname = $"SC-Res-{rs.OverridingBehaviour.OverrideExportKeyName}-png";

                    var mnum = Regex.Match(sname, "Resource_\\d+_forslide_(?<place>\\d+)").Groups["place"].Value;
                    var snum = int.Parse(mnum);

                    var sfile = MemoryMappedFile.CreateNew(sname, rs.BitmapPNGMS.Length);
                    mfiles.Add(sfile);
                    using (var stream = sfile.CreateViewStream())
                    {
                        // this will probably be slow and use oodles of memory
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Write(rs.BitmapPNGMS.GetBuffer(), 0, (int)rs.BitmapPNGMS.Length);
                    }
                    var kfile = MemoryMappedFile.CreateNew(kname, rs.KeyPNGMS.Length);
                    mfiles.Add(kfile);
                    using (var stream = kfile.CreateViewStream())
                    {
                        // this will probably be slow and use oodles of memory
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Write(rs.KeyPNGMS.GetBuffer(), 0, (int)rs.KeyPNGMS.Length);
                    }
                    if (presDescription.Slides.Count > snum)
                    {
                        presDescription.Slides[snum].HasOverridePrimary = true;
                        presDescription.Slides[snum].PrimaryResource = sname;
                        presDescription.Slides[snum].HasOverrideKey = true;
                        presDescription.Slides[snum].KeyResource = kname;
                    }

                }
            }

            presDescription.HeavyResourcePath = proj.LoadTmpPath;

            var dtext = JsonSerializer.Serialize(presDescription);

            var dfile = MemoryMappedFile.CreateNew(CommonAPINames.HotReloadPresentationDescriptionFile, dtext.Length * 8 + 1024);
            mfiles.Add(dfile);
            using (var stream = dfile.CreateViewStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(dtext);
            }

            return new SharedMemoryPresentation
            {
                Info = presDescription,
                MFiles = mfiles,
            };
        }
    }
}
