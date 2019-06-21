<!-- default file list -->
*Files to look at*:

* [MainPage.xaml](./CS/DXCharts_DisplayCrosshairCursor/MainPage.xaml) (VB: [MainPage.xaml](./VB/DXCharts_DisplayCrosshairCursor/MainPage.xaml))
* [MainPage.xaml.cs](./CS/DXCharts_DisplayCrosshairCursor/MainPage.xaml.cs) (VB: [MainPage.xaml.vb](./VB/DXCharts_DisplayCrosshairCursor/MainPage.xaml.vb))
<!-- default file list end -->
# How to display a crosshair cursor on a chart control
<!-- run online -->
**[[Run Online]](https://codecentral.devexpress.com/e3569)**
<!-- run online end -->


<p><strong>Note: This example applies to a</strong><strong> </strong><strong>DX</strong><strong>Charts version prior to v2012 vol 1. Starting from v2012 vol 1, a crosshair cursor is provided out-of-the-box and is enabled by default for all XY-series views.</strong></p><p>In some cases, you may need to follow some data changes on the diagram. It can be done easily using a crosshair cursor.</p><p>The following example demonstrates how to display a crosshair cursor on the <a href="http://documentation.devexpress.com/#WPF/clsDevExpressXpfChartsLineSeries2Dtopic"><u>LineSeries2D</u></a> chart and show the current series coordinates on the cross hair labels.</p><br />



<h3>Description</h3>

<p>To accomplish these tasks, you need to convert mouse coordinates to the diagram coordinates  and vice versa  using the<strong> XYDiagram2D.PointToDiagram </strong>and  <strong>XYDiagram2D.DiagramToPoint</strong>  methods correspondingly.</p><p>After the coordinate  transformation is done , it becomes possible to draw the crosshair cursor on the diagram and do other required customizations that are shown in code.     </p><br />
<p><br />
</p>

<br/>


