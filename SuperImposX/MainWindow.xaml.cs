using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace SuperImposX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string CurrentFile
        {
            get { return (string)GetValue(CurrentFileProperty); }
            set { SetValue(CurrentFileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentFile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentFileProperty =
            DependencyProperty.Register("CurrentFile", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        private static IEnumerable<GPXProcessing.TrackPoint>? _trackPoints;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void BrowseGPXClick(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "*.gpx|*.GPX";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Multiselect = false;
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            ofd.ValidateNames = true;
            if (ofd.ShowDialog() == true)
            this.CurrentFile = ofd.FileName;

            _trackPoints = GPXProcessing.ReadGPX(this.CurrentFile);
            var bounds = _trackPoints.GetBounds();
        }
    }
}
