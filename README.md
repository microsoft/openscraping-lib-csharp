# OpenScraping HTML Structured Data Extraction <br> C# Library

[![license:isc](https://img.shields.io/badge/license-mit-brightgreen.svg?style=flat-square)](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/LICENSE) [![NuGet Package version](https://img.shields.io/nuget/v/OpenScraping.svg?style=flat-square)](https://www.nuget.org/packages/OpenScraping/) [![Build Status](https://img.shields.io/travis/Microsoft/openscraping-lib-csharp.svg?style=flat-square)](https://travis-ci.org/Microsoft/openscraping-lib-csharp)

Turn unstructured HTML pages into structured data. The OpenScraping library can extract information from HTML pages using a JSON config file with xPath rules. It can scrape even multi-level complex objects such as tables and forum posts. 

This library is used in production to scrape thousands of pages. 

The latest NuGet package is .NET Standard 2.0, which means it can be used **both in .NET Core 2.0+ and .NET Framework 4.6.1+ projects**.

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

Below are a few of the built-in transformations, with links to example rules. To see how the example rules are tested, please check the code of the [test class](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/StructuredDataExtractionTests.cs), as well as the HTML files in the [TestData folder](https://github.com/Microsoft/openscraping-lib-csharp/tree/master/OpenScraping.Tests/TestData).

Name                                     | Purpose | Example
---------------------------------------- | ------- | --------------
AbbreviatedIntegerTranformation          | Converts strings like "9k" to the integer *9,000*, or "2m" to *2,000,000*, or "5B" to *5,000,000,000*. | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/quora.com.json)
CastToIntegerTransformation              | Converts strings to the corresponding integer. For example, converts "12450" to the integer *12,450*. | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/stackexchange.com.json)
ExtractIntegerTransformation             | Tries to find an integer in the middle of a string. For instance, for the string "Popularity: 1000 views" it extracts the integer *1,000*. Note that if the string would have a comma after the 1, it would just extract *1* as the integer. | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/answers.microsoft.com.json)
ListTitleTransformation                  | Tries to find the "title" of the current unordered or ordered HTML list by looking for some text just above the list. | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/support.office.com.json)
RemoveExtraWhitespaceTransformation      | Replaces consecutive spaces with a single space. For the string "hello&nbsp;&nbsp;&nbsp;&nbsp;world" it would return "hello world". | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/stackexchange.com.json)
SplitTransformation                      | Splits the string into an array based on a separator. | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/support.office.com.json)
TotalTextLengthAboveListTransformation   | Tries to determine the length of the text which is above an unordered or ordered HTML list. | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/answers.microsoft.com.json)
TrimTransformation                       | Runs  [String.Trim()](https://msdn.microsoft.com/en-us/library/system.string.trim(v=vs.110).aspx) on the extracted text before it gets written to the JSON output. | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/article_with_comments_div.json)
RegexTransformation                       | Matches text with regular expressions | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/regex_rules.json)
ParseDateTransformation                       | Converts text to date | [Here](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/parse_date_rules.json)

### Writing custom transformations

You can implement custom transformations in your own code. The library will pick them up through reflection. There are two types of transformations, ones that act on incoming HTML (first transformation in the chain), and ones that act on the output of previous transformations. The first kind implement ITransformationFromHtml and the second one implement ITransformationFromObject. You can actually have one transformation implement both interfaces, such as the [ParseDateTransformation](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping/Transformations/ParseDateTransformation.cs).

## Remove unwanted HTML tags and XPath nodes before extracting content

Let's say you want to extract a news article but before the actual extraction you would like to remove some HTML nodes. You can do that in two ways. The first (deprecated) way is to use the the **\_removeTags** setting, where you can list names of HTML tags that need to be removed before we start processing the xPath rules. The second (better) way is setting the **\_removeXPaths** setting, which allows listing XPath rules to find nodes that we want to remove BEFORE we process the normal \_xpath extraction rules.

Example HTML:
```html
<!DOCTYPE html>

<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>Page title</title>
</head>
<body>
    <h1>  Article  title    </h1>
    <div id="primary">
        <script>alert('test');</script>

        <p>Para1     content</p>
        <p>Para2   content</p>

        <div id="comments">
            <p>Comment1</p>
            <p>Comment2</p>
        </div>
    </div>
</body>
</html>
```

JSON config:
```javascript
{
  "_removeTags": [
    "script"
  ],
  "title": {
    "_xpath": "//h1",
    "_transformations": [
      "TrimTransformation"
    ]
  },
  "body": {
    "_removeXPaths": [
      "./div[@id='comments']"
    ],
    "_xpath": "//div[@id='primary']",
    "_transformations": [
      "RemoveExtraWhitespaceTransformation"
    ]
  }
}
```

Result:
```javascript
{
  "title": "Article  title",
  "body": "Para1 content Para2 content"
}
```

## MultiExtractor: Load multiple xPath rule config files and match on URL

You can use MultiExtractor to load multiple xPath rule JSON config files for different websites, then allow the code to pick the correct rule depending on the URL you provide. This is useful, for example, if you are parsing a network of websites with similar HTML design but with different URLs. For example, check these two JSON config files: [stackexchange.com.json](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/stackexchange.com.json) and [answers.microsoft.com.json](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/TestData/answers.microsoft.com.json). The first config file defines multiple potential URL patterns that can match that config:

```javascript
{
    "_urlPatterns": [
        "^https?:\/\/.+\\.stackexchange\\.com\/questions\/[0-9]+\/.+$",
        "^https?:\/\/stackoverflow\\.com\/questions\/[0-9]+\/.+$",
        "^https?:\/\/serverfault\\.com\/questions\/[0-9]+\/.+$",
        "^https?:\/\/superuser\\.com\/questions\/[0-9]+\/.+$",
        "^https?:\/\/askubuntu\\.com\/questions\/[0-9]+\/.+$"
    ],
...
}
```

We can load multiple of these config files into a MultiExtractor, then we can pass in an HTML file and its corresponding URL. MultiExtractor will then goes over the **\_urlPatterns** in each config file, it picks the config file which matches the URL, then applies the corresponding rules.
```csharp
var multiExtractor = new MultiExtractor(configRootFolder: "TestData", configFilesPattern: "*.json");
var json = multiExtractor.ParsePage(
	url: "http://answers.microsoft.com/en-us/windows/forum/windows_10-win_upgrade/i-want-to-reserve-my-free-copy-of-windows-10-but-i/9c3f7f56-3da8-4b40-a30f-e33772439ee1", 
	html: File.ReadAllText(Path.Combine("TestData", "answers.microsoft.com.html")));
```

To see a full example search for the **MultiWebsiteExtractionTest()** function in the [test class](https://github.com/Microsoft/openscraping-lib-csharp/blob/master/OpenScraping.Tests/StructuredDataExtractionTests.cs).
