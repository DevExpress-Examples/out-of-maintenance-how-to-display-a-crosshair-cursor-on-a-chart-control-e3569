Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Globalization
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Resources
Imports DevExpress.Xpf.Charts
Imports System.Xml.Linq


Namespace DXCharts_DisplayCrosshairCursor
	Partial Public Class MainPage
		Inherits UserControl

		Public Const ToolTipOffset As Double = 5

		Public Sub New()
			InitializeComponent()

			' Bind a chart to a datasource.
			series.DataSource = CreateDataSource()
		End Sub
		' Create datasource from XML.
		#Region "#CreateDataSource"
		Private Function CreateDataSource() As List(Of GoldPrice)
			Dim document As XDocument = DataLoader.LoadXmlFromResources("/Data/GoldPrices.xml")
			Dim goldPrices As New List(Of GoldPrice)()
			If document IsNot Nothing Then
				For Each element As XElement In document.Element("GoldPrices").Elements()
					Dim [date] As DateTime = Convert.ToDateTime(element.Element("Date").Value, CultureInfo.InvariantCulture)
					Dim price As Double = Convert.ToDouble(element.Element("Price").Value, CultureInfo.InvariantCulture)
					goldPrices.Add(New GoldPrice([date], price))
				Next element
			End If
			Return goldPrices
		End Function
		#End Region ' #CreateDataSource

		Private Sub chart_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
			' Clip crosshair lines within a diagram bounds.
			ClipCrosshairLines()

			' Get the current cursor position and transform it to the diagram coordinates.  
'			#Region " #PointToDiagram"
			Dim position As Point = e.GetPosition(chart)
			Dim diagramCoordinates As DiagramCoordinates = diagram.PointToDiagram(position)
'			#End Region '  #PointToDiagram

			If (Not diagramCoordinates.IsEmpty) Then
				' Get a value of a series point that is nearest to the current cursor position.
				Dim seriesValue As Double = GetSeriesValue(series, diagramCoordinates.DateTimeArgument)

				If (Not Double.IsNaN(seriesValue)) Then
					' Specify the text for X and Y crosshair labels.
					valueX.Text = diagramCoordinates.DateTimeArgument.ToShortDateString()
					valueY.Text = "$" & Math.Round(seriesValue)

					' Show the crosshair cursor elements.
					SetCrosshairVisibility(Visibility.Visible)

					' Convert chart coordinates to screen coordinates to place crosshair labels along the scale.
'					#Region " #DiagramToPoint"
					Dim controlCoordinates As ControlCoordinates = diagram.DiagramToPoint(diagramCoordinates.DateTimeArgument, seriesValue)
