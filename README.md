FSharp.GnuPlot
==============

An F# wrapper for the gnuplot charting library

Requires gnuplot to be installed separately: http://sourceforge.net/projects/gnuplot/files/

The samples folder shows how to use it in F# interactive. To use it in a compiled F# project:
* Copy src/gnuplot.fs into your project
* Create a plot like this:

```fsharp
let gp = new GnuPlot()
gp.Plot(Series.XY( [(0.0,1.0);(0.2,2.0);(2.0,1.5);(2.1,3.0)] , title = "Some xy plot"))
```
If the gnuplot folder is not in your system path, you may need to specify it like this:
```fsharp
let gp = new GnuPlot("C:\\Program Files (x86)\\gnuplot\\bin\\gnuplot.exe")
```