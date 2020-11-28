using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Integrated_Presenter
{
    public class Presentation
    {

        public string Folder { get; set; }

        public List<Slide> Slides { get; set; } = new List<Slide>();

        public static Slide EmptySlide = new Slide() { Source = "", Type = SlideType.Empty };

        public bool Create(string folder)
        {
            Slides.Clear();
            Folder = folder;

            // get all files in directory and hope they are slides
            var files = Directory.GetFiles(Folder).OrderBy(p => Convert.ToInt32(Regex.Match(Path.GetFileName(p), "(?<slidenum>\\d+).*").Groups["slidenum"].Value)).ToList();
            foreach (var file in files)
            {
                var filename = Regex.Match(Path.GetFileName(file), "\\d+_(?<type>[^-]*)-?(?<action>.*)\\..*");
                string name = filename.Groups["type"].Value;
                string action = filename.Groups["action"].Value;
                SlideType type;
                // look at the name to determine the type
                switch (name)
                {
                    case "Full":
                        type = SlideType.Full;
                        break;
                    case "Liturgy":
                        type = SlideType.Liturgy;
                        break;
                    case "Video":
                        type = SlideType.Video;
                        break;
                    default:
                        type = SlideType.Empty;
                        break;
                }
                Slide s = new Slide() { Source = file, Type = type, Action = action };
                Slides.Add(s);
            }

            return false;
        }


        public int SlideCount { get => Slides.Count; }

        public int CurrentSlide { get => _currentSlide + 1; }

        private int _currentSlide = 0;

        public Slide Prev
        {
            get
            {
                if (_currentSlide - 1 >= 0)
                {
                    return Slides[_currentSlide - 1];
                }
                else
                {
                    return EmptySlide;
                }
            }
        }


        public Slide Override { get; set; }
        public bool OverridePres { get; set; } = false;

        public Slide EffectiveCurrent
        {
            get
            {
                if (OverridePres && Override != null)
                {
                    return Override;
                }
                return Current;
            }
        }

        public Slide Current { get => Slides[_currentSlide]; }
        public Slide Next
        {
            get
            {
                if (_currentSlide + 1 < SlideCount)
                {
                    return Slides[_currentSlide + 1];
                }
                else
                {
                    return EmptySlide;
                }
            }
        }
        public Slide After
        {
            get
            {
                if (_currentSlide + 2 < SlideCount)
                {
                    return Slides[_currentSlide + 2];
                }
                else
                {
                    return EmptySlide;
                }
            }
        }

        public void NextSlide()
        {
            if (_currentSlide + 1 < SlideCount)
            {
                _currentSlide += 1;
            }
        }

        public void PrevSlide()
        {
            if (_currentSlide - 1 >= 0)
            {
                _currentSlide -= 1;
            }
        }

        public void StartPres()
        {
            _currentSlide = 0;
        }

    }

    public class Slide
    {
        public SlideType Type { get; set; }
        public string Source { get; set; }
        public string Action { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
    }

    public enum SlideType
    {
        Full,
        Liturgy,
        Video,
        Empty
    }

}
