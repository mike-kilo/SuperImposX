using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        #region Dependency properties

        public static readonly DependencyProperty CurrentFileProperty = DependencyProperty.Register("CurrentFile", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TrackCanvasSizeProperty = DependencyProperty.Register("TrackCanvasSize", typeof(Size), typeof(MainWindow), new PropertyMetadata(new Size() { Width = 100.0, Height = 100.0 }));

        public static readonly DependencyProperty TrackCanvasWidthProperty = DependencyProperty.Register("TrackCanvasWidth", typeof(double), typeof(MainWindow), new PropertyMetadata(100.0));

        public static readonly DependencyProperty TrackCanvasOriginProperty = DependencyProperty.Register("TrackCanvasOrigin", typeof(Point), typeof(MainWindow), new PropertyMetadata(new Point() { X = 0, Y = 0 }));

        public static readonly DependencyProperty NewTimeSpanProperty = DependencyProperty.Register("NewTimeSpan", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty NewTimeSpanIsValidProperty = DependencyProperty.Register("NewTimeSpanIsValid", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        #endregion

        #region Properties

        public string CurrentFile
        {
            get { return (string)GetValue(CurrentFileProperty); }
            set { SetValue(CurrentFileProperty, value); }
        }

        public Size TrackCanvasSize
        {
            get { return (Size)GetValue(TrackCanvasSizeProperty); }
            set 
            { 
                SetValue(TrackCanvasSizeProperty, value);
                this.TrackCanvas.UpdateLayout();
            }
        }

        public double TrackCanvasWidth
        {
            get { return (double)GetValue(TrackCanvasWidthProperty); }
            set 
            { 
                SetValue(TrackCanvasWidthProperty, value);
                if (_trackPoints == null) return;
                var bounds = _trackPoints.GetBounds();
                var aspect = (bounds.Max.Longitude - bounds.Min.Longitude) / (bounds.Max.Latitude - bounds.Min.Latitude);
                SetCanvasSize(this.TrackCanvas, new Size() { Width = value, Height = value / aspect });
            }
        }

        public Point TrackCanvasOrigin
        {
            get { return (Point)GetValue(TrackCanvasOriginProperty); }
            set 
            { 
                SetValue(TrackCanvasOriginProperty, value);
                Canvas.SetLeft(this.TrackCanvas, value.X);
                Canvas.SetTop(this.TrackCanvas, value.Y);
            }
        }

        public string NewTimeSpan
        {
            get { return (string)GetValue(NewTimeSpanProperty); }
            set { SetValue(NewTimeSpanProperty, value); }
        }

        public bool NewTimeSpanIsValid
        {
            get { return (bool)GetValue(NewTimeSpanIsValidProperty); }
            set { SetValue(NewTimeSpanIsValidProperty, value); }
        }

        #endregion

        #region Private members
        
        private static IEnumerable<GPXProcessing.TrackPoint>? _trackPoints;
        
        private static int _trackPointsElapsedCount = 0;

        private static ObservableCollection<TimeSpan> _trackPointsTime = new ObservableCollection<TimeSpan>();

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.TrackPointsTime.ItemsSource = _trackPointsTime;
        }

        #endregion

        #region Static methods

        private static void DrawGPXOnCanvas(IEnumerable<GPXProcessing.TrackPoint> points, Canvas canvas, int elapsedCount = 0)
        {
            var trackBounds = points.GetBounds();

            var line = points.CreatePolyline(new Size() { Width = canvas.ActualWidth, Height = canvas.ActualHeight }, trackBounds, 10);
            line.StrokeThickness = 1;
            line.Stroke = Brushes.White;
            var bgLine = points.CreatePolyline(new Size() { Width = canvas.ActualWidth, Height = canvas.ActualHeight }, trackBounds, 10);
            bgLine.Stroke = Brushes.Black;
            bgLine.StrokeThickness = 3;
            bgLine.Opacity = 0.618;

            canvas.Children.Clear();
            canvas.Children.Add(bgLine);
            canvas.Children.Add(line);

            if (elapsedCount > 0)
            {
                var elapsedLine = points.Take(elapsedCount).CreatePolyline(new Size() { Width = canvas.ActualWidth, Height = canvas.ActualHeight }, trackBounds, 10);
                elapsedLine.StrokeThickness = 3;
                elapsedLine.Stroke = Brushes.White;
                var elapsedBgLine = points.Take(elapsedCount).CreatePolyline(new Size() { Width = canvas.ActualWidth, Height = canvas.ActualHeight }, trackBounds, 10);
                elapsedBgLine.Stroke = Brushes.Black;
                elapsedBgLine.StrokeThickness = 5;
                elapsedBgLine.Opacity = 0.618;

                canvas.Children.Add(elapsedBgLine);
                canvas.Children.Add(elapsedLine);
            }
        }

        public static void SetCanvasSize(Canvas canvas, Size size)
        {
            canvas.Width = size.Width;
            canvas.Height = size.Height;
            canvas.UpdateLayout();
        }

        public static void SetCanvasOrigin(Canvas canvas, Point origin)
        {
            Canvas.SetLeft(canvas, origin.X);
            Canvas.SetTop(canvas, origin.Y);
        }

        #endregion

        #region Methods

        private void RedrawTrackCanvas()
        {
            if (_trackPoints == null) return;

            var bounds = _trackPoints.GetBounds();
            var aspect = (bounds.Max.Latitude - bounds.Min.Latitude) / (bounds.Max.Longitude - bounds.Min.Longitude);
            SetCanvasSize(this.TrackCanvas, new Size() { Width = this.TrackCanvasWidth, Height = this.TrackCanvasWidth * aspect });
            SetCanvasOrigin(this.TrackCanvas, this.TrackCanvasOrigin);
            DrawGPXOnCanvas(_trackPoints, this.TrackCanvas, _trackPointsElapsedCount);
        }

        #endregion

        #region Event handlers

        private void BrowseGPXClick(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "GPX|*.gpx;*.GPX",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ValidateNames = true
            };
            if (ofd.ShowDialog() == true)
            this.CurrentFile = ofd.FileName;

            _trackPoints = GPXProcessing.ReadGPX(this.CurrentFile);
            this.RedrawTrackCanvas();
        }

        private void RedrawClick(object sender, RoutedEventArgs e)
        {
            this.RedrawTrackCanvas();
        }

        private void TimeMomentsLoadClick(object sender, RoutedEventArgs e)
        {
            if (_trackPoints == null) return;
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Media files|*.mp4;*.avi;*.jpg;*.jpeg;*.png|All files|*.*",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = true,
                InitialDirectory = System.IO.Path.GetDirectoryName(this.CurrentFile),
                ValidateNames = true
            };
            if (ofd.ShowDialog() == true)
            {
                ofd.FileNames?
                    .Select(f => new System.IO.FileInfo(f).LastWriteTime)
                    .Where(f => f >= _trackPoints?.First().Timestamp && f <= _trackPoints.Last().Timestamp)
                    .Select(f => f.Subtract(_trackPoints.First().Timestamp))
                    .ToList()
                    .ForEach(f => _trackPointsTime.Add(f));
                _trackPointsTime.Sort();
            }
        }

        private void TrackPointsTimeSelected(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView)?.SelectedIndex < 0) return;

            _trackPointsElapsedCount = _trackPoints?
                .TakeWhile(p => p.Timestamp.Subtract(_trackPoints.First().Timestamp) <= _trackPointsTime[(sender as ListView)?.SelectedIndex ?? 0])
                .Count() ?? 0;

            this.RedrawTrackCanvas();
        }

        private void TimeMomentsAddClick(object sender, RoutedEventArgs e)
        {
            var newTimeSpan = new TimeSpan(0);
            if (TimeSpan.TryParse(this.NewTimeSpan, out newTimeSpan))
            {
                _trackPointsTime.Add(newTimeSpan);
                _trackPointsTime.Sort();
                this.NewTimeSpan = String.Empty;
            }
        }

        #endregion

        private void TimeMomentsClearClick(object sender, RoutedEventArgs e)
        {
            if (this.TrackPointsTime.SelectedIndex < 0) return;
            var selectedIndex = this.TrackPointsTime.SelectedIndex;
            _trackPointsTime.RemoveAt(this.TrackPointsTime.SelectedIndex);
            this.TrackPointsTime.SelectedIndex = (int)Math.Min(Math.Max(0, selectedIndex), this.TrackPointsTime.Items.Count - 1);
        }

        private void NewTimeSpanChanged(object sender, TextChangedEventArgs e)
        {
            this.NewTimeSpanIsValid = TimeSpan.TryParse((sender as TextBox)?.Text, out _);
        }
    }
}
