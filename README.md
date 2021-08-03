<p align="center">
  <img src="Grok.png" alt="Grok" width="250"/>
</p>

# grok.net
Cross platform .NET grok implementation as a NuGet package

 ![Windows Build](https://github.com/Marusyk/grok.net/workflows/Windows%20Build/badge.svg)
 ![Linux Build](https://github.com/Marusyk/grok.net/workflows/Linux%20Build/badge.svg)
 ![MacOs Build](https://github.com/Marusyk/grok.net/workflows/MacOs%20Build/badge.svg)
 
 [![GitHub release](https://badge.fury.io/gh/Marusyk%2Fgrok.net.svg)](https://github.com/Marusyk/grok.net/releases/tag/1.0.1)
 [![NuGet version](https://badge.fury.io/nu/grok.net.svg)](https://badge.fury.io/nu/grok.net)
 [![Nuget](https://img.shields.io/nuget/dt/grok.net.svg)](https://www.nuget.org/packages/Grok.Net)
 [![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Marusyk/grok.net/blob/main/LICENSE) 
 [![contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/Marusyk/grok.net/blob/main/CONTRIBUTING.md)

# How to Install

You can directly install this library from [Nuget][1]. There is package:

**[grok.net][2]**

    PM> Install-Package Grok.Net
[1]: http://nuget.org
[2]: https://www.nuget.org/packages/Grok.Net

# What is grok

Grok is a great way to parse unstructured log data into something structured and queryable. It sits on top of Regular Expression (regex) and uses text patterns to match lines in log files.

A great way to get started with building yours grok filters is this grok debug tool: https://grokdebug.herokuapp.com/

What can I use Grok for?
 - reporting errors and other patterns from logs and processes
 - parsing complex text output and converting it to json for external processing
 - apply 'write-once use-everywhere' to regular expressions
 - automatically providing patterns for unknown text inputs (logs you want patterns generated for future matching)

The syntax for a grok pattern is `%{SYNTAX:SEMANTIC}`

The `SYNTAX` is the name of [the pattern][3] that will match your text. `SEMANTIC` is the key. 

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

More about [grok][4]

[3]: https://raw.githubusercontent.com/logstash-plugins/logstash-patterns-core/main/patterns/grok-patterns
[4]: https://www.elastic.co/guide/en/logstash/current/plugins-filters-grok.html
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
