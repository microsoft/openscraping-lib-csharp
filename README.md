# OpenScraping HTML Structured Data Extraction <br> C# Library

[![license:isc](https://img.shields.io/badge/license-mit-brightgreen.svg?style=flat-square)](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/LICENSE) [![NuGet Package version](https://img.shields.io/nuget/v/OpenScraping.svg?style=flat-square)](https://www.nuget.org/packages/OpenScraping/) [![Build Status](https://img.shields.io/travis/Microsoft/openscraping-lib-csharp.svg?style=flat-square)](https://travis-ci.org/Microsoft/openscraping-lib-csharp)

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

## Example: Extracting an article from bbc.com

Below is a simple configuration file that extracts an article from [a www.bbc.com page](https://github.com/zmarty/openscraping-lib-nodejs/blob/master/test/www.bbc.com.html).
```javascript
{
  "title": "//div[contains(@class, 'story-body')]//h1",
  "dateTime": "//div[contains(@class, 'story-body')]//div[contains(@class, 'date')]",
  "body": "//div[@property='articleBody']"
}
```

Here is how to call the library:
```csharp
// www.bbc.com.json contains the JSON configuration file pasted above
var jsonConfig = File.ReadAllText(@"www.bbc.com.json");
var config = StructuredDataConfig.ParseJsonString(jsonConfig);

var html = File.ReadAllText(@"www.bbc.com.html", Encoding.UTF8);

var openScraping = new StructuredDataExtractor(config);
var scrapingResults = openScraping.Extract(html);

Console.WriteLine(JsonConvert.SerializeObject(scrapingResults, Formatting.Indented));
```

And here is the result for a bbc news article:
```javascript
{
  title: 'Robert Downey Jr pardoned for 20-year-old drug conviction',
  dateTime: '24 December 2015',
  body: 'Body of the article is shown here'
}
```

Here is how the [www.bbc.com page](https://github.com/zmarty/openscraping-lib-nodejs/blob/master/test/www.bbc.com.html) looked like on the day we saved the HTML for this sample:
<p align="center"><img src='https://i.imgur.com/jVqxuJn.jpg' alt='BBC News example page' width='500'></p>

## Example: Extracting a list of products from Ikea

The sample configuration below is more complex as it demonstrates support for extracting multiple items at the same time, and running transformations on them. For this example we are using a [products page from ikea.com](https://github.com/zmarty/openscraping-lib-nodejs/blob/master/test/www.ikea.com.html).
```javascript
{
  "products": 
  {
    "_xpath": "//div[@id='productLists']//div[starts-with(@id, 'item_')]",
    "title": ".//div[contains(@class, 'productTitle')]",
    "description": ".//div[contains(@class, 'productDesp')]",
    "price": 
    {
      "_xpath": ".//div[contains(@class, 'price')]/text()[1]",
      "_transformations": [
        "TrimTransformation"
      ]
    }
  }
}
```

Here is a snippet of the result:
```javascript
{
  products: [{
    title: 'HEMNES',
    description: 'coffee table',
    price: '$139.00'
  },
...
  {
    title: 'NORDEN',
    description: 'sideboard',
    price: '$149.00'
  },
  {
    title: 'SANDHAUG',
    description: 'tray table',
    price: '$79.99'
  }]
}
```

Here is how the [www.ikea.com page](https://github.com/zmarty/openscraping-lib-nodejs/blob/master/test/www.ikea.com.html) looked like on the day we saved the HTML for this sample:
<p align="center"><img src='https://i.imgur.com/2Q65ybI.jpg' alt='Ikea example page' width='500'></p>

You can find more complex examples in the [unit tests](https://github.com/Microsoft/openscraping-lib-csharp/blob/documentation-nuget/OpenScraping.Tests/StructuredDataExtractionTests.cs).
