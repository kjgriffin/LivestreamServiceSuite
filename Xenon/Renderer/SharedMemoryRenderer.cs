using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    public class SharedMemoryRenderer
    {

        public static List<MemoryMappedFile> ExportSlides(string sharedLocation, List<RenderedSlide> slides)
        {
            //MemoryMappedFile mstream = MemoryMappedFile.CreateOrOpen()

            // compute approx memory needs...

            //int memEstimate = slides.Count(s => s.MediaType == MediaType.Image) * 

            List<MemoryMappedFile> result = new List<MemoryMappedFile>();

            int id = 0;
            foreach (var rs in slides)
            {
                if (rs.MediaType == MediaType.Image)
                {
                    var mfile = MemoryMappedFile.CreateNew($"SC-hotreload-{id++}", 1920 * 1080 * 4 + 10 * 1000);
                    result.Add(mfile);

                    using (var stream = mfile.CreateViewStream())
                    {
                        // this will probably be slow and use oodles of memory
                        rs.Bitmap.SaveAsBmp(stream);
                    }

                    if (rs.MediaType == MediaType.Text)
                    {

                    }

                }

            }

            return result;

        }
    }
}
