Lightweigtht F# wrapper for gnuplot
===================================

FnuPlot is a lightweight wrapper for the [gnuplot](http://www.gnuplot.info/) charting and 
visualization library. Since gnuplot is cross-platform (works on Mac, Linux and Windows),
you can use this library on all of the platforms mentioned above.

FnuPlot provides two features on top of gnuplot. First, it hides the gnuplot process, so you 
do not have to start and control the process; you can use the `GnuPlot` type and call it using
`SendCommand`. Second, FnuPlot implements a simple domain-specific language for building a 
number of common chart types.

[![Build Status](https://travis-ci.org/fsprojects/FnuPlot.png?branch=master)](https://travis-ci.org/fsprojects/FnuPlot)
[![NuGet Status](http://img.shields.io/nuget/v/FnuPlot.svg?style=flat)](https://www.nuget.org/packages/FnuPlot/)
[![Issue Stats](http://issuestats.com/github/fsprojects/FnuPlot/badge/issue)](http://issuestats.com/github/fsprojects/FnuPlot)
[![Issue Stats](http://issuestats.com/github/fsprojects/FnuPlot/badge/pr)](http://issuestats.com/github/fsprojects/FnuPlot)

More information
----------------

For more information about FnuPlot, please refer to the FnuPlot documentation:

 * [FnuPlot documentation](http://fsprojects.github.io/FnuPlot) is the project home page
    with examples, API reference and more information.

 * Read the [Getting started tutorial](http://fsprojects.github.io/FnuPlot/tutorial.html), which contains more detailed description of 
   different charts supported by the library. The [Visualizing population using WorldBank](http://fsprojects.github.io/FnuPlot/worldbank.html)
   tutorial shows how to create the chart above.

 * [API Reference](http://fsprojects.github.io/FnuPlot/reference/index.html) contains automatically generated documentation for all types, 
   modules and functions in the library. This includes additional brief samples on using most of the
   functions.


Maintainer(s)
----------------

- [@LukeAllen](https://github.com/LukeAllen)
- [@tpetricek](https://github.com/tpetricek)

The default maintainer account for projects under "fsprojects" is [@fsgit](https://github.com/fsgit) - F# Community Project Incubation Space (repo management)
