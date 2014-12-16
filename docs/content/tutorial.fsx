(**
Getting started with FnuPlot 
============================
*)

#r "../../bin/FnuPlot.dll"
open FnuPlot
open System.Drawing

(**
First
*)

// Create gnuplot process
let gp = new GnuPlot()
gp.Set(output = Output(X11, font="arial"))

// Draw a simple function specified as a string
gp.Plot("sin(x)")

(**
Lines
*)

// Draw plot using a single series of data
gp.Plot(Series.Lines [2.0; 1.0; 2.0; 5.0]) 

// Specify additional properties of the series 
gp.Plot(Series.Lines( title="Some plot", lineColor = Color.OliveDrab, 
                      weight = 3, data = [2.0; 1.0; 2.0; 5.0]) )

(**
Histogram
*)

// Histogram is another directly supported type of series                                            
gp.Plot(Series.Histogram( [2.0; 1.0; 2.0; 5.0], title = "Some plot", fill = Solid, 
                          lineColor = Color.SteelBlue, weight = 3) )

(**
Combining
*)

// Plot multiple different series into a single plot
gp.Plot
  [ Series.Function( "sin(x*3) + 3", title = "Sinus", 
                     lineColor = Color.Goldenrod, weight = 3)
    Series.Lines( [2.0; 1.0; 2.0; 5.0], title = "Some plot", 
                  lineColor = Color.OliveDrab, weight = 3)
    Series.Histogram( [2.0; 1.0; 2.0; 5.0], title = "Some plot", fill = Solid, 
                      lineColor = Color.SteelBlue, weight = 3) ]

(**
X Y pairs
*)

// Plot (x,y) pairs
gp.Plot(Series.XY( [(0.0,1.0);(0.2,2.0);(2.0,1.5);(2.1,3.0)] , title = "Some xy plot"))

(**
Time series
*)

open System
gp.Plot(Series.TimeY( [(DateTime(1900,1,1),1.0);(DateTime(1950,1,1),2.0);(DateTime(1980,1,21),1.5);(DateTime(2014,1,1),3.0)], title = "Some time series" ))

(**
Specifying properties
---------------------

Specify range
*)

gp.Plot(
  range = RangeY.[-10.0 .. 10.0 ],
  data = Series.Lines [2.0; 1.0; 2.0; 5.0])  

(**
SPecify range and style
*)

gp.Plot(
  style = Style(fill=Solid),
  range = Range.[-0.5 .. 3.7, 0.0 .. 6.0],
  data = 
    [ Series.Histogram([2.0; 1.0; 2.0; 5.0], lineColor = Color.OliveDrab)
      Series.Histogram([1.5; 2.0; 2.5; 4.5], lineColor = Color.SteelBlue) ])

(**
COnfiguring things
*)

// We can also change global properties using the 'Set' method
gp.Set(style = Style(fill = Solid), range = Range.[-0.5 .. 3.7, 0.0 .. 6.0])
// And we can set output to a png fole
gp.Set(output = Output(Png("/home/tomas/Temp/test1.png")))

// The setting is then used by all plots
gp.Plot
  [ Series.Histogram([2.0; 1.0; 2.0; 5.0], lineColor = Color.OliveDrab)
    Series.Histogram([1.5; 2.0; 2.5; 4.5], lineColor = Color.SteelBlue) ]

gp.Set(output = Output(X11, font="arial"))

