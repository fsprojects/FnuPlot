// ----------------------------------------------------------------------------
// Example - demonstrates using gnuplot from F#
// ----------------------------------------------------------------------------
#load "../src/gnuplot.fs"

open FSharp.GnuPlot
open System.Drawing

// ----------------------------------------------------------------------------

// Create gnuplot process
let gp = new GnuPlot()
gp.Set(output = Output(X11, font="arial"))

// Draw a simple function specified as a string
gp.Plot("sin(x)")

// Draw plot using a single series of data
gp.Plot(Series.Lines [2.0; 1.0; 2.0; 5.0]) 

// Specify additional properties of the series 
gp.Plot(Series.Lines( title="Some plot", lineColor = Color.OliveDrab, 
                      weight = 3, data = [2.0; 1.0; 2.0; 5.0]) )

// Histogram is another directly supported type of series                                            
gp.Plot(Series.Histogram( [2.0; 1.0; 2.0; 5.0], title = "Some plot", fill = Solid, 
                          lineColor = Color.SteelBlue, weight = 3) )

// Plot multiple different series into a single plot
gp.Plot
  [ Series.Function( "sin(x*3) + 3", title = "Sinus", 
                     lineColor = Color.Goldenrod, weight = 3)
    Series.Lines( [2.0; 1.0; 2.0; 5.0], title = "Some plot", 
                  lineColor = Color.OliveDrab, weight = 3)
    Series.Histogram( [2.0; 1.0; 2.0; 5.0], title = "Some plot", fill = Solid, 
                      lineColor = Color.SteelBlue, weight = 3) ]

// Specify range of the plot using 'range' named parameter
gp.Plot(
  range = RangeY.[-10.0 .. 10.0 ],
  data = Series.Lines [2.0; 1.0; 2.0; 5.0])  

// Specify range and a style of a plot at once
gp.Plot(
  style = Style(fill=Solid),
  range = Range.[-0.5 .. 3.7, 0.0 .. 6.0],
  data = 
    [ Series.Histogram([2.0; 1.0; 2.0; 5.0], lineColor = Color.OliveDrab)
      Series.Histogram([1.5; 2.0; 2.5; 4.5], lineColor = Color.SteelBlue) ])

// We can also change global properties using the 'Set' method
gp.Set(style = Style(fill = Solid), range = Range.[-0.5 .. 3.7, 0.0 .. 6.0])
// And we can set output to a png fole
gp.Set(output = Output(Png("/home/tomas/Temp/test1.png")))

// The setting is then used by all plots
gp.Plot
  [ Series.Histogram([2.0; 1.0; 2.0; 5.0], lineColor = Color.OliveDrab)
    Series.Histogram([1.5; 2.0; 2.5; 4.5], lineColor = Color.SteelBlue) ]

gp.Set(output = Output(X11, font="arial"))

