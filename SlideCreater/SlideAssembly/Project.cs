using SlideCreater.AssetManagment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace SlideCreater.SlideAssembly
{
    public class Project
    {
        public SlideLayout Layouts { get; set; } = new SlideLayout();
        public List<Slide> Slides { get; set; } = new List<Slide>();

        public List<ProjectAsset> Assets { get; set; } = new List<ProjectAsset>();
        public string SourceCode { get; set; } = string.Empty;


        private int slidenum = 0;
        public int NewSlideNumber => slidenum++;


        public void Save(string filename)
        {
            try
            {
                var sobj = JsonSerializer.Serialize<Project>(this);
                using (var sw = new StreamWriter(filename))
                {
                    sw.Write(sobj);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to save project");
            }
        }

        public static Project Load(string filename)
        {
            try
            {
                string contents;
                using (var sr = new StreamReader(filename))
                {
                    contents = sr.ReadToEnd();
                }
                return JsonSerializer.Deserialize<Project>(contents);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load project");
            }
            return new Project();
        }
    }
}
