using System;
using System.Collections.Generic;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;
using DevExpress.Xpf.Charts;
using System.Xml.Linq;


namespace DXCharts_DisplayCrosshairCursor {
    public partial class MainPage : UserControl {

        public const double ToolTipOffset = 5;

        public MainPage() {
            InitializeComponent();

            // Bind a chart to a datasource.
            series.DataSource = CreateDataSource();
        }
        // Create datasource from XML.
        #region #CreateDataSource
        List<GoldPrice> CreateDataSource() {
            XDocument document = DataLoader.LoadXmlFromResources("/Data/GoldPrices.xml");
            List<GoldPrice> goldPrices = new List<GoldPrice>();
            if (document != null) {
                foreach (XElement element in document.Element("GoldPrices").Elements()) {
                    DateTime date = Convert.ToDateTime(element.Element("Date").Value, CultureInfo.InvariantCulture);
                    double price = Convert.ToDouble(element.Element("Price").Value, CultureInfo.InvariantCulture);
                    goldPrices.Add(new GoldPrice(date, price));
                }
            }
            return goldPrices;
        }
        #endregion #CreateDataSource

        void chart_MouseMove(object sender, MouseEventArgs e) {
            // Clip crosshair lines within a diagram bounds.
            ClipCrosshairLines();

            // Get the current cursor position and transform it to the diagram coordinates.  
            #region  #PointToDiagram
            Point position = e.GetPosition(chart);
            DiagramCoordinates diagramCoordinates = diagram.PointToDiagram(position);
            #endregion  #PointToDiagram

            if (!diagramCoordinates.IsEmpty) {
                // Get a value of a series point that is nearest to the current cursor position.
                double seriesValue = GetSeriesValue(series, diagramCoordinates.DateTimeArgument);

                if (!double.IsNaN(seriesValue)) {
                    // Specify the text for X and Y crosshair labels.
                    valueX.Text = diagramCoordinates.DateTimeArgument.ToShortDateString();
                    valueY.Text = "$" + Math.Round(seriesValue);

                    // Show the crosshair cursor elements.
                    SetCrosshairVisibility(Visibility.Visible);

                    // Convert chart coordinates to screen coordinates to place crosshair labels along the scale.
                    #region  #DiagramToPoint
                    ControlCoordinates controlCoordinates = diagram.DiagramToPoint(diagramCoordinates.DateTimeArgument, seriesValue);
                    #endregion  #DiagramToPoint
                    PlaceValuesOnAxis(position.X, controlCoordinates.Point.Y);

                    // Draw the crosshair cursor.
                    Canvas.SetLeft(verticalLine, position.X);
                    Canvas.SetTop(horizontalLine, controlCoordinates.Point.Y);
                }
            }
            else {
                // Hide the crosshair cursor elements.
                SetCrosshairVisibility(Visibility.Collapsed);
            }
        }

        // Clip crosshair lines into a diagram.
        #region #ClipCrosshairLines
        void ClipCrosshairLines() {
            ControlCoordinates coordinatesTopLeft = GetTopLeftCoordinates();
            ControlCoordinates coordinatesBottomRight = GetBottomRightCoordinates();
            RectangleGeometry geometry = new RectangleGeometry();
            geometry.Rect = new Rect(coordinatesTopLeft.Point, coordinatesBottomRight.Point);
            crosshairCursorCanvas.Clip = geometry;
        }
        ControlCoordinates GetTopLeftCoordinates() {
            return diagram.DiagramToPoint((DateTime)axisX.ActualRange.ActualMinValue, (double)axisY.ActualRange.ActualMaxValue);
        }
        ControlCoordinates GetBottomRightCoordinates() {
            return diagram.DiagramToPoint((DateTime)axisX.ActualRange.ActualMaxValue, (double)axisY.ActualRange.ActualMinValue);
        }
        #endregion #ClipCrosshairLines
        // Arrange labels along axes.
        void PlaceValuesOnAxis(double x, double y) {
            ControlCoordinates coordinatesTopLeft = GetTopLeftCoordinates();
            ControlCoordinates coordinatesBottomRight = GetBottomRightCoordinates();
            Canvas.SetLeft(valueX, x);
            Canvas.SetTop(valueX, coordinatesBottomRight.Point.Y);
            Canvas.SetLeft(valueY, coordinatesTopLeft.Point.X);
            Canvas.SetTop(valueY, y);
        }

        // Set crosshair cursor visibility.
        void SetCrosshairVisibility(Visibility newValue) {
            verticalLine.Visibility = newValue;
            horizontalLine.Visibility = newValue;
            valueX.Visibility = newValue;
            valueY.Visibility = newValue;
        }

        // Move the X and Y labels along the axes.
        void valueX_SizeChanged(object sender, SizeChangedEventArgs e) {
            TranslateTransform transform = valueX.RenderTransform as TranslateTransform;
            if (transform != null) {
                transform.X = -Math.Round(e.NewSize.Width / 2);
                transform.Y = ToolTipOffset;
            }
        }
        void valueY_SizeChanged(object sender, SizeChangedEventArgs e) {
            TranslateTransform transform = valueY.RenderTransform as TranslateTransform;
            if (transform != null) {
                transform.X = -ToolTipOffset - e.NewSize.Width;
                transform.Y = -Math.Round(e.NewSize.Height / 2);
            }
        }

        // Find a series point that is closest to an argument from chart coordinates.
        double GetSeriesValue(Series series, DateTime argument) {
            for (int i = 0; i < series.Points.Count - 1; i++) {
                if (series.Points[i].DateTimeArgument == argument)
                    return series.Points[i].Value;
                else if (series.Points[i].DateTimeArgument < argument && series.Points[i + 1].DateTimeArgument > argument) {
                    TimeSpan interval1 = argument - series.Points[i].DateTimeArgument;
                    TimeSpan interval2 = series.Points[i + 1].DateTimeArgument - argument;
                    return interval1 <= interval2 ? series.Points[i].Value : series.Points[i + 1].Value;
                }
            }
            return double.NaN;
        }

    }

    // A class that stores the text to be displayed in crosshair labels.
    public class ValueItem : Control {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ValueItem), new PropertyMetadata(string.Empty));

        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
    }

    // A class to store series points argument and value.
    public class GoldPrice {
        readonly DateTime date;
        readonly double price;

        public DateTime Date { get { return date; } }
        public double Price { get { return price; } }

        public GoldPrice(DateTime date, double price) {
            this.date = date;
            this.price = price;
        }
    }

    // A class to load data from an XML file.
    public static class DataLoader {
        public static XDocument LoadXmlFromResources(string fileName) {
            try {
                fileName = "/DXCharts_DisplayCrosshairCursor;component" + fileName;
                Uri uri = new Uri(fileName, UriKind.RelativeOrAbsolute);
                StreamResourceInfo info = Application.GetResourceStream(uri);
                return XDocument.Load(info.Stream);
            }
            catch {
                return null;
            }
        }
    }
}
