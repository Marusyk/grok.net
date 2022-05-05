![Grok](https://github.com/Marusyk/grok.net/raw/main/Grok.png)

Cross platform .NET grok implementation as a NuGet package

 [![Build](https://github.com/Marusyk/grok.net/actions/workflows/builds.yml/badge.svg?branch=main)](https://github.com/Marusyk/grok.net/actions/workflows/builds.yml)
 [![GitHub release](https://badge.fury.io/gh/Marusyk%2Fgrok.net.svg)](https://github.com/Marusyk/grok.net/releases/tag/v1.1.0)
 [![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Marusyk/grok.net/blob/main/LICENSE) 
 [![contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/Marusyk/grok.net/blob/main/CONTRIBUTING.md)
 
 [![NuGet version](https://badge.fury.io/nu/grok.net.svg)](https://badge.fury.io/nu/grok.net)
 [![Nuget](https://img.shields.io/nuget/dt/grok.net.svg)](https://www.nuget.org/packages/Grok.Net)
 [![PowerShell Gallery Version](https://img.shields.io/powershellgallery/v/Grok)](https://www.powershellgallery.com/packages/Grok)
 [![PowerShell Gallery](https://img.shields.io/powershellgallery/dt/Grok)](https://www.powershellgallery.com/packages/Grok)

# Code Coverage

[![Coverage Status](https://coveralls.io/repos/github/Marusyk/grok.net/badge.svg)](https://coveralls.io/github/Marusyk/grok.net)

# How to Install

Install as a library from [Nuget](http://nuget.org):

**[Grok.Net](https://www.nuget.org/packages/Grok.Net)**

    PM> Install-Package Grok.Net
    
Install as a PowerShell module from [PowershellGallery](https://www.powershellgallery.com):

**[Grok](https://www.powershellgallery.com/packages/Grok)**

```powershell
Install-Module -Name Grok
```

# What is grok

Grok is a great way to parse unstructured log data into something structured and queryable. It sits on top of Regular Expression (regex) and uses text patterns to match lines in log files.

A great way to get started with building yours grok filters is this grok debug tool: https://grokdebug.herokuapp.com/

What can I use Grok for?
 - reporting errors and other patterns from logs and processes
 - parsing complex text output and converting it to json for external processing
 - apply 'write-once use-everywhere' to regular expressions
 - automatically providing patterns for unknown text inputs (logs you want patterns generated for future matching)

The syntax for a grok pattern is `%{SYNTAX:SEMANTIC}`

The `SYNTAX` is the name of [the pattern](https://raw.githubusercontent.com/logstash-plugins/logstash-patterns-core/main/patterns/grok-patterns) that will match your text. `SEMANTIC` is the key. 

For example, `3.44` will be matched by the `NUMBER` pattern and `55.3.244.1` will be matched by the `IP` pattern. `3.44` could be the duration of an event, so you could call it simply `duration`. Further, a string `55.3.244.1` might identify the `client` making a request.
For the above example, your grok filter would look something like this:

```
%{NUMBER:duration} %{IP:client}
```
Examples: With that idea of a syntax and semantic, we can pull out useful fields from a sample log like this fictional http request log:

```
55.3.244.1 GET /index.html 15824 0.043
```
The pattern for this could be:

```
%{IP:client} %{WORD:method} %{URIPATHPARAM:request} %{NUMBER:bytes} %{NUMBER:duration}
```

More about [grok](https://www.elastic.co/guide/en/logstash/current/plugins-filters-grok.html)

# How to use

Create a new instance with grok pattern:

```csharp
Grok grok = new Grok("%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}");
```

then prepare some logs to parse

```csharp
string logs = @"06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165
                06-21-19 21:00:13:589265;156;WARN;main;DECODED: 775233900043 EMPTY DISTANCE: --------";
```

You are ready to parse and print result

```csharp
var grokResult = grok.Parse(logs);
foreach (var item in grokResult)
{
  Console.WriteLine($"{item.Key} : {item.Value}");
}
```

# Custom grok patterns

There is the possibility to add your own patterns.
Create a file and write the pattern you need as the pattern name, space, then the regexp for that pattern.

For example, Patterns\grok-custom-patterns:

```
ZIPCODE [1-9]{1}[0-9]{2}\s{0,1}[0-9]{3}
```

then load the file and pass the stream to Grok:

```csharp
FileStream customPatterns = System.IO.File.OpenRead(@"Patterns\grok-custom-patterns");
Grok grok = new Grok("%{ZIPCODE:zipcode}:%{EMAILADDRESS:email}", customPatterns);
var grokResult = grok.Parse($"122001:Bob.Davis@microsoft.com");
```

# PowerShell Module

Install and use the Grok as a PowerShell module

```powershell
grok -i "06-21-19 21:00:13:589241;15;INFO;main;DECODED: 775233900043 DECODED BY: 18500738 DISTANCE: 1.5165" -g "%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}"
```
To get help use `help grok` command

## Build

On Windows: 
```powershell
build.ps1
```

On Linux/Mac:
```bash
build.sh
```

## Contributing

Would you like to help make grok.net even better? We keep a list of issues that are approachable for newcomers under the [good-first-issue](https://github.com/Marusyk/grok.net/issues?q=is%3Aopen+is%3Aissue+label%3A%22good+first+issue%22) label.

Also. please read [CONTRIBUTING.md](https://github.com/Marusyk/grok.net/blob/main/CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## License

This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/Marusyk/grok.net/blob/main/LICENSE) file for details

Thanks to [@martinjt](https://github.com/martinjt). The project is based on [martinjt/grokdotnet](https://github.com/martinjt/grokdotnet). 
