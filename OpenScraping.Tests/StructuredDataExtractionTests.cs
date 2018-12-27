// -----------------------------------------------------------------------
// <copyright file="StructuredDataExtractionTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Search.StructuredDataExtraction.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using OpenScraping;
    using OpenScraping.Config;
    using System;
    using System.Globalization;
    using System.IO;

    [TestClass]
    public class StructuredDataExtractionTests
    {
        [TestMethod]
        public void MicrosoftAnswersExtractionTest()
        {
            var configPath = Path.Combine("TestData", "answers.microsoft.com.json");
            var config = StructuredDataConfig.ParseJsonFile(configPath);
            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(File.ReadAllText(Path.Combine("TestData", "answers.microsoft.com.html")));
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);

            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            // Question
            Assert.AreNotEqual(null, parsedJson["question"], "Extractor should find a question in the HTML file");
            
            var question = parsedJson["question"];
            Assert.AreEqual("8acb1ac5-0acd-4c68-9eeb-e4afff5b39d8", question["id"].Value, "The extracted id is incorrect");
            Assert.AreEqual("I want to reserve my free copy of Windows 10, but I don’t see the icon on the taskbar", question["title"].Value, "The extracted title is incorrect");
            Assert.AreNotEqual(null, question["content"], "The extracted question should have a content");
            Assert.IsTrue(question["content"].Value.Length > 0, "The extracted question content should have a length > 0");
            Assert.AreEqual(1642653, question["views"].Value, "The extracted views snippet is incorrect");

            // Question context
            Assert.AreNotEqual(null, question["hints"], "The extracted question should have hints");
            Assert.AreEqual(4, question["hints"].Count, "The extracted question should have 4 hints");
            Assert.AreEqual("PC", question["hints"][3].ToString(), "The 4th hint of the extracted question should be PC");

            // Answers
            Assert.AreNotEqual(null, parsedJson["answers"], "Extractor should find answers in the HTML file");
            Assert.AreEqual(2, parsedJson["answers"].Count, "Extractor should find two answers in the thread summary of the HTML file");

            var secondAnswer = parsedJson["answers"][1];
            Assert.AreEqual("Most Helpful Reply", secondAnswer["type"].Value, "The extracted type of the answer is incorrect");
            Assert.AreNotEqual(null, secondAnswer["content"], "The content array in the extracted answer should not be null");
            Assert.IsTrue(secondAnswer["content"].Count > 0, "The content array in the extracted answer should have one or more items");
            Assert.AreEqual(4, secondAnswer["lists"].Count, "The lists array should have 4 items");
            Assert.IsTrue(secondAnswer["lists"][0]["items"].Count > 0, "First item in the lists array should have at least one item");

            // Check is textAboveLength exists in each list
            foreach (var answer in parsedJson["answers"])
            {
                var lists = answer["lists"];

                if (lists != null)
                {
                    foreach (var list in lists)
                    {
                        Assert.AreEqual(JTokenType.Integer, list["textAboveLength"].Type, "The extracted textAboveLength should be an integer");
                        var textAboveLength = ((JValue)list["textAboveLength"]).ToObject<int>();
                        Assert.IsTrue(textAboveLength > 0, string.Format(CultureInfo.InvariantCulture, "textAboveLength was not greater than 0. The extracted value is: {0}", textAboveLength));
                    }
                }
            }
        }

        [TestMethod]
        public void OfficeSupportExtractionTest()
        {
            var configPath = Path.Combine("TestData", "support.office.com.json");
            var config = StructuredDataConfig.ParseJsonFile(configPath);
            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(File.ReadAllText(Path.Combine("TestData", "support.office.com.html")));
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);

            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            Assert.AreEqual("Export data to Excel", parsedJson["title"].Value, "The extracted title is incorrect");
            Assert.AreEqual("You can copy data from a Microsoft Office Access 2007 database into a worksheet by exporting a database object to a Microsoft Office Excel 2007 workbook. You do this by using the Export Wizard in Office Access 2007.", parsedJson["abstract"].Value, "The extracted abstract is incorrect");
            Assert.AreEqual(9, parsedJson["versions"].Count, "The extracted versions field is incorrect");

            Assert.AreEqual(5, parsedJson["sections"].Count, "The extracted json should have 5 sections");

            var secondSection = parsedJson["sections"][1];
            Assert.AreEqual("Exporting data to Excel: the basics", secondSection["title"].Value, "The title of the second section is incorrect");
            Assert.AreEqual(9, secondSection["text"]["paragraphs"].Count, "The paragraphs count of the second section is incorrect");
            Assert.AreEqual(2, secondSection["text"]["unorderedLists"].Count, "The paragraphs count of the second section is incorrect");

            var secondList = secondSection["text"]["unorderedLists"][1];
            Assert.AreEqual("If this is the first time you are exporting data to Excel", secondList["title"].Value, "The title of the second list in the second section is incorrect");
            Assert.AreEqual(4, secondList["items"].Count, "The second list in the second section should have 4 items");
        }

        [TestMethod]
        public void MultiWebsiteExtractionTest()
        {
            var multiExtractor = new MultiExtractor(configRootFolder: "TestData", configFilesPattern: "*.json");
            var json = multiExtractor.ParsePage(
                url: "http://answers.microsoft.com/en-us/windows/forum/windows_10-win_upgrade/i-want-to-reserve-my-free-copy-of-windows-10-but-i/9c3f7f56-3da8-4b40-a30f-e33772439ee1", 
                html: File.ReadAllText(Path.Combine("TestData", "answers.microsoft.com.html")));

            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            // Question
            Assert.AreNotEqual(null, parsedJson["question"], "Extractor should find a question in the HTML file");

            var question = parsedJson["question"];
            Assert.AreEqual("I want to reserve my free copy of Windows 10, but I don’t see the icon on the taskbar", question["title"].Value, "The extracted title is incorrect");
            Assert.AreNotEqual(null, question["content"], "The extracted question should have a content");
            Assert.IsTrue(question["content"].Value.Length > 0, "The extracted question content should have a length > 0");
            Assert.AreEqual(1642653, question["views"].Value, "The extracted views snippet is incorrect");

            // Question context
            Assert.AreNotEqual(null, question["hints"], "The extracted question should have hints");
            Assert.AreEqual(4, question["hints"].Count, "The extracted question should have 4 hints");
            Assert.AreEqual("PC", question["hints"][3].ToString(), "The 4th hint of the extracted question should be PC");

            // Answers
            Assert.AreNotEqual(null, parsedJson["answers"], "Extractor should find answers in the HTML file");
            Assert.AreEqual(2, parsedJson["answers"].Count, "Extractor should find two answers in the thread summary of the HTML file");

            var secondAnswer = parsedJson["answers"][1];
            Assert.AreEqual("Most Helpful Reply", secondAnswer["type"].Value, "The extracted type of the answer is incorrect");
            Assert.AreNotEqual(null, secondAnswer["content"], "The content array in the extracted answer should not be null");
            Assert.IsTrue(secondAnswer["content"].Count > 0, "The content array in the extracted answer should have one or more items");
            Assert.AreEqual(4, secondAnswer["lists"].Count, "The lists array should have 4 items");
            Assert.IsTrue(secondAnswer["lists"][0]["items"].Count > 0, "First item in the lists array should have at least one item");

            // Check is textAboveLength exists in each list
            foreach (var answer in parsedJson["answers"])
            {
                var lists = answer["lists"];

                if (lists != null)
                {
                    foreach (var list in lists)
                    {
                        Assert.AreEqual(JTokenType.Integer, list["textAboveLength"].Type, "The extracted textAboveLength should be an integer");
                        var textAboveLength = ((JValue)list["textAboveLength"]).ToObject<int>();
                        Assert.IsTrue(textAboveLength > 0, string.Format(CultureInfo.InvariantCulture, "textAboveLength was not greater than 0. The extracted value is: {0}", textAboveLength));
                    }
                }
            }
        }

        [TestMethod]
        public void StackExchangeEx1ExtractionTest()
        {
            var configPath = Path.Combine("TestData", "stackexchange.com.json");
            var config = StructuredDataConfig.ParseJsonFile(configPath);
            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(File.ReadAllText(Path.Combine("TestData", "stackoverflow.com.example1.html")));
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);

            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            // Question
            Assert.AreNotEqual(null, parsedJson["question"], "Extractor should find a question in the HTML file");

            var question = parsedJson["question"];
            Assert.AreEqual("Is there a way to crack the password on an Excel VBA Project?", question["title"].Value, "The extracted title is incorrect");
            Assert.AreNotEqual(null, question["content"], "The extracted question should have a content");
            Assert.AreEqual(JTokenType.String, question["content"].Type, "The extracted question content should be a string");
            Assert.IsTrue(question["content"].Value.Length > 0, "The extracted question content should have a length > 0");
            Assert.AreNotEqual(null, question["votes"], "The extracted question should have a votes field");
            Assert.AreEqual(JTokenType.Integer, question["votes"].Type, "The votes extracted from the question should be of type int");
            Assert.AreEqual(196, question["votes"].Value, "The votes extracted from the question should have a value of 196");

            // Question context
            Assert.AreNotEqual(null, question["hints"], "The extracted question should have hints");
            Assert.AreEqual(4, question["hints"].Count, "The extracted question should have 4 hints");
            Assert.AreEqual("passwords", question["hints"][3].ToString(), "The 4th hint of the extracted question should be passwords");

            // Best Answer
            var bestAnswer = parsedJson["bestAnswer"];
            Assert.AreNotEqual(null, bestAnswer["content"], "The extracted answer should have a content");
            Assert.AreEqual(JTokenType.String, bestAnswer["content"].Type, "The extracted answer content should be a string");
            Assert.IsTrue(bestAnswer["content"].Value.Length > 0, "The extracted answer content should have a length > 0");
            Assert.AreNotEqual(null, bestAnswer["votes"], "The extracted answer should have a votes field");
            Assert.AreEqual(JTokenType.Integer, bestAnswer["votes"].Type, "The votes extracted from the answer should be of type int");
            Assert.AreEqual(153, bestAnswer["votes"].Value, "The votes extracted from the answer should have a value of 153");
            Assert.AreEqual(1, bestAnswer["lists"].Count, "The lists array should have 1 item");
            Assert.AreEqual(8, bestAnswer["lists"][0]["items"].Count, "The first item in the lists array should have 4 items");

            // Check is textAboveLength exists in each list
            var lists = bestAnswer["lists"];

            if (lists != null)
            {
                foreach (var list in lists)
                {
                    Assert.AreEqual(JTokenType.Integer, list["textAboveLength"].Type, "The extracted textAboveLength should be an integer");
                    var textAboveLength = ((JValue)list["textAboveLength"]).ToObject<int>();
                    Assert.IsTrue(textAboveLength > 0, string.Format(CultureInfo.InvariantCulture, "textAboveLength was not greater than 0. The extracted value is: {0}", textAboveLength));
                }
            }
        }

        [TestMethod]
        public void StackExchangeEx2ExtractionTest()
        {
            var configPath = Path.Combine("TestData", "stackexchange.com.json");
            var config = StructuredDataConfig.ParseJsonFile(configPath);
            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(File.ReadAllText(Path.Combine("TestData", "stackoverflow.com.example2.html")));
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);

            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            // Question
            Assert.AreNotEqual(null, parsedJson["question"], "Extractor should find a question in the HTML file");

            var question = parsedJson["question"];
            Assert.AreEqual("How to configure Visual Studio 2008 to use IIS Express?", question["title"].Value, "The extracted title is incorrect");
            Assert.AreNotEqual(null, question["content"], "The extracted question should have a content");
            Assert.AreEqual(JTokenType.String, question["content"].Type, "The extracted question content should be a string");
            Assert.IsTrue(question["content"].Value.Length > 0, "The extracted question content should have a length > 0");
            Assert.AreNotEqual(null, question["votes"], "The extracted question should have a votes field");
            Assert.AreEqual(JTokenType.Integer, question["votes"].Type, "The votes extracted from the question should be of type int");
            Assert.AreEqual(9, question["votes"].Value, "The votes extracted from the question should have a value of 9");

            // Question context
            Assert.AreNotEqual(null, question["hints"], "The extracted question should have hints");
            Assert.AreEqual(2, question["hints"].Count, "The extracted question should have 2 hints");
            Assert.AreEqual("iis-express", question["hints"][1].ToString(), "The 2nd hint of the extracted question should be passwords");

            // Best Answer
            var bestAnswer = parsedJson["bestAnswer"];
            Assert.AreNotEqual(null, bestAnswer["content"], "The extracted answer should have a content");
            Assert.AreEqual(JTokenType.String, bestAnswer["content"].Type, "The extracted answer content should be a string");
            Assert.IsTrue(bestAnswer["content"].Value.Length > 0, "The extracted answer content should have a length > 0");
            Assert.AreNotEqual(null, bestAnswer["votes"], "The extracted answer should have a votes field");
            Assert.AreEqual(JTokenType.Integer, bestAnswer["votes"].Type, "The votes extracted from the answer should be of type int");
            Assert.AreEqual(17, bestAnswer["votes"].Value, "The votes extracted from the answer should have a value of 17");
            Assert.AreEqual(1, bestAnswer["lists"].Count, "The lists array should have 1 item");
            Assert.AreEqual(7, bestAnswer["lists"][0]["items"].Count, "The first item in the lists array should have 7 items");

            // Check is textAboveLength exists in each list
            var lists = bestAnswer["lists"];

            if (lists != null)
            {
                foreach (var list in lists)
                {
                    Assert.AreEqual(JTokenType.Integer, list["textAboveLength"].Type, "The extracted textAboveLength should be an integer");
                    var textAboveLength = ((JValue)list["textAboveLength"]).ToObject<int>();
                    Assert.IsTrue(textAboveLength > 0, string.Format(CultureInfo.InvariantCulture, "textAboveLength was not greater than 0. The extracted value is: {0}", textAboveLength));
                }
            }
        }

        // Tests JSON config rules which are singular and plural (_xpath vs _xpaths, _transformation vs _transformations, etc.)
        [TestMethod]
        public void StackExchangeEx1SingularPluralExtractionTest()
        {
            var configPath = Path.Combine("TestData", "stackexchange_plural_singular_rules.json");
            var config = StructuredDataConfig.ParseJsonFile(configPath);
            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(File.ReadAllText(Path.Combine("TestData", "stackoverflow.com.example1.html")));
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);

            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            // Question
            Assert.AreNotEqual(null, parsedJson["question"], "Extractor should find a question in the HTML file");

            var question = parsedJson["question"];
            Assert.AreEqual("Is there a way to crack the password on an Excel VBA Project?", question["title"].Value, "The extracted title is incorrect");
            Assert.AreNotEqual(null, question["content"], "The extracted question should have a content");
            Assert.AreEqual(JTokenType.String, question["content"].Type, "The extracted question content should be a string");
            Assert.IsTrue(question["content"].Value.Length > 0, "The extracted question content should have a length > 0");
            Assert.AreNotEqual(null, question["votes"], "The extracted question should have a votes field");
            Assert.AreEqual(JTokenType.Integer, question["votes"].Type, "The votes extracted from the question should be of type int");
            Assert.AreEqual(196, question["votes"].Value, "The votes extracted from the question should have a value of 196");

            // Question context
            Assert.AreNotEqual(null, question["hints"], "The extracted question should have hints");
            Assert.AreEqual(4, question["hints"].Count, "The extracted question should have 4 hints");
            Assert.AreEqual("passwords", question["hints"][3].ToString(), "The 4th hint of the extracted question should be passwords");

            // Best Answer
            var bestAnswer = parsedJson["bestAnswer"];
            Assert.AreNotEqual(null, bestAnswer["content"], "The extracted answer should have a content");
            Assert.AreEqual(JTokenType.String, bestAnswer["content"].Type, "The extracted answer content should be a string");
            Assert.IsTrue(bestAnswer["content"].Value.Length > 0, "The extracted answer content should have a length > 0");
            Assert.AreNotEqual(null, bestAnswer["votes"], "The extracted answer should have a votes field");
            Assert.AreEqual(JTokenType.Integer, bestAnswer["votes"].Type, "The votes extracted from the answer should be of type int");
            Assert.AreEqual(153, bestAnswer["votes"].Value, "The votes extracted from the answer should have a value of 153");
            Assert.AreEqual(1, bestAnswer["lists"].Count, "The lists array should have 1 item");
            Assert.AreEqual(8, bestAnswer["lists"][0]["items"].Count, "The first item in the lists array should have 4 items");

            // Check is textAboveLength exists in each list
            var lists = bestAnswer["lists"];

            if (lists != null)
            {
                foreach (var list in lists)
                {
                    Assert.AreEqual(JTokenType.Integer, list["textAboveLength"].Type, "The extracted textAboveLength should be an integer");
                    var textAboveLength = ((JValue)list["textAboveLength"]).ToObject<int>();
                    Assert.IsTrue(textAboveLength > 0, string.Format(CultureInfo.InvariantCulture, "textAboveLength was not greater than 0. The extracted value is: {0}", textAboveLength));
                }
            }
        }

        [TestMethod]
        public void QuoraExtractionTest()
        {
            var configPath = Path.Combine("TestData", "quora.com.json");
            var config = StructuredDataConfig.ParseJsonFile(configPath);
            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(File.ReadAllText(Path.Combine("TestData", "quora.com.html")));
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);

            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            // Question
            Assert.AreNotEqual(null, parsedJson["question"], "Extractor should find a question in the HTML file");

            var question = parsedJson["question"];
            Assert.AreEqual("What are some tips for creating a successful Kickstarter project?", question["title"].Value, "The extracted title is incorrect");

            // Question context
            Assert.AreNotEqual(null, question["hints"], "The extracted question should have hints");
            Assert.AreEqual(5, question["hints"].Count, "The extracted question should have 5 hints");
            Assert.AreEqual("Kickstarter", question["hints"][3].ToString(), "The 4th hint of the extracted question should be Kickstarter");

            // Answers
            Assert.AreNotEqual(null, parsedJson["answers"], "Extractor should find answers in the HTML file");
            Assert.AreEqual(5, parsedJson["answers"].Count, "Extractor should find 5 answers in the thread summary of the HTML file");

            var firstAnswer = parsedJson["answers"][0];
            Assert.AreNotEqual(null, firstAnswer["content"], "The content string should not be null in the extracted answer");
            Assert.IsTrue(firstAnswer["content"].Value.Length > 0, "The content string should not be empty in the extracted answer");
            Assert.AreEqual(6800, firstAnswer["views"].Value, "The extracted views count is incorrect");
            Assert.AreEqual(1, firstAnswer["lists"].Count, "The lists array should have 1 item");
            Assert.IsTrue(firstAnswer["lists"][0]["items"].Count > 0, "First item in the lists array should have at least one item");

            var secondAnswer = parsedJson["answers"][1];
            Assert.AreEqual(2, secondAnswer["views"].Value, "The extracted views count is incorrect");

            // Check is textAboveLength exists in each list
            foreach (var answer in parsedJson["answers"])
            {
                var lists = answer["lists"];

                if (lists != null)
                {
                    foreach (var list in lists)
                    {
                        Assert.AreEqual(JTokenType.Integer, list["textAboveLength"].Type, "The extracted textAboveLength should be an integer");
                        var textAboveLength = ((JValue)list["textAboveLength"]).ToObject<int>();
                        Assert.IsTrue(textAboveLength > 0, string.Format(CultureInfo.InvariantCulture, "textAboveLength was not greater than 0. The extracted value is: {0}", textAboveLength));
                    }
                }
            }
        }

        [TestMethod]
        public void QuoraWithWikiExtractionTest()
        {
            var configPath = Path.Combine("TestData", "quora.com.json");
            var config = StructuredDataConfig.ParseJsonFile(configPath);
            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(File.ReadAllText(Path.Combine("TestData", "quora.com.withwiki.html")));
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);

            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            // Question
            Assert.AreNotEqual(null, parsedJson["question"], "Extractor should find a question in the HTML file");

            var question = parsedJson["question"];
            Assert.AreEqual("What can I learn/know right now in 10 minutes that will be useful for the rest of my life?", question["title"].Value, "The extracted title is incorrect");

            // Answers
            Assert.AreNotEqual(null, parsedJson["answers"], "Extractor should find answers in the HTML file");
            Assert.AreEqual(5, parsedJson["answers"].Count, "Extractor should find 5 answers in the thread summary of the HTML file");

            // Best Answer
            Assert.AreNotEqual(null, parsedJson["bestAnswer"], "Extractor should find the best answer in the HTML file");

            var bestAnswer = parsedJson["bestAnswer"];
            Assert.AreNotEqual(null, bestAnswer["content"], "The content string should not be null in the extracted answer");
            Assert.IsTrue(bestAnswer["content"].Value.Length > 0, "The content string should not be empty in the extracted answer");
            Assert.AreEqual(9, bestAnswer["lists"].Count, "The lists array should have 9 items");
            Assert.AreEqual(25, bestAnswer["lists"][1]["items"].Count, "Second item in the lists array should have 25 items");

            // Check is textAboveLength exists in each list
            foreach (var answer in parsedJson["answers"])
            {
                var lists = answer["lists"];

                if (lists != null)
                {
                    foreach (var list in lists)
                    {
                        Assert.AreEqual(JTokenType.Integer, list["textAboveLength"].Type, "The extracted textAboveLength should be an integer");
                        var textAboveLength = ((JValue)list["textAboveLength"]).ToObject<int>();
                        Assert.IsTrue(textAboveLength > 0, string.Format(CultureInfo.InvariantCulture, "textAboveLength was not greater than 0. The extracted value is: {0}", textAboveLength));
                    }
                }
            }

            var bestAnswerLists = bestAnswer["lists"];

            if (bestAnswerLists != null)
            {
                foreach (var list in bestAnswerLists)
                {
                    Assert.AreEqual(JTokenType.Integer, list["textAboveLength"].Type, "The extracted textAboveLength should be an integer");
                    var textAboveLength = ((JValue)list["textAboveLength"]).ToObject<int>();
                    Assert.IsTrue(textAboveLength > 0, string.Format(CultureInfo.InvariantCulture, "textAboveLength was not greater than 0. The extracted value is: {0}", textAboveLength));
                }
            }
        }

        [TestMethod]
        public void RemoveXPathsExtractionTest()
        {
            var configPath = Path.Combine("TestData", "article_with_comments_div.json");
            var config = StructuredDataConfig.ParseJsonFile(configPath);
            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(File.ReadAllText(Path.Combine("TestData", "article_with_comments_div.html")));
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);

            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            Assert.AreEqual("Article  title", parsedJson["title"].Value, "The extracted title is incorrect");
            Assert.AreEqual("Para1 content Para2 content", parsedJson["body"].Value, "The extracted body is incorrect");

        }

        [TestMethod]
        public void RegexTest()
        {
            var configPath = Path.Combine("TestData", "regex_rules.json");
            var config = StructuredDataConfig.ParseJsonFile(configPath);
            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(File.ReadAllText(Path.Combine("TestData", "article_with_date.html")));
            var actualJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            var parsedActualJson = JObject.Parse(actualJson);

            var expectedJsonPath = Path.Combine("TestData", "regex_expected_result.json");
            var expectedJson = File.ReadAllText(expectedJsonPath);
            var parsedExpectedJson = JObject.Parse(expectedJson);

            Assert.IsTrue(JToken.DeepEquals(parsedActualJson, parsedExpectedJson));
        }

        [TestMethod]
        public void ParseDateTest()
        {
            var configPath = Path.Combine("TestData", "parse_date_rules.json");
            var config = StructuredDataConfig.ParseJsonFile(configPath);
            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(File.ReadAllText(Path.Combine("TestData", "article_with_date.html")));
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            Assert.AreEqual(DateTime.Parse("2018-11-24T00:00:00"), parsedJson["parsedDateNoFormat"].Value);
            Assert.AreEqual(DateTime.Parse("2011-12-30T00:00:00"), parsedJson["parsedDateWithFormat"].Value);
            Assert.AreEqual(DateTime.Parse("2008-06-12T00:00:00"), parsedJson["parsedDateNoFormatWithProviderStyle"].Value);
        }

        [TestMethod]
        public void CastToIntegerTest()
        {
            var html = "<meta property=\"width\" content=\"1200\">";

            var configJson = @"
            {
                'width': {
                    '_xpath': '/meta[@property=\'width\']/@content',
                    '_transformation': 'CastToIntegerTransformation'
                }
            }
            ";

            var config = StructuredDataConfig.ParseJsonString(configJson);

            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(html);
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            Assert.AreEqual(1200, parsedJson["width"].Value);
        }

        [TestMethod]
        public void HtmlDecodeTest()
        {
            var html = "<html><body><div id='content'>&lt;a href=''&gt;A link&lt;/a&gt;.</div></body></html>";

            var configJson = @"
            {
                'text': {
                    '_xpath': '//div[@id=\'content\']',
                    '_transformation': 'HtmlDecodeTransformation'
                }
            }
            ";

            var config = StructuredDataConfig.ParseJsonString(configJson);

            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(html);
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            Assert.AreEqual("<a href=''>A link</a>.", parsedJson["text"].Value);
        }

        [TestMethod]
        public void HtmlEncodeTest()
        {
            var html = "<html><body><div id='content'>a < b</div></body></html>";

            var configJson = @"
            {
                'text': {
                    '_xpath': '//div[@id=\'content\']',
                    '_transformation': 'HtmlEncodeTransformation'
                }
            }
            ";

            var config = StructuredDataConfig.ParseJsonString(configJson);

            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(html);
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            Assert.AreEqual("a &lt; b", parsedJson["text"].Value);
        }

        [TestMethod]
        public void UrlDecodeTest()
        {
            var html = "<html><body><div id='content'><a href='https://www.bing.com/search?q=hello+world'></a></div></body></html>";

            var configJson = @"
            {
                'text': {
                    '_xpath': '//div[@id=\'content\']/a/@href',
                    '_transformation': 'UrlDecodeTransformation'
                }
            }
            ";

            var config = StructuredDataConfig.ParseJsonString(configJson);

            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(html);
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            Assert.AreEqual("https://www.bing.com/search?q=hello world", parsedJson["text"].Value);
        }

        [TestMethod]
        public void UrlEncodeTest()
        {
            var html = "<html><body><div id='content'><a href='hello world'></a></div></body></html>";

            var configJson = @"
            {
                'text': {
                    '_xpath': '//div[@id=\'content\']/a/@href',
                    '_transformation': 'UrlEncodeTransformation'
                }
            }
            ";

            var config = StructuredDataConfig.ParseJsonString(configJson);

            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(html);
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            Assert.AreEqual("hello+world", parsedJson["text"].Value);
        }

        [TestMethod]
        public void ExtractTextTest()
        {
            var html = "<html><body><div id='content'><a href=''>A link</a>with adjacent text.</div></body></html>";

            var configJson = @"
            {
                'text': {
                    '_xpath': '//div[@id=\'content\']',
                    '_transformation': 'ExtractTextTransformation'
                }
            }
            ";

            var config = StructuredDataConfig.ParseJsonString(configJson);

            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(html);
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            Assert.AreEqual("A link with adjacent text.", parsedJson["text"].Value);
        }

        [TestMethod]
        public void RemoveExtraWhitespaceTransformationTest()
        {
            var html = "<html><body><div id='content'><a href=''>A link</a>with     adjacent text. &quot;the final frontier&quot;</div></body></html>";

            var configJson = @"
            {
                'text': {
                    '_xpath': '//div[@id=\'content\']',
                    '_transformations': [
                        'ExtractTextTransformation',
                        'HtmlDecodeTransformation',
                        'RemoveExtraWhitespaceTransformation'
                    ]
                }
            }
            ";

            var config = StructuredDataConfig.ParseJsonString(configJson);

            var extractor = new StructuredDataExtractor(config);
            var result = extractor.Extract(html);
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            dynamic parsedJson = JsonConvert.DeserializeObject(json);

            Assert.AreEqual("A link with adjacent text. \"the final frontier\"", parsedJson["text"].Value);
        }
    }
}