'					#End Region '  #DiagramToPoint
					PlaceValuesOnAxis(position.X, controlCoordinates.Point.Y)

					' Draw the crosshair cursor.
					Canvas.SetLeft(verticalLine, position.X)
					Canvas.SetTop(horizontalLine, controlCoordinates.Point.Y)
				End If
			Else
				' Hide the crosshair cursor elements.
				SetCrosshairVisibility(Visibility.Collapsed)
			End If
		End Sub

		' Clip crosshair lines into a diagram.
		#Region "#ClipCrosshairLines"
		Private Sub ClipCrosshairLines()
			Dim coordinatesTopLeft As ControlCoordinates = GetTopLeftCoordinates()
			Dim coordinatesBottomRight As ControlCoordinates = GetBottomRightCoordinates()
			Dim geometry As New RectangleGeometry()
			geometry.Rect = New Rect(coordinatesTopLeft.Point, coordinatesBottomRight.Point)
			crosshairCursorCanvas.Clip = geometry
		End Sub
		Private Function GetTopLeftCoordinates() As ControlCoordinates
			Return diagram.DiagramToPoint(CDate(axisX.ActualRange.ActualMinValue), CDbl(axisY.ActualRange.ActualMaxValue))
		End Function
		Private Function GetBottomRightCoordinates() As ControlCoordinates
			Return diagram.DiagramToPoint(CDate(axisX.ActualRange.ActualMaxValue), CDbl(axisY.ActualRange.ActualMinValue))
		End Function
		#End Region ' #ClipCrosshairLines
		' Arrange labels along axes.
		Private Sub PlaceValuesOnAxis(ByVal x As Double, ByVal y As Double)
			Dim coordinatesTopLeft As ControlCoordinates = GetTopLeftCoordinates()
			Dim coordinatesBottomRight As ControlCoordinates = GetBottomRightCoordinates()
			Canvas.SetLeft(valueX, x)
			Canvas.SetTop(valueX, coordinatesBottomRight.Point.Y)
			Canvas.SetLeft(valueY, coordinatesTopLeft.Point.X)
			Canvas.SetTop(valueY, y)
		End Sub

		' Set crosshair cursor visibility.
		Private Sub SetCrosshairVisibility(ByVal newValue As Visibility)
			verticalLine.Visibility = newValue
			horizontalLine.Visibility = newValue
			valueX.Visibility = newValue
			valueY.Visibility = newValue
		End Sub

		' Move the X and Y labels along the axes.
		Private Sub valueX_SizeChanged(ByVal sender As Object, ByVal e As SizeChangedEventArgs)
			Dim transform As TranslateTransform = TryCast(valueX.RenderTransform, TranslateTransform)
			If transform IsNot Nothing Then
				transform.X = -Math.Round(e.NewSize.Width / 2)
				transform.Y = ToolTipOffset
			End If
		End Sub
		Private Sub valueY_SizeChanged(ByVal sender As Object, ByVal e As SizeChangedEventArgs)
			Dim transform As TranslateTransform = TryCast(valueY.RenderTransform, TranslateTransform)
			If transform IsNot Nothing Then
				transform.X = -ToolTipOffset - e.NewSize.Width
				transform.Y = -Math.Round(e.NewSize.Height / 2)
			End If
		End Sub

		' Find a series point that is closest to an argument from chart coordinates.
		Private Function GetSeriesValue(ByVal series As Series, ByVal argument As DateTime) As Double
			For i As Integer = 0 To series.Points.Count - 2
				If series.Points(i).DateTimeArgument = argument Then
					Return series.Points(i).Value
				ElseIf series.Points(i).DateTimeArgument < argument AndAlso series.Points(i + 1).DateTimeArgument > argument Then
					Dim interval1 As TimeSpan = argument - series.Points(i).DateTimeArgument
					Dim interval2 As TimeSpan = series.Points(i + 1).DateTimeArgument - argument
					Return If(interval1 <= interval2, series.Points(i).Value, series.Points(i + 1).Value)
				End If
			Next i
			Return Double.NaN
		End Function

	End Class

	' A class that stores the text to be displayed in crosshair labels.
	Public Class ValueItem
		Inherits Control
		Public Shared ReadOnly TextProperty As DependencyProperty = DependencyProperty.Register("Text", GetType(String), GetType(ValueItem), New PropertyMetadata(String.Empty))

		Public Property Text() As String
			Get
				Return CStr(GetValue(TextProperty))
			End Get
			Set(ByVal value As String)
				SetValue(TextProperty, value)
			End Set
		End Property
	End Class

	' A class to store series points argument and value.
	Public Class GoldPrice
		Private ReadOnly date_Renamed As DateTime
		Private ReadOnly price_Renamed As Double

		Public ReadOnly Property [Date]() As DateTime
			Get
				Return date_Renamed
			End Get
		End Property
		Public ReadOnly Property Price() As Double
			Get
				Return price_Renamed
			End Get
		End Property

		Public Sub New(ByVal [date] As DateTime, ByVal price As Double)
			Me.date_Renamed = [date]
			Me.price_Renamed = price
		End Sub
	End Class

	' A class to load data from an XML file.
	Public NotInheritable Class DataLoader
		Private Sub New()
		End Sub
		Public Shared Function LoadXmlFromResources(ByVal fileName As String) As XDocument
			Try
				fileName = "/DXCharts_DisplayCrosshairCursor;component" & fileName
				Dim uri As New Uri(fileName, UriKind.RelativeOrAbsolute)
				Dim info As StreamResourceInfo = Application.GetResourceStream(uri)
				Return XDocument.Load(info.Stream)
			Catch
				Return Nothing
			End Try
		End Function
	End Class
End Namespace