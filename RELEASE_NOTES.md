#### 0.1.0-beta - February 4, 2015
* Replaced Series.Lines, Series.XY, Series.TimeY, and Series.Function with a single overloaded Series.Lines which accepts any of the above data types. **Important note:** This will break code which used the old constructors. To fix your legacy code, simply replace all usages of the above constructors with Series.Lines
* Added Series.Points and Series.Impulses constructors. These work exactly like Series.Lines, but instead of plotting a continuous line they plot the datapoints as isolated points or isolated thin bars, respectively.

#### 0.0.5-beta - December 22, 2014
* Adding more documentation, minor code refactoring

#### 0.0.4-beta - December 18, 2014
* Adding tutorial content

#### 0.0.3-beta - December 16, 2014
* Publish documentation

#### 0.0.2-beta - December 16, 2014
* Fixing remaining build issues and releasing

#### 0.0.1-beta - December 16, 2014
* Using project scaffold and initial NuGet release
