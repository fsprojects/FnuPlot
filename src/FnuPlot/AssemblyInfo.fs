namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FnuPlot")>]
[<assembly: AssemblyProductAttribute("FnuPlot")>]
[<assembly: AssemblyDescriptionAttribute("An F# wrapper for the gnuplot charting library")>]
[<assembly: AssemblyVersionAttribute("0.0.3")>]
[<assembly: AssemblyFileVersionAttribute("0.0.3")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.3"
