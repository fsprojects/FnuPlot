(*** hide ***)
#I "../../bin/"
#I "../../packages/FSharp.Data/lib/net40"
let pathOpt = 
  [ // MAINTAINERS: Feel free to add other paths where Gnuplot lives on your machine..
    "C:/Programs/Data/gnuplot/bin/gnuplot.exe" 
    "/usr/local/bin/gnuplot"
    "C:/Program Files (x86)/gnuplot/bin/gnuplot.exe"]
  |> Seq.tryFind System.IO.File.Exists
let path = defaultArg pathOpt "gnuplot"
(**
Visualizng population using WorldBank
=====================================

In this walkthrough, we look at a larger example of data analysis that uses FnuPlot to
visualize the results. We use the [WorldBank](http://data.worldbank.org) as the data source
and we plot how the total population in the countries of the world changed between 1990 and 2005. The
example is inspired by the [Asynchrnous and data-driven programming chapter](http://manning.com/petricek)
from Real-World Functional Programming.

Downloading data from the WorldBank
-----------------------------------

In the chapter used as the inspiration, the data download is done using the LINQ to XML library
and the `XDocument` type. Here, we use F# type providers instead. We use XML type provider to
parse the data and WorldBank type provider to find the ID of the required indicator.

First, we connect to the WorldBank to get the ID and write a function that sends HTTP request
to download one page of data for the specified indicator:
*)
#r "FSharp.Data.dll"
open FSharp.Data

// Get indicator code from WorldBank
let wb = WorldBankData.GetDataContext()
let indCode = wb.Countries.World.Indicators.``Population, total``.IndicatorCode
let root = "http://api.worldbank.org/countries/indicators/"
let key = "hq8byg8k7t2fxc6hp7jmbx26"

/// Asynchronously downloads the population 
/// data for the specified year & page
let asyncGetPage year page = 
  let range = sprintf "%d:%d" year year
  Http.AsyncRequestString
    ( root + indCode, 
      query=[ "api_key", key; "per_page", "100"; 
              "date", range; "page", string page ] )
(**
Now, we want to save a sample response to a local file and use the XML type provider
to parse the response. The easiest way to do this is to use `File.WriteAllText` to 
save a sample result, say for `asyncGetPage 2000 1` to a local file. Here, we use 
`worldbank.xml` in the current folder, so we can load the type provider as follows:
*)
#r "System.Xml.Linq.dll"
type WB = XmlProvider<"worldbank.xml">
(**
The generated type `WB` lets us parse the responses returned by `asyncGetPage`. To
download all data, we first need to request the first page, so that we know how many
pages in total are there. Then we can download the rest of the pages in parallel. This
is done in the following `async` function:
*)
let asyncGetPopulation year = async {
  // Download the first page & get total no. of pages
  let! first = asyncGetPage year 1
  let parsed = WB.Parse(first)

  // Download the remaining pages in parallel
  let! rest = 
    [ for pg in 2 .. parsed.Pages -> async {
        let! response = asyncGetPage year pg 
        return WB.Parse(response) } ]
    |> Async.Parallel
  // Return all pages
  return Seq.append [parsed] rest }
(**
The return type of `asyncGetPopulation` is `Async<seq<WB.Data>>`, which means that it 
_asynchronously_ returns a collection of `<data>` elements from the XML document. Now,
we use `Async.Parallel` again to download data for 3 different years:
*)
let allData = 
  [ for y in [ 1990; 2000; 2005 ] ->
      asyncGetPopulation y ]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> Seq.concat
(**
The operation creates a list of asynchronous operations, composes them so that they are
performed asynchronously, runs them and then it concatenates the results, so that we get
just a single collection of `<data>` nodes.

Processing downloaded data
--------------------------

Before we can do the visualization, we need to find the countries for which we have all
the data. To do that, we build two lookup tables. One that maps country code and year to
a value and another, which maps country code to a country name. 
*)
// Lookup table mapping IDs to country names
let idToName = 
  [ for d in allData do
      for item in d.Datas do
        yield item.Country.Id, item.Country.Value ]
  |> dict

// Lookup table mapping ID and Year to a value
let idAndYearToValue =
  [ for d in allData do
      for item in d.Datas do
        if item.Value.IsSome then
          yield (item.Country.Id, item.Date), item.Value.Value ]
  |> dict

(**
Now that we have the two dictionaries, we can create collections of numbers in a format
that can be passed to `gnuplot`. First of all, we want to display data only for some of
the countries. We choose countries and regions that have data for all 3 years and have 
population over 500 million. This will give us large countries and aggregated regions that
WorldBank monitors as a whole.

Next, we need to pick the names of the regions/countries (for axis labels) and a list of
collections with numerical values for each year:
*)

/// Returns true if data is available for all 3 monitored years
/// and the country or region has over 500 million
let isVisibleCountry id =
  [1990; 2000; 2005] |> Seq.forall (fun y ->
    match idAndYearToValue.TryGetValue( (id, y) ) with
    | true, v -> v > int64 5e8
    | _ -> false )

/// Names of visible countries/regions for axis labels
let names = 
  [ for KeyValue(id, name) in idToName do
      if isVisibleCountry id then yield name ]

/// List of tuples consisting of a year and data for the year
let stats = 
  [ for y in [1990; 2000; 2005] do
     let data =
      [ for KeyValue(id, name) in idToName do
          if isVisibleCountry id then 
            yield float (idAndYearToValue.[id, y]) ]
     yield y, data ] 
(**
Visualizing data with gnuplot
-----------------------------

To create a plot using `gnuplot`, we first need to reference the `FnuPlot.dll`
library, open the required namespace and create `GnuPlot` instance. Here, we pass
the path to the `gnuplot` executable as an argument. If it is available directly in
your path, you can just call `new GnuPlot()` without parameters:
*)
#r "FnuPlot.dll"
open FnuPlot
open System.Drawing

let gp = new GnuPlot(path)
(**
To make the chart nicer, we configure a number of options first. We specify the output
type to X11 (to create chart in a window) and a font. Then we also set the range and 
the style of histogram bars (filled). Finally, we add titles and specify that the chart
should be rotated (from the bottom to the top):
*)
gp.Set
  ( output = Output(X11, font = "arial"),
    style = Style(Solid), 
    range = RangeY.[ 5e8 .. 75e8 ],
    titles = Titles(x = names, xrotate = -90) )
(**
We want to create histogram using custom colors, so we'll zip the data with the following
list of colors:
*)
let colors = [ Color.OliveDrab; Color.SteelBlue; Color.Goldenrod ]
(**
Now we have all we need to create the chart. We use `gp.Plot` to display the plot. As an 
argument, we give it a collection of series created using `Series.Histogram`. When 
createing a series, we specify the `lineColor` parameter to get the required color (one
color for each year):
*)        
gp.Plot
  [ for (y, values), clr in Seq.zip stats colors ->
      Series.Histogram
        ( data = values, title = string y, lineColor = clr) ]
(**
<img src="img/worldbank.png" alt="WorldBank data visualization" />
*)