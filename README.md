# OpenScraping HTML Structured Data Extraction <br> C# Library

[![license:isc](https://img.shields.io/badge/license-mit-brightgreen.svg?style=flat-square)](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/LICENSE) [![NuGet Package version](https://img.shields.io/nuget/v/OpenScraping.svg?style=flat-square)](https://www.nuget.org/packages/OpenScraping/) [![Build Status](https://img.shields.io/travis/Microsoft/openscraping-lib-csharp.svg?style=flat-square)](https://travis-ci.org/Microsoft/openscraping-lib-csharp)

Turn unstructured HTML pages into structured data. The OpenScraping library can extract information from HTML pages using a JSON config file with xPath rules. It can scrape even multi-level complex objects such as tables and forum posts. 

This library is used in production to scrape thousands of pages. 

The latest NuGet package is only compatible with .Net Core 2.0 projects. If your project uses .Net Framework, please use version 0.0.5 of the package.

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

Below is a simple configuration file that extracts an article from [a www.bbc.com page](https://github.com/OpenScraping/openscraping-lib-nodejs//blob/master/test/www.bbc.com.html).
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

Here is how the [www.bbc.com page](https://github.com/OpenScraping/openscraping-lib-nodejs//blob/master/test/www.bbc.com.html) looked like on the day we saved the HTML for this sample:
<p align="center"><img src='https://i.imgur.com/jVqxuJn.jpg' alt='BBC News example page' width='500'></p>

## Example: Extracting a list of products from Ikea

The sample configuration below is more complex as it demonstrates support for extracting multiple items at the same time, and running transformations on them. For this example we are using a [products page from ikea.com](https://github.com/OpenScraping/openscraping-lib-nodejs//blob/master/test/www.ikea.com.html).
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

Here is how the [www.ikea.com page](https://github.com/OpenScraping/openscraping-lib-nodejs//blob/master/test/www.ikea.com.html) looked like on the day we saved the HTML for this sample:
<p align="center"><img src='https://i.imgur.com/2Q65ybI.jpg' alt='Ikea example page' width='500'></p>

You can find more complex examples in the [unit tests](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/StructuredDataExtractionTests.cs).

## Transformations

In the Ikea example above we used a transformation called *TrimTransformation*. Transformation modify the raw extracted HTML nodes in some ways. For instance, TrimTransformation just runs [String.Trim()](https://msdn.microsoft.com/en-us/library/system.string.trim(v=vs.110).aspx) on the extracted text before it gets written to the JSON output.

Here are a few of the built-in transformations:

Name                                     | Purpose | Example
---------------------------------------- | ------- | --------------
AbbreviatedIntegerTranformation          | Converts strings like "9k" to the integer *9,000*, or "2m" to *2,000,000*, or "5B" to *5,000,000,000*. | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/quora.com.json)
CastToIntegerTransformation              | Converts strings to the corresponding integer. For example, converts "12450" to the integer *12,450*. | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/stackexchange.com.json)
ExtractIntegerTransformation             | Tries to find an integer in the middle of a string. For instance, for the string "Popularity: 1000 views" it extracts the integer *1,000*. Note that if the string would have a comma after the 1, it would just extract *1* as the integer. | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/answers.microsoft.com.json)
ListTitleTransformation                  | Tries to find the "title" of the current unordered or ordered HTML list by looking for some text just above the list. | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/support.office.com.json)
RemoveExtraWhitespaceTransformation      | Replaces consecutive spaces with a single space. For the string "hello&nbsp;&nbsp;&nbsp;&nbsp;world" it would return "hello world". | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/stackexchange.com.json)
SplitTransformation                      | Splits the string into an array based on a separator. | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/support.office.com.json)
TotalTextLengthAboveListTransformation   | Tries to determine the length of the text which is above an unordered or ordered HTML list. | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/answers.microsoft.com.json)
TrimTransformation                       | Runs  [String.Trim()](https://msdn.microsoft.com/en-us/library/system.string.trim(v=vs.110).aspx) on the extracted text before it gets written to the JSON output. | 
