FnuPlot: A lightweigtht F# wrapper for gnuplot
==============================================

FnuPlot is a lightweight wrapper for the [gnuplot](http://www.gnuplot.info/) charting and 
visualization library. Since gnuplot is cross-platform (works on Mac, Linux and Windows),
you can use this library on all of the platforms mentioned above.

FnuPlot provides two features on top of gnuplot. First, it hides the gnuplot process, so you 
do not have to start and control the process; you can use the `GnuPlot` type and call it using
`SendCommand`. Second, FnuPlot implements a simple domain-specific language for building a 
number of common chart types.

More information
----------------

For more information about FnuPlot, please refer to the FnuPlot documentation:

  * [FnuPlot documentation](http://fsprojects.github.io/FnuPlot) is the project home page
    with examples, API reference and more information.

  * Read the [Getting started tutorial](http://fsprojects.github.io/FnuPlot/tutorial.html), which contains more detailed description of 
   different charts supported by the library. The [Visualizng population using WorldBank](http://fsprojects.github.io/FnuPlot/worldbank.html)
   tutorial shows how to create the chart above.

 * [API Reference](http://fsprojects.github.io/FnuPlot/reference/index.html) contains automatically generated documentation for all types, 
   modules and functions in the library. This includes additional brief samples on using most of the
   functions.
