// ----------------------------------------------------------------------------
// GnuPlot.fs - Provides simple wrappers for calling gnuplot from F#
// ----------------------------------------------------------------------------
namespace FnuPlot

open System
open System.Drawing
open System.Diagnostics

// ----------------------------------------------------------------------------
// Various basic types for representing filling, ranges and for formatting
// ----------------------------------------------------------------------------

/// Represents possible fill styles of a plot. A plot can be filled using
/// a solid fill or using a specified pre-defined pattern (represented by 
/// an integer that `gnuplot` understands)
type FillStyle = 
  /// Fill the plot with a solid fill
  | Solid
  /// Fill the plot with a pre-defined gnuplot pattern
  | Pattern of int


/// [omit]
/// Represents an abstract command that sets some property
/// of the plot (and allows undoing the change)
type ICommand = 
  abstract Command : string
  abstract Cleanup : string


/// [omit]
/// Utilities for formatting values and for working with commands
module InternalUtils = 
  /// Formats a value of type option<'a> as a string
  /// using the emtpy string if the value is missing
  let formatArg (f:'T -> string) (a:option<'T>) =
    match a with 
    | None -> ""
    | Some(v) -> (f v)
      
  /// Turns an option value containing some class implementing
  /// ICommand into a list containing exaclty ICommand values
  let commandList (opt:option<'T>) =
    match opt with 
    | Some(cmd) -> [cmd :> ICommand]
    | None -> []

  /// Formats a value of type option<float>
  let formatNum = formatArg (sprintf "%f")
  
open InternalUtils

/// [omit]
module Internal = 
  /// Type that represents a range for a plot (this type is not
  /// intended to be constructed directly - use 'Range.[ ... ]` instead)
  type Range(?xspec, ?yspec) = 
    let xspec = sprintf "set xrange %s\n" (defaultArg xspec "[:]")
    let yspec = sprintf "set yrange %s\n" (defaultArg yspec "[:]")
    interface ICommand with 
      member x.Command = xspec + yspec
      member x.Cleanup = "set autoscale xy" 

  /// Type that allows elegant construction of ranges specifying both X and Y
  type RangeImplXY() = 
    member x.GetSlice(fx, tx, fy, ty) = 
      Range(sprintf "[%s:%s]" (formatNum fx) (formatNum tx),
            sprintf "[%s:%s]" (formatNum fy) (formatNum ty))

  /// Type that allows elegant construction of ranges specifying only X range
  type RangeImplX() = 
    member x.GetSlice(fx, tx) = 
      Range(xspec = sprintf "[%s:%s]" (formatNum fx) (formatNum tx))

  /// Type that allows elegant construction of ranges specifying only Y range
  type RangeImplY() = 
    member x.GetSlice(fy, ty) = 
      Range(yspec = sprintf "[%s:%s]" (formatNum fy) (formatNum ty))

/// Module with values for elegant construction of ranges. This module is automatically
/// opened, so you do not need to open it explicitly. This lets you specify ranges using
/// the following notation:
///
///     // Specify the upper bound on X axis
///     RangeX.[ .. 10.0]
///
///     // Specify the upper bound on Y axis
///     RangeY.[ .. 10.0]
///
///     // Specify both X and Y axes
///     Range.[-10.0 .. , 2.0 .. 8.0]
///
[<AutoOpen>]
module Ranges =
  open Internal
  
  /// Ranges can be constructed using the slicing syntax.
  /// For example:
  ///
  ///     Range.[-10.0 .. , 2.0 .. 8.0]
  let Range = RangeImplXY()
  /// Ranges can be constructed using the slicing syntax.
  /// For example:
  ///
  ///     RangeX.[ .. 10.0]
  let RangeX = RangeImplX()
  /// Ranges can be constructed using the slicing syntax.
  /// For example:
  ///
  ///     RangeY.[ .. 10.0]
  let RangeY = RangeImplY()

// ----------------------------------------------------------------------------
// Formatting of arguments passed to plot and constriction of various types
// of series (Lines, Histogram, ... other options TBD.)
// ----------------------------------------------------------------------------

/// [omit]
/// Module that contains formatting of various gnuplot arguments
module InternalFormat =   
  let formatNumArg s = formatArg (sprintf " %s %d" s)
  let formatTitle = formatArg (sprintf " t '%s'")
  let formatColor s = formatArg (fun (color:Color) ->
    sprintf " %s rgb '#%02x%02x%02x'" s color.R color.G color.B)
  let formatFill = formatArg (function
    | Solid -> " fs solid"
    | Pattern(n) -> sprintf " fs pattern %d" n)
  let formatRotate s = formatArg (sprintf "set %s rotate by %d" s)
  let formatTitles tics = formatArg (fun list ->
    let titles = 
      [ for t, n in Seq.zip list [0 .. (List.length list) - 1] do 
          yield sprintf "\"%s\" %d" t n ]
      |> String.concat ", " 
    sprintf "set %s (%s)" tics titles )
  let formatTimeForXaxis = formatArg (sprintf "set timefmt \"%s\" \nset xdata time")
     
open InternalFormat

/// Data that is passed as an argument to the `Series` type. For most of the types, 
/// we use one of the `Data` cases; `Function` is used when specifying function as a string.
/// You do not need to create `Data` values directly. Instead, use `Series.XY`, `Series.Line`,
/// `Series.TimeY`, `Series.Function` and other.
type Data = 
  /// Sequence of numerical values. The index of an item is the X value, the number is the Y value
  | DataY of float seq 
  /// Sequence of X and Y coordinates. The first element of the tuple is the X value, the second is the Y value
  | DataXY of (float*float) seq  
  /// Sequence of X and Y coordinates where the Y is `DateTime`. The `DateTime` determines the position on the 
  /// X axis. This cannot be mixed with the other Data type options such as DataXY.
  | DataTimeY of (DateTime*float) seq 
  /// A string holding a function of X in the gnuplot format, e.g. `sin(x)`. The range of X comes from the other 
  /// Data series on the plot, or from the optional `Range` object.
  | Function of string 

/// Represents the different types or styles of series.  Roughly corresponds to the gnuPlot 'with lines', 'with points' etc.
type SeriesType =
  /// Series will be displayed as lines
  | Lines
  /// Series will be displayed as a histogram
  | Histogram
  /// Series will be displayed as points
  | Points
  /// Series will be displayed as impulses
  | Impulses

/// Represents a series of data for the `gp.Plot` function
/// The type can be constructed directly (by setting the `plot` parameter
/// to the desired series type) or indirectly using static
/// members such as 'Series.Histogram'
type Series(plot, data, ?title, ?lineColor, ?weight, ?fill) = 
  let plotText =
    match plot with
      | Lines -> "lines"
      | Histogram -> "histogram"
      | Points -> "points"
      | Impulses -> "impulses"
  let cmd = 
    (match data with 
     | DataY _ -> " '-' using 1 with " + plotText
     | DataXY _ | DataTimeY _ -> " '-' using 1:2 with " + plotText
     | Function f -> f + " with " + plotText)
      + (formatTitle title) 
      + (formatNumArg "lw" weight)
      + (formatColor "lc" lineColor) 
      + (formatFill fill)

  static let getType defaultType t =
    match t with
      | None -> defaultType
      | Some ty -> ty
  static let typeWithLinesAsDefault = getType Lines

  /// Returns the data of the series
  member x.Data = data
  /// Returns the formatted gnuplot command
  member x.Command = cmd

  /// Creates a line data series for a plot  
  static member Lines(data, ?title, ?lineColor, ?weight) = 
    Series(Lines, DataY data, ?title=title, ?lineColor=lineColor, ?weight=weight)
  /// Creates an XY data series for a plot  
  static member XY(data, ?title, ?lineColor, ?weight, ?seriesType) = 
    Series(typeWithLinesAsDefault seriesType, DataXY data, ?title=title, ?lineColor=lineColor, ?weight=weight)
  /// Creates a time-series plot from sequence of `DateTime` and value pairs
  static member TimeY(data, ?title, ?lineColor, ?weight, ?seriesType) = 
    Series(typeWithLinesAsDefault seriesType, DataTimeY data, ?title=title, ?lineColor=lineColor, ?weight=weight)
  /// Creates a histogram data series for a plot  
  static member Histogram(data, ?title, ?lineColor, ?weight, ?fill) = 
    Series(Histogram, DataY data, ?title=title, ?lineColor=lineColor, ?weight=weight, ?fill=fill)
  /// Creates a series specified as a function
  static member Function(func, ?title, ?lineColor, ?weight, ?fill, ?seriesType) = 
    Series(typeWithLinesAsDefault seriesType, Function func, ?title=title, ?lineColor=lineColor, ?weight=weight, ?fill=fill)

/// Represents a style of a plot (can be passed to the Plot method
/// to set style for single plotting or to the Set method to set the
/// style globally)
type Style(?fill) = 
  let cmd = 
    [ formatFill fill; ]
    |> List.filter (String.IsNullOrEmpty >> not)
    |> List.map (sprintf "set style %s\n")
    |> String.concat ""
  interface ICommand with 
    member x.Command = cmd
    member x.Cleanup = "" // not implemented
  

/// Various output types that can be specified to gnuplot. Currently, the
/// wrapper supports the following options:
///
///  - `OutputType.X11` for charts that are opened in a new window
///  - `OutputType.Png` for saving charts to a PNG file.
///  - `OutputType.Eps` for saving charts to an EPS file.
type OutputType = 
  /// Creates charts in a new window
  | X11
  /// Saves charts to a specified PNG file
  | Png of string
  /// Saves charts to a specified EPS file
  | Eps of string

/// The type can be used to specify output type for `gnuplot`. The type
/// combines a value of `OutputType` with additional parameters such as fonts.
/// For example, to create a PNG, you can use:
///
///     gp.Set(output = Output(Png("/temp/test.png")))
///
type Output(output:OutputType, ?font) =
  interface ICommand with
    member x.Command = 
      let font = font |> formatArg (sprintf " font '%s'")
      match output with 
      | X11 -> "set term x11" + font
      | Png(s) -> sprintf "set term png%s\nset output '%s'" font s
      | Eps(s) -> sprintf "set term postscript eps enhanced%s\nset output '%s'" font s
    member x.Cleanup = "set term x11"

/// Used to specify titles for the X and Y axes. In addition to the text for the labels,
/// you can also specify the rotation of the labels. For example:
/// 
///     // specify rotated titles for x axis
///     Titles(x=["one"; "two"], xrotate=-70)
///
type Titles(?x, ?xrotate, ?y, ?yrotate) = 
  let cmd =
    [ formatRotate "xtic" xrotate, "set xtic rotate by 0"
      formatRotate "ytic" yrotate, "set ytic rotate by 0"
      formatTitles "xtics" x, "set xtics auto"
      formatTitles "ytics" y, "set ytics auto" ]
    |> List.filter (fun (s, _) -> s <> "")
  interface ICommand with
    member x.Command = 
      cmd |> List.map fst |> String.concat "\n"
    member x.Cleanup =
      cmd |> List.map snd |> String.concat "\n"


/// Used to specify datetime format for the x and y axes, if they contain time data.
/// The parameter `format` is a gnuplot time format; for more information
/// [refer to the gnuplot documentation](http://gnuplot.sourceforge.net/docs_4.2/node274.html).
type TimeFormatX(?format) = 
  let cmd =
    [ formatTimeForXaxis format , "set xdata"]
    |> List.filter (fun (s, _) -> s <> "")
  interface ICommand with
    member x.Command = 
      cmd |> List.map fst |> String.concat "\n"
    member x.Cleanup =
      cmd |> List.map snd |> String.concat "\n"

/// [omit]
/// The below two values control Gnuplot's display of DateTimes. They must match.
[<AutoOpen>]
module timeFormatting = 
  let selectedTimeGnuplotFormat = """%d-%b-%Y-%H:%M:%S""" //format that Gnuplot will expect. see http://gnuplot.sourceforge.net/docs_4.2/node274.html
  let dateTimeToSelectedGnuplotFormat (t:DateTime) = t.ToString("dd-MMM-yyyy-HH:mm:ss") //converts a DateTime to a string in the above format. See http://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx


// ----------------------------------------------------------------------------
// The main type that wraps the gnuplot process
// ----------------------------------------------------------------------------

/// The main type of the library. It provides a wrapper for calling gnuplot from F#. 
/// Plots are drawn using the `Plot` function and can be created using the `Series` type. 
/// For example:
///
///     // Create gnuplot process
///     let gp = GnuPlot()
///
///     // Plot a function specified as a string
///     gp.Plot("sin(x)")
///
///     // Create a simple line plot
///     gp.Plot(Series.Lines [2.0; 1.0; 2.0; 5.0])   
///
///     // Create a histogram plot drawn using blue color 
///     gp.Plot(Series.Histogram(lineColor=Color.Blue, data=[2.0; 1.0; 2.0; 5.0]))
///
type GnuPlot private (actualPath:string) =
  // Start the gnuplot process when the class is created
  let gp = 
    new ProcessStartInfo
      (FileName = actualPath, UseShellExecute = false, Arguments = "", 
       RedirectStandardError = true, CreateNoWindow = true, 
       RedirectStandardOutput = true, RedirectStandardInput = true) 
    |> Process.Start
  

  [<Literal>]
  let DEBUGGING = false

  // Provide event for reading gnuplot messages
  let msgEvent = 
    Event.merge gp.OutputDataReceived gp.ErrorDataReceived
      |> Event.map (fun de -> de.Data)
  do 
    if DEBUGGING then
        msgEvent.Add (fun output -> System.Diagnostics.Debug.Print(output + "\n"))
    gp.BeginOutputReadLine()  
    gp.BeginErrorReadLine()
    gp.EnableRaisingEvents <- true

  // Send command to gnuplot process
  let sendCommand(str:string) =
    gp.StandardInput.Write(str + "\n")
    if DEBUGGING then
        System.Diagnostics.Debug.Print (">>" + str + "\n")

  /// Create a new instance of the `GnuPlot` object. This starts the `gnuplot`
  /// process in the background. The optional parameter `path` can be used to
  /// specify `gnuplot` location if it is not available in `PATH`.
  new(?path:string) = new GnuPlot(actualPath=defaultArg path "gnuplot")

  // We want to dipose of the running process when the wrapper is disposed
  // The followign bits implement proper 'disposal' pattern
  member private x.Dispose(disposing) = 
   gp.Kill()  
   if disposing then gp.Dispose()

  /// [omit]
  override x.Finalize() = 
    x.Dispose(false)
    
  interface System.IDisposable with
    member x.Dispose() = 
      x.Dispose(true)
      System.GC.SuppressFinalize(x)

  // Write data to the gnuplot command line
  member private x.WriteData(data:Data) = 
    match data with 
    | DataY data ->
      for yPt in data do 
        x.SendCommand(string yPt)
      x.SendCommand("e")
    | DataXY data ->
      for (xPt,yPt) in data do 
        x.SendCommand((string xPt) + " " + (string yPt))
      x.SendCommand("e")
    | DataTimeY data ->
      for (timePt,yPt) in data do 
        x.SendCommand( (dateTimeToSelectedGnuplotFormat timePt) + " " + (string yPt))
      x.SendCommand("e")
    | _ -> ()
    
  // --------------------------------------------------------------------------
  // Public members that can be called by the user
  
  /// Send a string command directly to the gnuplot process.
  member x.SendCommand(str) = sendCommand(str)
  
  /// Set a style or a range of the gnuplot session. For example:
  ///
  ///     // set fill style to a numbered pattern
  ///     gp.Set(style = Style(fill = Pattern(3)))
  ///  
  ///     // set the X range of the output plot to [-10:]
  ///     gp.Set(range = RangeX.[-10.0 .. ]
  ///
  member x.Set(?style:Style, ?range:Internal.Range, ?output:Output, ?titles:Titles, ?TimeFormatX:TimeFormatX) = 
    let commands = List.concat [ commandList output; commandList style; commandList range; commandList titles ; commandList TimeFormatX]
    for cmd in commands do
      //printfn "Setting:\n%s" cmd.Command
      x.SendCommand(cmd.Command)

  /// Reset style or range set previously (used mainly internally)
  member x.Unset(?style:Style, ?range:Internal.Range) = 
    let commands = List.concat [ commandList style; commandList range ]
    for cmd in commands do
      if "" <> cmd.Cleanup then x.SendCommand(cmd.Cleanup)
  
  /// Draw a plot specified as a string. Range and style can
  /// be specified using optional parameters. For example:
  ///
  ///     // draw sin(x) function
  ///     gp.Plot("sin(x)")
  ///
  member x.Plot(func:string, ?style, ?range, ?output, ?titles) = 
    x.Plot([Series.Function(func)], ?style=style, ?range=range, ?output=output, ?titles=titles)

  /// Draw a plot of a single data series. Range and style can 
  /// be specified using optional parameters. For example:
  ///
  ///     // Create a simple line plot
  ///     gp.Plot(Series.Lines [2.0; 1.0; 2.0; 5.0],
  ///             range = RangeY.[-1.0 ..])   
  ///    
  member x.Plot(data:Series, ?style, ?range, ?output, ?titles) = 
    x.Plot([data], ?style=style, ?range=range, ?output=output, ?titles=titles)

  /// Draw a plot consisting of multiple data series. Range and 
  /// style can be specified using optional parameters. For example:
  ///
  ///     // Create a simple line plot
  ///     gp.Plot
  ///      [ Series.Lines(title="Lines", data=[2.0; 1.0; 2.0; 5.0])
  ///        Series.Histogram(fill=Solid, data=[2.0; 1.0; 2.0; 5.0]) ]
  ///    
  member x.Plot(data:seq<Series>, ?style:Style, ?range:Internal.Range, ?output:Output, ?titles:Titles) = 
    //Set up the plot format. 
    match (Seq.head data).Data with
        | DataTimeY _ ->    //Plotting time ranges requires special setup of the time format
                            let timeFmt = Some (TimeFormatX(selectedTimeGnuplotFormat))
                            x.Set(?style=style, ?range=range, ?output=output, ?titles=titles, ?TimeFormatX = timeFmt)
        | _ ->  //normal non-time plot
                x.Set(?style=style, ?range=range, ?output=output, ?titles=titles)
    
    //plot each Series
    let cmd = 
      "plot \\\n" +
      ( [ for s in data -> s.Command ]
        |> String.concat ", \\\n" )
    x.SendCommand(cmd)
    for s in data do
      x.WriteData(s.Data)
    //undo plot format setup
    x.Unset(?style=style, ?range=range)

