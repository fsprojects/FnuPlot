(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"
let names = []
let stats = []
(**
Lightweight F# wrapper for gnuplot
==================================

FnuPlot is a lightweight wrapper for the [gnuplot](http://www.gnuplot.info/) charting and 
visualization library, which lets you create cross-platform, publication-quality charts
on Mac, Linux and Windows. FnuPlot provides two features on top of gnuplot: 

 - First, it hides the gnuplot process, so you do not have to start and control the process.
   You can easily starts `gnuplot` in the background just by creating a new instance of the
   `GnuPlot` type. Then you can send commands to `gnuplot` easily using the `SendCommand` method.

 - Second, FnuPlot implements a simple domain-specific language for building a number of common 
   chart types. This means that you can use `gp.Plot` to plot charts for functions, line charts,
   histograms and a few other charts and you can use `gp.Set` to configure `gnuplot`.

Visualizing population of the world
-----------------------------------

To give see a quick example of the DSL for building `gnuplot` charts, have a look at the following 
example from the [visualizing population using WorldBank data] tutorial. The tutorial downloads
data from the WorldBank and then uses FnuPlot to create a single chart combining three histograms
for three different years:
*)        
// Starts 'gnuplot' process interactively
#r "FnuPlot.dll"
open FnuPlot
let gp = new GnuPlot()

// Configure chart style, ranges and titles
gp.Set
  ( style = Style(Solid), 
    range = RangeY.[ 5e8 .. 75e8 ],
    titles = Titles(x = names, xrotate = -90) )

// Create a chart combining several histograms
[ for year, values, color in stats ->
    Series.Histogram
      ( data = values, title = string year, 
        lineColor = color) ]
|> gp.Plot
(**

The example first creates `GnuPlot` (assuming that the executable is in your `PATH`). Then it calls
`gp.Set` to configure the chart, setting the Y axis range and titles. Then it creates a sequence of
histograms (with different line colours) that are then passed to `gp.Plot` to display the plot. The
resulting chart looks as follows:

<img src="img/worldbank.png" alt="WorldBank data visualization" />

How to get FnuPlot
------------------

 * The library is available as [FnuPlot on NuGet](https://www.nuget.org/packages/FnuPlot). To get the
   code also, [get the code from GitHub](https://github.com/fsprojects/FnuPlot/).

 * All of the FnuPlot functionality is currently implemented in a single file, and so you can also copy the 
   `FnuPlot.fs` file to your project. The recommended way to do this is to use [Paket](http://fsprojects.github.io/Paket/) 
   for managing your references and use [GitHub reference](http://fsprojects.github.io/Paket/github-dependencies.html)
   specified in your `paket.dependencies` file using `github fsprojects/FnuPlot src/FnuPlot/FnuPlot.fs`.

Samples & documentation
-----------------------

The FnuPlot library comes with a documentation that is automatically generated from `*.fsx` files in 
[the content folder][content].  This means that you can also open the code in your favourite F# editor
and run it interactively.

 * Read the [Getting started tutorial](tutorial.html), which contains more detailed description of 
   different charts supported by the library. The [Visualizing population using WorldBank](worldbank.html)
   tutorial shows how to create the chart above.

 * [API Reference](reference/index.html) contains automatically generated documentation for all types, 
   modules and functions in the library. This includes additional brief samples on using most of the
   functions.
 
Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding a new public API, please also 
consider adding [samples][content] that can be turned into a documentation. 

The library is available under Apache 2.0 license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/fsprojects/FnuPlot/tree/master/docs/content
  [gh]: https://github.com/fsprojects/FnuPlot
  [issues]: https://github.com/fsprojects/FnuPlot/issues
  [readme]: https://github.com/fsprojects/FnuPlot/blob/master/README.md
  [license]: https://github.com/fsprojects/FnuPlot/blob/master/LICENSE.md
*)
