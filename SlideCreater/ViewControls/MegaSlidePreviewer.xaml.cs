using System.Windows;
using System.Windows.Controls;

using Xenon.Renderer;

namespace SlideCreater.ViewControls
{
    /// <summary>
    /// Interaction logic for MegaSlidePreviewer.xaml
    /// </summary>
    public partial class MegaSlidePreviewer : UserControl
    {
        public MegaSlidePreviewer()
        {
            InitializeComponent();
        }

        private RenderedSlide _slide;
        public RenderedSlide Slide
        {
            get => Slide;
            set
            {
                _slide = value;
                Dispatcher.Invoke(() =>
                {
                    main.Slide = value;
                    main.ShowSlide(false);
                    key.Slide = value;
                    key.ShowSlide(true);


                    bHasPostsetTag.Visibility = value.IsPostset ? Visibility.Visible : Visibility.Hidden;
                    postset.Content = value.IsPostset ? value.Postset.ToString() : "none";

                    number.Content = value.Number;
                    string tmp = string.Join(", ", Xenon.Renderer.SlideExporter.WillCreate(value));
                    creates.Text = tmp;
                    if (value.MediaType == Xenon.SlideAssembly.MediaType.Text)
                    {
                        tbScriptText.Text = value.Text;
                        bHasScriptTag.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        tbScriptText.Text = "";
                        bHasScriptTag.Visibility = Visibility.Collapsed;
                    }
                    if (value.HasPilot)
                    {
                        bHasPilotTag.Visibility = Visibility.Visible;
                        tbPilotText.Text = value.Pilot;
                    }
                    else
                    {
                        bHasPilotTag.Visibility = Visibility.Collapsed;
                        tbPilotText.Text = string.Empty;
                    }
                    bIsResourceTag.Visibility = value.Number == -1 ? Visibility.Visible : Visibility.Collapsed;
                });
            }
        }
    }
}
