using Microsoft.Office.Interop.PowerPoint;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LSBgenerator
{
    public class PowerPointRenderer
    {


        public void RenderStillsToPowerpoint(string filename, List<string> stills)
        {
            Microsoft.Office.Interop.PowerPoint.Application _powerpoint = new Microsoft.Office.Interop.PowerPoint.Application();

            Presentation presentation = _powerpoint.Presentations.Add(Microsoft.Office.Core.MsoTriState.msoTrue);

            Slides slides = presentation.Slides;
            int slidenum = 1;
            stills = stills.OrderBy(s => int.Parse(System.IO.Path.GetFileNameWithoutExtension(s).Split('_')[1])).ToList();
            foreach (var still in stills)
            {
                _Slide slide = slides.Add(slidenum++, PpSlideLayout.ppLayoutBlank);
                // add image
                slide.Shapes.AddPicture(still, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoTrue, 0, 0);
            }

            presentation.SaveAs(filename, Microsoft.Office.Interop.PowerPoint.PpSaveAsFileType.ppSaveAsDefault, Microsoft.Office.Core.MsoTriState.msoTrue);

            presentation.Close();
            _powerpoint.Quit();

        }

    }
}
