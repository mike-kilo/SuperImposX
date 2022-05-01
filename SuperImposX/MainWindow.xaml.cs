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
        #region Singleton

        private static MainWindow? _instance;

        #endregion

        #region Dependency properties

        public static readonly DependencyProperty CurrentFileProperty = DependencyProperty.Register("CurrentFile", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TrackCanvasSizeProperty = DependencyProperty.Register("TrackCanvasSize", typeof(Size), typeof(MainWindow), new PropertyMetadata(new Size() { Width = 100.0, Height = 100.0 }));

        public static readonly DependencyProperty TrackCanvasWidthProperty = DependencyProperty.Register("TrackCanvasWidth", typeof(double), typeof(MainWindow), new PropertyMetadata(100.0), TrackCanvasValidateCallback);

        public static readonly DependencyProperty TrackCanvasOriginXProperty = DependencyProperty.Register("TrackCanvasOriginX", typeof(double), typeof(MainWindow), new PropertyMetadata(0.0), TrackCanvasValidateCallback);

        public static readonly DependencyProperty TrackCanvasOriginYProperty = DependencyProperty.Register("TrackCanvasOriginY", typeof(double), typeof(MainWindow), new PropertyMetadata(0.0), TrackCanvasValidateCallback);

        public static readonly DependencyProperty NewTimeSpanProperty = DependencyProperty.Register("NewTimeSpan", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty NewTimeSpanIsValidProperty = DependencyProperty.Register("NewTimeSpanIsValid", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty RouteDateStartProperty = DependencyProperty.Register("RouteDateStart", typeof(DateTime), typeof(MainWindow), new PropertyMetadata(DateTime.MinValue));

        public static readonly DependencyProperty RouteDateFinishProperty = DependencyProperty.Register("RouteDateFinish", typeof(DateTime), typeof(MainWindow), new PropertyMetadata(DateTime.MaxValue));

        public static readonly DependencyProperty RouteTimeElapsedProperty = DependencyProperty.Register("RouteTimeElapsed", typeof(TimeSpan), typeof(MainWindow), new PropertyMetadata(new TimeSpan(0)));

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
            set { SetValue(TrackCanvasSizeProperty, value); }
        }

        public double TrackCanvasWidth
        {
            get { return (double)GetValue(TrackCanvasWidthProperty); }
            set { SetValue(TrackCanvasWidthProperty, value); }
        }

        public double TrackCanvasOriginX
        {
            get { return (double)GetValue(TrackCanvasOriginXProperty); }
            set { SetValue(TrackCanvasOriginXProperty, value); }
        }

        public double TrackCanvasOriginY
        {
            get { return (double)GetValue(TrackCanvasOriginYProperty); }
            set { SetValue(TrackCanvasOriginYProperty, value); }
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

        public DateTime RouteDateStart
        {
            get { return (DateTime)GetValue(RouteDateStartProperty); }
            set { SetValue(RouteDateStartProperty, value); }
        }

        public DateTime RouteDateFinish
        {
            get { return (DateTime)GetValue(RouteDateFinishProperty); }
            set { SetValue(RouteDateFinishProperty, value); }
        }

        public TimeSpan RouteTimeElapsed
        {
            get { return (TimeSpan)GetValue(RouteTimeElapsedProperty); }
            set { SetValue(RouteTimeElapsedProperty, value); }
        }

        #endregion

        #region Private members

        private static IEnumerable<GPXProcessing.TrackPoint>? _trackPoints;
        
        private static int _trackPointsElapsedCount = 0;

        private static ObservableCollection<Helpers.ElapsedPoint> _trackPointsTime = new ObservableCollection<Helpers.ElapsedPoint>();

        private HeightProfile _heightProfile = null;

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            _instance = this;
            this.DataContext = this;
            this.TrackPointsTime.ItemsSource = _trackPointsTime;
        }

        #endregion

        #region Static methods

        private static void DrawGPXOnCanvas(IEnumerable<GPXProcessing.TrackPoint> points, Canvas canvas, int elapsedCount = 0)
        {
            var trackBounds = points.GetBounds();
            var canvasSize = new Size() { Width = canvas.ActualWidth, Height = canvas.ActualHeight };
            var margin = 10;

            var line = points.CreatePolyline(canvasSize, trackBounds, margin);
            line.StrokeThickness = 2;
            line.Stroke = Brushes.White;
            var bgLine = points.CreatePolyline(canvasSize, trackBounds, margin);
            bgLine.Stroke = Brushes.Black;
            bgLine.StrokeThickness = 4;
            bgLine.Opacity = 0.618;

            canvas.Children.Clear();
            canvas.Children.Add(bgLine);
            canvas.Children.Add(line);

            if (elapsedCount > 0)
            {
                var elapsedLine = points.Take(elapsedCount).CreatePolyline(canvasSize, trackBounds, margin);
                elapsedLine.StrokeThickness = 5;
                elapsedLine.Stroke = Brushes.White;
                var elapsedBgLine = points.Take(elapsedCount).CreatePolyline(canvasSize, trackBounds, margin);
                elapsedBgLine.Stroke = Brushes.Black;
                elapsedBgLine.StrokeThickness = 7;
                elapsedBgLine.Opacity = 0.618;

                var currentPosition = points.Skip(elapsedCount - 1).First().CreateEllipse(canvasSize, trackBounds, margin);

                canvas.Children.Add(elapsedBgLine);
                canvas.Children.Add(elapsedLine);
                canvas.Children.Add(currentPosition);
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
            SetCanvasOrigin(this.TrackCanvas, new Point() { X = this.TrackCanvasOriginX, Y = this.TrackCanvasOriginY });
            DrawGPXOnCanvas(_trackPoints, this.TrackCanvas, _trackPointsElapsedCount);
        }

        private void RedrawHeightProfile()
        {
            if (_heightProfile == null) return;
            _heightProfile.HeightProfileCanvas.Width = 640;
            _heightProfile.HeightProfileCanvas.Height = 100;
            Canvas.SetLeft(_heightProfile.HeightProfileCanvas, 320);
            Canvas.SetTop(_heightProfile.HeightProfileCanvas, 610);
            _heightProfile.HeightProfileCanvas.UpdateLayout();

            _heightProfile.Redraw();
        }

        #endregion

        #region Callbacks

        public static bool TrackCanvasValidateCallback(object value)
        {
            _instance?.RedrawTrackCanvas();
            return true;
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
            this.RouteDateStart = _trackPoints.First().Timestamp;
            this.RouteDateFinish = _trackPoints.Last().Timestamp;
            this.RouteTimeElapsed = this.RouteDateFinish.Subtract(this.RouteDateStart);

            
            if (this._heightProfile != null)
                this.PreviewCanvas.Children.Remove(this._heightProfile.HeightProfileCanvas);
            this._heightProfile = new HeightProfile(_trackPoints);
            this.PreviewCanvas.Children.Add(_heightProfile.HeightProfileCanvas);

            this.RedrawTrackCanvas();
            this.RedrawHeightProfile();
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
                if (_trackPoints == null || !_trackPoints.Any()) return;

                var timeMargin = new TimeSpan(1, 0, 0);
                ofd.FileNames?
                    .Select(f => new { Filename = new System.IO.FileInfo(f).Name, LastWriteTime = new System.IO.FileInfo(f).LastWriteTime })
                    .Where(f => f.LastWriteTime.Add(timeMargin) >= _trackPoints?.First().Timestamp && f.LastWriteTime.Subtract(timeMargin) <= _trackPoints?.Last().Timestamp)
                    .Select(f => new { Filename = f.Filename, Elapsed = f.LastWriteTime.Subtract(_trackPoints.First().Timestamp), FileTime = f.LastWriteTime, })
                    .ToList()
                    .ForEach(f => _trackPointsTime.Add(new Helpers.ElapsedPoint() 
                    { 
                        ElapsedTime = f.Elapsed < new TimeSpan(0) ? new TimeSpan(0) : f.Elapsed, 
                        Filename = System.IO.Path.GetFileNameWithoutExtension(f.Filename),
                        FileTime = f.FileTime,
                    }));
                _trackPointsTime.Sort();
            }
        }

        private void TrackPointsTimeSelected(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView)?.SelectedIndex < 0) return;

            _trackPointsElapsedCount = _trackPoints?
                .TakeWhile(p => p.Timestamp.Subtract(_trackPoints.First().Timestamp) <= _trackPointsTime[(sender as ListView)?.SelectedIndex ?? 0].ElapsedTime)
                .Count() ?? 0;

            this.RedrawTrackCanvas();
            this._heightProfile.Redraw(_trackPointsElapsedCount);
        }

        private void TimeMomentsAddClick(object sender, RoutedEventArgs e)
        {
            var newTimeSpan = new TimeSpan(0);
            if (TimeSpan.TryParse(this.NewTimeSpan, out newTimeSpan))
            {
                _trackPointsTime.Add(new Helpers.ElapsedPoint() { ElapsedTime = newTimeSpan, Filename = newTimeSpan.ToString().Replace(':','.'), FileTime = new DateTime(newTimeSpan.Ticks) });
                _trackPointsTime.Sort();
                this.NewTimeSpan = String.Empty;
            }
        }

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

        private void GenerateSuperimposeImagesClick(object sender, RoutedEventArgs e)
        {
            if (!_trackPointsTime.Any()) return;
            this.PreviewCanvas.SaveCanvasToPNG(
                System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.CurrentFile) ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                $"{System.IO.Path.GetFileNameWithoutExtension(this.CurrentFile)}_{_trackPointsTime[this.TrackPointsTime.SelectedIndex].ElapsedTime.ToString().Replace(':', '.')}_{_trackPointsTime[this.TrackPointsTime.SelectedIndex].Filename}.png"));

            //foreach (var elapsed in _trackPointsTime)
            //{
            //    _trackPointsElapsedCount = _trackPoints?
            //        .TakeWhile(p => p.Timestamp.Subtract(_trackPoints.First().Timestamp) <= elapsed.ElapsedTime)
            //        .Count() ?? 0;

            //    this.RedrawTrackCanvas();
            //    this.TrackCanvas.SaveCanvasToPNG(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.CurrentFile) ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $"{System.IO.Path.GetFileNameWithoutExtension(this.CurrentFile)}_{elapsed.ElapsedTime.ToString().Replace(':', '.')}_{elapsed.Filename}.png"));
            //}
        }

        #endregion
    }
}
