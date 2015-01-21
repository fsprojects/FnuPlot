(*** hide ***)
#I "../../bin"
let pathOpt = 
  [ // MAINTAINERS: Feel free to add other paths where Gnuplot lives on your machine..
    @"C:\Programs\Data\gnuplot\bin\gnuplot.exe"
    @"C:\Program Files (x86)\gnuplot\bin\gnuplot.exe"]
  |> Seq.tryFind System.IO.File.Exists
let path = defaultArg pathOpt "gnuplot"
(**
Getting started with FnuPlot 
============================

FnuPlot is a lightweight wrapper for the [gnuplot](http://www.gnuplot.info/) charting and 
visualization library. Since gnuplot is cross-platform (works on Mac, Linux and Windows),
you can use this library on all of the platforms mentioned above.

FnuPlot provides two features on top of gnuplot. First, it hides the gnuplot process, so you 
do not have to start and control the process; you can use the `GnuPlot` type and call it using
`SendCommand`. Second, FnuPlot implements a simple domain-specific language for building a 
number of common chart types.

Installing and configuring FnuPlot
----------------------------------

FnuPlot is [available on NuGet](http://nuget.org/packages/FnuPlot) and you can use NuGet
to reference it. However, all of the FnuPlot functionality is currently implemented in a
single file, and so you can also copy the `FnuPlot.fs` file to your project. The recommended
way to do this is to use [Paket](http://fsprojects.github.io/Paket/) for managing your
references and use [GitHub reference](http://fsprojects.github.io/Paket/github-dependencies.html)
specified in your `paket.dependencies` file as follows:

    [lang=text]
    github fsprojects/FnuPlot src/FnuPlot/FnuPlot.fs

Once you have the reference, you can either add `FnuPlot.dll` to your reference (when using
a compiled project) or use `#load "FnuPlot.fs"` to load the FnuPlot file obtained using 
Paket. Alternatively, you can use `#r` if you're referencing FnuPlot using NuGet package:
*)
#r "FnuPlot.dll"
open FnuPlot
open System
open System.Drawing
(**
The access to gnuplot is managed through the `GnuPlot` type. When creating an instance, you
can specify the path to the `gnuplot` executable. If you do not pass a path explicitly, then
FnuPlot assumes that you have gnuplot in your `PATH` variable:
*)
let gp_default = new GnuPlot()
let gp = new GnuPlot(path)
(**
The `Set` method on the `GnuPlot` object provides a way to configure `gnuplot`. Here, we specify
that plots should appear in a new window (using X11) and we also specify the default font:
*)
gp.Set(output = Output(X11, font="arial"))
(**

Plotting functions and line charts
----------------------------------

The `Plot` method provided by `GnuPlot` has a number of overloads. You can specify a function
to be drawn (as a string), you can specify a single data series or you can specify multiple
series (which can also use different chart types).

The following shows how to call `Plot` with a function specified as a string:
*)
gp.Plot("sin(x)")
(**
<img src="img/tutorial-1.png" alt="Sin function" />

This creates a chart with a single series, created (implicitly) by specifying the function
as a string. If you want to create other kinds of series, you need to use the `Series` type.
The following uses `Series.Lines` to create a single line chart from a specified list of
data points:
*)
gp.Plot(Series.Lines [2.0; 1.0; 2.0; 5.0]) 
(**
<img src="img/tutorial-2.png" alt="Line series" />

The `Series.Lines` method takes a number of optional parameters that you can use to specify
the title, colour and other properties of the line chart. You can also make the call using
the pipelining operator:
*)
Series.Lines
  ( title="Some plot", lineColor = Color.OliveDrab, 
    weight = 3, data = [2.0; 1.0; 2.0; 5.0])
|> gp.Plot
(**
<img src="img/tutorial-3.png" alt="Line series with configuration" />

When creating a line series from data points consisting of both X and Y values, you can use
the `Series.XY` method (note that FnuPlot also supports time-series data sets, which are 
discussed below).
*)
Series.XY( [0.0,1.0; 0.2,2.0; 2.0,1.5; 2.1,3.0] , title = "Some xy plot")
|> gp.Plot
(**
<img src="img/tutorial-4.png" alt="Line series with X and Y values" />
*)
(**
Plotting histograms and time-series
-----------------------------------

Another kind of chart for which FnuPlot provides an easy-to-use wrapper is histogram. This can
be created using the `Series.Histogram` function. Again, we can specify additional properties
using a number of optional parameters:
*)
Series.Histogram
  ( [2.0; 5.0; 2.0; 1.0], title = "Some plot", fill = Solid, 
    lineColor = Color.SteelBlue, weight = 3) 
|> gp.Plot
(**
<img src="img/tutorial-5.png" alt="Histogram" />

As already mentioned, FnuPlot also supports time-series charts. These can be created using the
`Series.TimeY` method. This method takes the values as pairs of `DateTime` and `float`:
*)
let values =
  [ DateTime(2014,1,1), 1.0
    DateTime(2014,1,5), 3.0
    DateTime(2014,2,1), 1.5 ]

Series.TimeY(values, title = "Some time series" )
|> gp.Plot
(**
<img src="img/tutorial-6.png" alt="Time-series" />

Combining multiple chart series
-------------------------------

As mentioned earlier, the `Plot` function has a number of overloads. We have already
seen an overload that takes a string (to plot a specified function) and an overload that
takes a single series. However, you can also call `Plot` with a collection of series. 
In that case, gnuplot will render multiple series into a single chart.

The following (slightly silly) demo combines the `sin(x*3)+3)` function specified as
a string together with line chart and histogram created from values:
*)
[ Series.Function
    ( "sin(x*3) + 3", title = "Sinus", 
      lineColor = Color.Goldenrod, weight = 3)
  Series.Lines
    ( [2.0; 1.0; 2.0; 5.0], title = "Line", 
      lineColor = Color.OliveDrab, weight = 3)
  Series.Histogram
    ( [2.0; 5.0; 2.0; 1.0], title = "Hist", fill = Solid, 
      lineColor = Color.SteelBlue, weight = 3) ]
|> gp.Plot
(**
<img src="img/tutorial-7.png" alt="Combining multiple charts" />

Configuring ranges and styles
-----------------------------

So far, we always specified properties of individual chart series locally using optional
parameters of a `Series.<Some>` method. However, the `gp.Plot` method also takes a number
of additional parameters that can be used to specify properties of the whole chart. Most
importantly, you can use these to specify ranges:
*)
gp.Plot(
  range = RangeY.[-10.0 .. 10.0 ],
  data = Series.Lines [2.0; 1.0; 2.0; 5.0])  
(**
<img src="img/tutorial-8.png" alt="Combining multiple charts" />

To specify a range, you can use `RangeY` (as shown in the code snippet) or `RangeX` to set
the range for one of the two axes. If you want to configure both ranges, you can use the
`Range` type. For example, to specify X axis range from -10 to 10 and Y axis range from 
-1 to 1, you can write `Range.[-10.0 .. 10.0, -1.0 .. 1.0]`.

The next example shows how to specify fill style for the whole chart, so that you do not have
to repeat this for every single histogram that is being combined:
*)
gp.Plot(
  style = Style(fill=Solid),
  range = Range.[-0.5 .. 3.7, 0.0 .. 6.0],
  data = 
    [ Series.Histogram([2.0; 1.0; 2.0; 5.0], lineColor = Color.OliveDrab)
      Series.Histogram([1.5; 2.0; 2.5; 4.5], lineColor = Color.SteelBlue) ])
(**
<img src="img/tutorial-9.png" alt="Combining multiple charts" />

Saving charts to a file
-----------------------
At the beginning of the tutorial, we used the `gp.Set` method to specify the output kind for
gnuplot. We used `Output(X11, font="arial")` to use the X11 server, which means that all 
charts are created in a gnuplot window. 

If you want to save charts to a file, then you can use the `Png` output. The following 
(slightly longer) example also demonstrates other optional parameters of the `gp.Set` 
method. This gives you another way to specify the range and style - this time, the configuration
will apply to all created charts:
*)
// Set global properties using the 'Set' method
gp.Set(style = Style(fill = Solid), range = Range.[-0.5 .. 3.7, 0.0 .. 6.0])
// Specify the 'Png' output kind
gp.Set(output = Output(Png("/home/tomas/Temp/test1.png")))

// Create a plot (which will be saved to a file)
gp.Plot
  [ Series.Histogram([2.0; 1.0; 2.0; 5.0], lineColor = Color.OliveDrab)
    Series.Histogram([1.5; 2.0; 2.5; 4.5], lineColor = Color.SteelBlue) ]

// Reset the configuration back to X11 plots
gp.Set(output = Output(X11, font="arial"))

(**

Plotting series in different styles
-----------------------------------
We can also use the 'seriesType' parameter when creating a new series to specify how it is drawn.

For instance in the demo below we plot one function as 'impulses' and one as 'points'.

*)

[ Series.Function( "besj0(x)*0.12e1", title = "Plot as Impulses", seriesType = Impulses)
  Series.Function( "(x**besj0(x))-2.5", title = "Plot as Points", seriesType = Points)]
|> gp.Plot

(**
<img src="img/tutorial-10.png" alt="Different series style" />
*)