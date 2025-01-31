﻿using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.Json;

using Xenon.LayoutEngine;
using Xenon.Renderer;

namespace Xenon.SlideAssembly
{

    public interface ISlideHashable
    {
        public int Hash();
    }

    public class Slide
    {

        public const string LAYOUT_INFO_KEY = "slide.layout.info";

        public Dictionary<string, Color> Colors { get; set; } = new Dictionary<string, Color>();

        public string Name { get; set; }
        public int Number { get; set; }
        public SlideFormat Format { get; set; }
        public MediaType MediaType { get; set; }
        public string Asset { get; set; }
        public List<SlideLine> Lines { get; set; } = new List<SlideLine>();
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> NonRenderedMetadata { get; set; } = new Dictionary<string, object>();
        public SlideOverridingBehaviour OverridingBehaviour { get; set; } = new SlideOverridingBehaviour();

        public Slide Clone()
        {
            return new Slide()
            {
                Colors = Colors,
                Name = Name,
                Number = Number,
                Format = Format,
                MediaType = MediaType,
                Asset = Asset,
                Lines = Lines,
                Data = Data,
                NonRenderedMetadata = NonRenderedMetadata,
                OverridingBehaviour = OverridingBehaviour,
            };
        }

        public int Hash()
        {
            // borrowing the implementation from: https://thomaslevesque.com/2020/05/15/things-every-csharp-developer-should-know-1-hash-codes/
            unchecked // Allow arithmetic overflow, numbers will just "wrap around"
            {
                int hashcode = 1430287;
                hashcode = hashcode * 7302013 ^ Name.GetHashCode();
                // DILEBERATLEY EXCLUDE NUMBER (the intention is to allow the slide's ordering to change, but the content of the slide needs to be hashed)
                //hashcode = hashcode * 7302013 ^ Number.GetHashCode();
                hashcode = hashcode * 7302013 ^ (int)Format;
                hashcode = hashcode * 7302013 ^ (int)MediaType;
                hashcode = hashcode * 7302013 ^ Asset.GetHashCode();
                hashcode = hashcode * 7302013 ^ OverridingBehaviour.Hash();

                // we don't really like Lines anymore.... but I'd better since I think some types may still use these
                /*
                foreach (var line in Lines)
                {
                    hashcode = hashcode * 7302013 ^ line.Hash();
                }
                */
                var lineshash = JsonSerializer.Serialize(Lines);
                hashcode = hashcode * 7302013 ^ lineshash.GetHashCode();

                // not sure how to handle data...
                /*
                foreach (var kvp in Data)
                {
                    hashcode = hashcode * 7302013 ^ kvp.GetHashCode();

                    var hobj = kvp.Value as ISlideHashable;
                    hashcode = hashcode * 7302013 ^ hobj?.Hash() ?? (kvp.Value?.GetHashCode() ?? 90103133);
                }
                */

                // maybe just use data as a string???
                // perhaps expensive....
                StringBuilder sb = new StringBuilder();
                //var datahash = JsonSerializer.Serialize(Data);
                sb.Append(JsonSerializer.Serialize(Data));

                // this isn't good enough to handle stitchedHymns. The boxed-assigned-ordered-images doesn't serialize correctly
                // ... rather than actually fix this, we'll just special case it
                if (Data.TryGetValue(StitchedImageRenderer.DATAKEY_BOX_ASSIGNED_IMAGES, out var boxAssignments))
                {
                    // how to hash this?
                    // just append it to the previous string?
                    foreach (var item in boxAssignments as List<(LSBImageResource rsx, int index)>)
                    {
                        sb.Append(item.rsx.Size.ToString());
                        sb.Append(item.rsx.AssetRef.ToString());
                        sb.Append(item.index);
                    }
                    
                }

                var datahash = sb.ToString();
                hashcode = hashcode * 7302013 ^ datahash.GetHashCode();

                return hashcode;
            }
        }

        public Slide()
        {
            Colors.Add("text", Color.White);
            Colors.Add("alttext", Color.Teal);
            Colors.Add("background", Color.Gray);
            Colors.Add("keybackground", Color.Black);
            Colors.Add("keytrans", Color.Gray);
        }

    }

    public class SlideOverridingBehaviour
    {
        public bool ForceOverrideExport { get; set; } = false;
        public string OverrideExportName { get; set; } = "";
        public string OverrideExportKeyName { get; set; } = "";

        public int Hash()
        {
            // borrowing the implementation from: https://thomaslevesque.com/2020/05/15/things-every-csharp-developer-should-know-1-hash-codes/
            unchecked // Allow arithmetic overflow, numbers will just "wrap around"
            {
                int hashcode = 1430287;
                hashcode = hashcode * 7302013 ^ ForceOverrideExport.GetHashCode();
                hashcode = hashcode * 7302013 ^ OverrideExportName.GetHashCode();
                hashcode = hashcode * 7302013 ^ OverrideExportKeyName.GetHashCode();
                return hashcode;
            }

        }

    }

}
