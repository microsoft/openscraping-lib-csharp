# OpenScraping HTML Structured Data Extraction <br> C# Library

[![license:isc](https://img.shields.io/badge/license-mit-brightgreen.svg?style=flat-square)](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/LICENSE) [![Build Status](https://img.shields.io/travis/Microsoft/openscraping-lib-csharp.svg?style=flat-square)](https://travis-ci.org/Microsoft/openscraping-lib-csharp)

Turn unstructured HTML pages into structured data. The OpenScraping library can extract information from HTML pages using a JSON config file with xPath rules. It can scrape even multi-level complex objects such as tables and forum posts. 

This library is used in production to scrape thousands of pages. 

This is the **C#** version. A separate but similar **Node.js** library is located [here](https://github.com/zmarty/openscraping-lib-nodejs).

## Self-contained example

Create a new console C# project, then install the [OpenScraping NuGet package](https://www.nuget.org/packages/OpenScraping/) by using the GUI or by using this command in the [Package Manager Console](http://docs.nuget.org/consume/package-manager-console):

```PowerShell
Install-Package OpenScraping
```

Then paste and run the following code:

```C#
namespace OpenScrapingTest
{
    using System;
    using Newtonsoft.Json;
    using OpenScraping;
    using OpenScraping.Config;

    class Program
    {
        static void Main(string[] args)
        {
            var configJson = @"
            {
                'title': '//h1',
                'body': '//div[contains(@class, \'article\')]'
            }
            ";

            var config = StructuredDataConfig.ParseJsonString(configJson);

            var html = "<html><body><h1>Article title</h1><div class='article'>Article contents</div></body></html>";

            var openScraping = new StructuredDataExtractor(config);
            var scrapingResults = openScraping.Extract(html);

            Console.WriteLine(scrapingResults["title"]);
            Console.WriteLine("----------------------------");
            Console.WriteLine(JsonConvert.SerializeObject(scrapingResults, Formatting.Indented));
            Console.ReadKey();
        }
    }
}
```

The output looks like this:
```
Article title
----------------------------
{
  "title": "Article title",
  "body": "Article contents"
}
```

You can find more complex examples in the [unit tests](https://github.com/Microsoft/openscraping-lib-csharp/blob/documentation-nuget/OpenScraping.Tests/StructuredDataExtractionTests.cs).

## Documentation TODO
* More examples in README
* Modify MultiExtractor and document it
