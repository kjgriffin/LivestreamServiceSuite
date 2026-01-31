using DeepSixGUI.Templates;

using Microsoft.Win32;

using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeepSixGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DeepSixWizard : Window
    {
        public DeepSixWizard()
        {
            InitializeComponent();
        }

        GravePlot _grave;
        public void ApplyCFGObj(GravePlot plot)
        {
            _grave = plot;

            tbServiceType.Text = _grave.ServiceName;
            tbServiceDate.SelectedDate = _grave.ServiceDate;
            tbServiceTime.Text = _grave.ServiceTime;
            tbDeceasedName.Text = _grave.DeceasedName;
            tbBirthDeath.Text = _grave.Lifespan;

            cbHymn1.IsChecked = _grave.Hymns[0].Use;
            tbHymn1.Text = _grave.Hymns[0].Number;
            cbHymn2.IsChecked = _grave.Hymns[1].Use;
            tbHymn2.Text = _grave.Hymns[1].Number;
            cbHymn3.IsChecked = _grave.Hymns[2].Use;
            tbHymn3.Text = _grave.Hymns[2].Number;
            cbHymn4.IsChecked = _grave.Hymns[3].Use;
            tbHymn4.Text = _grave.Hymns[3].Number;

            tbServiceFile.Text = _grave.ServicePath;

            cbReading1.IsChecked = _grave.Readings[0].Use;
            tbReading1.Text = _grave.Readings[0].Reference;
            cbReading2.IsChecked = _grave.Readings[1].Use;
            tbReading2.Text = _grave.Readings[1].Reference;
            cbReading3.IsChecked = _grave.Readings[2].Use;
            tbReading3.Text = _grave.Readings[2].Reference;
            cbReading4.IsChecked = _grave.Readings[3].Use;
            tbReading4.Text = _grave.Readings[3].Reference;

            rbNIV.IsChecked = _grave.Translation == "NIV";
            rbESV.IsChecked = _grave.Translation == "ESV";
        }

        internal void Create()
        {
            _grave.ServiceName = tbServiceType.Text;
            _grave.ServiceDate = tbServiceDate.SelectedDate ?? DateTime.Now;
            _grave.ServiceTime = tbServiceTime.Text;
            _grave.DeceasedName = tbDeceasedName.Text;
            _grave.Lifespan = tbBirthDeath.Text;

            _grave.Hymns[0].Use = cbHymn1.IsChecked ?? false;
            _grave.Hymns[1].Use = cbHymn2.IsChecked ?? false;
            _grave.Hymns[2].Use = cbHymn3.IsChecked ?? false;
            _grave.Hymns[3].Use = cbHymn4.IsChecked ?? false;
            _grave.Hymns[0].Number = tbHymn1.Text;
            _grave.Hymns[1].Number = tbHymn2.Text;
            _grave.Hymns[2].Number = tbHymn3.Text;
            _grave.Hymns[3].Number = tbHymn4.Text;

            _grave.Readings[0].Use = cbReading1.IsChecked ?? false;
            _grave.Readings[1].Use = cbReading2.IsChecked ?? false;
            _grave.Readings[2].Use = cbReading3.IsChecked ?? false;
            _grave.Readings[3].Use = cbReading4.IsChecked ?? false;
            _grave.Readings[0].Reference = tbReading1.Text;
            _grave.Readings[1].Reference = tbReading2.Text;
            _grave.Readings[2].Reference = tbReading3.Text;
            _grave.Readings[3].Reference = tbReading4.Text;

            _grave.Translation = rbNIV.IsChecked.Value ? "NIV" : "ESV";

            // validate readings??
            if (GraveDigger.CatchBadReadings(_grave, out var fails))
            {
                MessageBox.Show(string.Join(Environment.NewLine, fails), "Invalid Reading References!");
                return;
            }
            else
            {
                DialogResult = true;
                this.Close();
            }
        }

        private void ChooseServiceButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Import from save of Lutheran Service Bulletin";
            ofd.Filter = "LSB Service (*.html)|*.html";
            if (ofd.ShowDialog() == true)
            {
                _grave.ServicePath = ofd.FileName;
                tbServiceFile.Text = ofd.FileName;
            }
        }

        private void Click_Create(object sender, RoutedEventArgs e)
        {
            Create();
        }

        private void Click_Load(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load Saved Grave Plot";
            ofd.Filter = "LSB Service (*.json)|*.json";
            var savePath = System.IO.Path.Combine(System.IO.Path.Join(System.IO.Path.GetTempPath(), "slidecreaterautosaves"));
            ofd.DefaultDirectory = savePath;
            ofd.InitialDirectory = savePath;
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    using (var stream = File.OpenRead(ofd.FileName))
                    using (var reader = new StreamReader(stream))
                    { 
                        var obj = reader.ReadToEnd();
                        var sobj = JsonSerializer.Deserialize<GravePlot>(obj);
                        ApplyCFGObj(sobj);
                    }
                }
                catch (Exception)
                {
                }
            }

        }
    }
}