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

        private static void DrawGPXOnCanvas(IEnumerable<GPXProcessing.TrackPoint> points, Canvas canvas)
        {
            var line = points.CreatePolyline(new Size() { Width = canvas.ActualWidth, Height = canvas.ActualHeight }, 10);
            line.StrokeThickness = 1;
            line.Stroke = Brushes.White;
            var bgLine = points.CreatePolyline(new Size() { Width = canvas.ActualWidth, Height = canvas.ActualHeight }, 10);
            bgLine.Stroke = Brushes.Black;
            bgLine.StrokeThickness = 3;
            bgLine.Opacity = 0.618;

            canvas.Children.Clear();
            canvas.Children.Add(bgLine);
            canvas.Children.Add(line);
        }

        private void BrowseGPXClick(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "*.gpx|*.GPX",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ValidateNames = true
            };
            if (ofd.ShowDialog() == true)
            this.CurrentFile = ofd.FileName;

            _trackPoints = GPXProcessing.ReadGPX(this.CurrentFile);
            DrawGPXOnCanvas(_trackPoints, this.PreviewCanvas);
        }
    }
}
