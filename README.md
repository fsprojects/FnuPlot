*Note: I am no longer maintaining this project or using F#. If someone out there would like take over maintaining it, let me know. -Luke*

Lightweight F# wrapper for gnuplot
===================================

FnuPlot is a lightweight wrapper for the [gnuplot](http://www.gnuplot.info/) charting and 
visualization library. Since gnuplot is cross-platform (works on Mac, Linux and Windows),
you can use Fnuplot on all of those platforms.

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

* See the [Getting started tutorial](http://fsprojects.github.io/FnuPlot/tutorial.html) for usage examples.

* [API Reference](http://fsprojects.github.io/FnuPlot/reference/index.html) contains automatically generated documentation for all types, modules and functions in the library. This includes additional brief samples on using most of the functions.


Maintainer(s)
----------------

- [@LukeAllen](https://github.com/LukeAllen)
- [@tpetricek](https://github.com/tpetricek)

The default maintainer account for projects under "fsprojects" is [@fsprojectsgit](https://github.com/fsprojectsgit) - F# Community Project Incubation Space (repo management)
