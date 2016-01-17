// -----------------------------------------------------------------------
// <copyright file="MultiExtractor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using OpenScraping.Config;

    public class MultiExtractor
    {
        private List<Tuple<ConfigSection, StructuredDataExtractor>> configsToExtractors = new List<Tuple<ConfigSection, StructuredDataExtractor>>();

        public MultiExtractor(string configRootFolder, string configFilesPattern)
        {
            var files = Directory.GetFiles(configRootFolder, configFilesPattern);
            var regexRules = 0; // used to configure the C# regex cache size

            if (files != null && files.Length > 0)
            {
                foreach (var file in files)
                {
                    var config = StructuredDataConfig.ParseJsonFile(file);

                    if (config.UrlPatterns != null && config.UrlPatterns.Count > 0)
                    {
                        regexRules += config.UrlPatterns.Count;
                        var extractor = new StructuredDataExtractor(config);
                        this.configsToExtractors.Add(Tuple.Create<ConfigSection, StructuredDataExtractor>(config, extractor));
                    }
                }
            }

            // The default is 15
            if (regexRules > 15)
            {
                Regex.CacheSize = regexRules;
            }
        }

        public StructuredDataExtractor FindFirstExtractor(string url)
        {
            foreach (var tuple in this.configsToExtractors)
            {
                var config = tuple.Item1;

                if (config.UrlPatterns != null && config.UrlPatterns.Count > 0)
                {
                    foreach (var rule in config.UrlPatterns)
                    {
                        if (Regex.IsMatch(url, rule))
                        {
                            var extractor = tuple.Item2;
                            return extractor;
                        }
                    }
                }
            }

            return null;
        }

        public string ParsePage(string url, string html)
        {
            var extractor = this.FindFirstExtractor(url);

            if (extractor != null)
            {
                var structuredJson = extractor.Extract(html);
                var serializedJson = JsonConvert.SerializeObject(structuredJson, Formatting.Indented);

                if (serializedJson != null)
                {
                    return serializedJson;
                }
            }

            return null;
        }
    }
}