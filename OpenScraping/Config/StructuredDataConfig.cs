// -----------------------------------------------------------------------
// <copyright file="StructuredDataConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Config
{
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class StructuredDataConfig
    {
        public static ConfigSection ParseJsonFile(string configPath)
        {
            var json = File.ReadAllText(configPath, Encoding.UTF8);
            return ParseJsonString(json);
        }

        public static ConfigSection ParseJsonString(string json)
        {
            var parsedJson = (JObject)JsonConvert.DeserializeObject(json);

            var rootSection = new ConfigSection();
            ParseSection(parsedJson, rootSection);
            return rootSection;
        }

        private static void ParseSection(JObject parsedJson, ConfigSection currentConfig)
        {
            foreach (var item in parsedJson)
            {
                switch (item.Key)
                {
                    case "_configName":
                        // Process the friendly internal name we have to this config
                        ProcessConfigName(parsedJson, currentConfig);
                        break;
                    case "_urlPatterns":
                        // Process configuration on URL patterns that should be used to
                        // determine which HTML pages to process with the configuration
                        ProcessUrlPatterns(parsedJson, currentConfig);
                        break;
                    case "_removeTags":
                        // Process the list of descendent tags to remove before extracting any data from this node
                        ProcessRemoveTags(parsedJson, currentConfig);
                        break;
                    case "_xpath":
                        // Process configuration on XPath rules used to extract this block
                        // from HTML
                        ProcessXPathRules(parsedJson, currentConfig);
                        break;
                    case "_transformations": // one or more transformations
                        // Process configuration on how to clean/modify the values extracted 
                        // from HTML
                        ProcessTransformations(parsedJson, currentConfig);
                        break;
                    case "_transformation": // single transformation
                        // Process configuration on how to clean/modify the values extracted 
                        // from HTML
                        ProcessTransformation(parsedJson, currentConfig);
                        break;
                    case "_forceArray":
                        // Force the value of this item to be an array (true or false)
                        ProcessForceArray(parsedJson, currentConfig);
                        break;
                    default:
                        // We assume all other keys in the JSON at this level are for actual HTML 
                        // sections that need to be extracted, not configuration settings
                        ProcessChild(item.Key, item.Value, parsedJson, currentConfig);
                        break;
                }
            }
        }

        private static void ProcessConfigName(JObject parsedJson, ConfigSection currentConfig)
        {
            var configNameKeyName = "_configName";

            if (parsedJson[configNameKeyName] != null && parsedJson[configNameKeyName].Type == JTokenType.String)
            {
                currentConfig.ConfigName = parsedJson[configNameKeyName].ToString();
            }
        }

        private static void ProcessUrlPatterns(JObject parsedJson, ConfigSection currentConfig)
        {
            var urlPatternsKeyName = "_urlPatterns";

            if (parsedJson[urlPatternsKeyName] != null && parsedJson[urlPatternsKeyName].Type == JTokenType.Array)
            {
                foreach (var urlPattern in parsedJson[urlPatternsKeyName])
                {
                    currentConfig.UrlPatterns.Add(urlPattern.ToString());
                }
            }
        }

        private static void ProcessRemoveTags(JObject parsedJson, ConfigSection currentConfig)
        {
            var removeTagsKeyName = "_removeTags";

            if (parsedJson[removeTagsKeyName] != null && parsedJson[removeTagsKeyName].Type == JTokenType.Array)
            {
                foreach (var removeTagPattern in parsedJson[removeTagsKeyName])
                {
                    currentConfig.RemoveTags.Add(removeTagPattern.ToString().ToLowerInvariant());
                }
            }
        }

        private static void ProcessXPathRules(JObject parsedJson, ConfigSection currentConfig)
        {
            var xPathKeyName = "_xpath";

            if (parsedJson[xPathKeyName] != null)
            {
                if (parsedJson[xPathKeyName].Type == JTokenType.Array)
                {
                    foreach (var xPath in parsedJson[xPathKeyName])
                    {
                        currentConfig.XPathRules.Add(xPath.ToString());
                    }
                }
                else if (parsedJson[xPathKeyName].Type == JTokenType.String)
                {
                    currentConfig.XPathRules.Add(parsedJson[xPathKeyName].ToString());
                }
            }
        }

        private static void ProcessTransformations(JObject parsedJson, ConfigSection currentConfig)
        {
            var transformationsKeyName = "_transformations";

            if (parsedJson[transformationsKeyName] != null && parsedJson[transformationsKeyName].Type == JTokenType.Array)
            {
                var transformations = parsedJson[transformationsKeyName];

                foreach (var transformation in transformations)
                {
                    if (transformation.Type == JTokenType.Object)
                    {
                        ProcessTransformation(currentConfig, transformation);
                    }
                    else if (transformation.Type == JTokenType.String)
                    {
                        ProcessTransformation(currentConfig, TransformationConfigFromName(transformation.ToString()));
                    }
                }
            }
        }

        private static void ProcessTransformation(JObject parsedJson, ConfigSection currentConfig)
        {
            var transformationKeyName = "_transformation";

            if (parsedJson[transformationKeyName] != null && parsedJson[transformationKeyName].Type == JTokenType.String)
            {
                ProcessTransformation(currentConfig, TransformationConfigFromName(parsedJson[transformationKeyName].ToString()));
            }
        }

        private static JToken TransformationConfigFromName(string name)
        {
            var config = new JObject();
            config.Add(new JProperty("_type", name));
            return config;
        }

        private static void ProcessTransformation(ConfigSection currentConfig, JToken transformation)
        {
            var transformationConfig = new TransformationConfig();

            foreach (var item in transformation)
            {
                if (item.Type == JTokenType.Property)
                {
                    var property = (JProperty)item;
                    var propertyName = property.Name.ToString();
                    var propertyValue = property.Value;

                    switch (propertyName)
                    {
                        case "_type":
                            transformationConfig.Type = propertyValue.ToString();
                            break;
                        default:
                            transformationConfig.ConfigAttributes[propertyName] = propertyValue;
                            break;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(transformationConfig.Type))
            {
                currentConfig.Transformations.Add(transformationConfig);
            }
        }

        private static void ProcessForceArray(JObject parsedJson, ConfigSection currentConfig)
        {
            var forceArrayKeyName = "_forceArray";

            if (parsedJson[forceArrayKeyName] != null)
            {
                if (parsedJson[forceArrayKeyName].Type == JTokenType.Boolean)
                {
                    currentConfig.ForceArray = ((JValue)parsedJson[forceArrayKeyName]).ToObject<bool>();
                }
            }
        }

        private static void ProcessChild(string childName, JToken rawChild, JObject parsedJson, ConfigSection currentConfig)
        {
            if (rawChild.Type == JTokenType.Object)
            {
                var childSection = new ConfigSection();
                ParseSection(rawChild.Value<JObject>(), childSection);
                currentConfig.Children[childName] = childSection;
            }
            else if (rawChild.Type == JTokenType.String)
            {
                var childSection = new ConfigSection();
                childSection.XPathRules.Add(rawChild.ToString());

                currentConfig.Children[childName] = childSection;
            }
        }
    }
}
