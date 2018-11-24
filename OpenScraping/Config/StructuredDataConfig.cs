// -----------------------------------------------------------------------
// <copyright file="StructuredDataConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Config
{
    using System.Collections.Generic;
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
                    case "_urlPattern":
                    case "_urlPatterns":
                        // Process configuration on URL patterns that should be used to
                        // determine which HTML pages to process with the configuration
                        ProcessConfigKey(parsedJson, currentConfig.UrlPatterns, keyName: item.Key);
                        break;
                    case "_removeTag":
                    case "_removeTags":
                        // Process the list of descendent tags to remove before extracting any data from this node
                        ProcessConfigKey(parsedJson, currentConfig.RemoveTags, keyName: item.Key);
                        break;
                    case "_removeXPath":
                    case "_removeXPaths":
                        // Process the list of XPath rules to apply for deleting descendant nodes before extracting any data from this node
                        ProcessConfigKey(parsedJson, currentConfig.RemoveXPathRules, keyName: item.Key);
                        break;
                    case "_xpath":
                    case "_xpaths":
                        // Process configuration on XPath rules used to extract this block from HTML
                        ProcessConfigKey(parsedJson, currentConfig.XPathRules, keyName: item.Key);
                        break;
                    case "_transformation":
                    case "_transformations":
                        // Process configuration on how to clean/modify the values extracted from HTML
                        ProcessTransformations(parsedJson, currentConfig, keyName: item.Key);
                        break;
                    case "_forceArray":
                        // Force the value of this item to be an array (true or false)
                        ProcessForceArray(parsedJson, currentConfig, keyName: item.Key);
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
            var keyName = "_configName";

            if (parsedJson[keyName] != null && parsedJson[keyName].Type == JTokenType.String)
            {
                currentConfig.ConfigName = parsedJson[keyName].ToString();
            }
        }

        private static void ProcessConfigKey(JObject parsedJson, ICollection<string> configItems, string keyName)
        {
            if (parsedJson[keyName] != null)
            {
                if (parsedJson[keyName].Type == JTokenType.Array)
                {
                    foreach (var urlPattern in parsedJson[keyName])
                    {
                        configItems.Add(urlPattern.ToString());
                    }
                }
                else if (parsedJson[keyName].Type == JTokenType.String)
                {
                    configItems.Add(parsedJson[keyName].ToString());
                }
            }
        }

        private static void ProcessTransformations(JObject parsedJson, ConfigSection currentConfig, string keyName)
        {
            if (parsedJson[keyName] != null)
            {
                if (parsedJson[keyName].Type == JTokenType.Array)
                {
                    var transformations = parsedJson[keyName];

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
                else
                {
                    var transformation = parsedJson[keyName];
                    ProcessTransformation(currentConfig, TransformationConfigFromName(transformation.ToString()));
                }
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

        private static void ProcessForceArray(JObject parsedJson, ConfigSection currentConfig, string keyName)
        {
            if (parsedJson[keyName] != null)
            {
                if (parsedJson[keyName].Type == JTokenType.Boolean)
                {
                    currentConfig.ForceArray = ((JValue)parsedJson[keyName]).ToObject<bool>();
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
