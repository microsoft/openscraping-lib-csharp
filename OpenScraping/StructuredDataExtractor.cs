// -----------------------------------------------------------------------
// <copyright file="StructuredDataExtractor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Xml.XPath;

namespace OpenScraping
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using HtmlAgilityPack;
    using Newtonsoft.Json.Linq;
    using OpenScraping.Transformations;
    using OpenScraping.Config;

    public class StructuredDataExtractor
    {
        private ConfigSection config;

        private Dictionary<string, ITransformationFromObject> transformationsFromContainer = new Dictionary<string, ITransformationFromObject>();

        private Dictionary<string, ITransformationFromHtml> transformationsFromHtml = new Dictionary<string, ITransformationFromHtml>();

        public StructuredDataExtractor(ConfigSection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            this.config = config;
            
            this.LoadTransformations();
        }

        public JContainer Extract(string html)
        {
            if (html == null)
            {
                throw new ArgumentNullException("html");
            }

            var document = new HtmlAgilityPack.HtmlDocument();

            // Fixed HtmlAgilityPack throws StackOverflowException on pages with lots of nested tags - https://code.google.com/p/abot/issues/detail?id=77
            document.OptionFixNestedTags = true;

            try
            {
                document.LoadHtml(html);
            }
            catch (Exception)
            {
                return null;
            }

            if (document.DocumentNode == null)
            {
                return null;
            }

            return (JContainer)this.Extract(name: "root", config: this.config, parentNode: document.DocumentNode, logicalParents: new List<HtmlAgilityPack.HtmlNode>());
        }

        private void LoadTransformations()
        {
            var transformationFromContainerType = typeof(ITransformationFromObject);
            var transformationFromHtmlType = typeof(ITransformationFromHtml);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes;

                // http://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes
                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    assemblyTypes = ex.Types;
                }

                if (assemblyTypes != null && assemblyTypes.Length > 0)
                {
                    var transformationFromContainers = assemblyTypes.Where(x => transformationFromContainerType.IsAssignableFrom(x)
                            && !x.IsInterface
                            && !x.IsAbstract);

                    foreach (var type in transformationFromContainers)
                    {
                        var transformationFromContainer = (ITransformationFromObject)Activator.CreateInstance(type);
                        this.transformationsFromContainer[type.Name] = transformationFromContainer;
                    }

                    var transformationFromHtmlTypes = assemblyTypes.Where(x => transformationFromHtmlType.IsAssignableFrom(x)
                            && !x.IsInterface
                            && !x.IsAbstract);

                    foreach (var type in transformationFromHtmlTypes)
                    {
                        var transformationFromHtml = (ITransformationFromHtml)Activator.CreateInstance(type);
                        this.transformationsFromHtml[type.Name] = transformationFromHtml;
                    }
                }
            }
        }

        private object Extract(string name, ConfigSection config, HtmlAgilityPack.HtmlNode parentNode, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            this.RemoveUnwantedTags(config, parentNode);

            // We will try to extract text for this item because it does not have children
            var containers = new JArray();

            if (config.XPathRules != null && config.XPathRules.Count > 0)
            {
                var navigator = parentNode.CreateNavigator();

                foreach (var xpath in config.XPathRules)
                {
                    // TODO: Add try catch Exception
                    var nodes = navigator.Select(xpath);

                    if (nodes != null && nodes.Count > 0)
                    {
                        var newLogicalParents = logicalParents.GetRange(0, logicalParents.Count);
                        newLogicalParents.Add(parentNode);

                        foreach (HtmlNodeNavigator nodeNavigator in nodes)
                        {
                            this.RemoveXPaths(config, nodeNavigator.CurrentNode);

                            if (config.Children != null && config.Children.Count > 0)
                            {
                                var container = new JObject();
                                this.ExtractChildren(config: config, parentNode: nodeNavigator.CurrentNode, container: container, logicalParents: newLogicalParents);
                                containers.Add(container);
                            }
                            else if (config.Transformations != null && config.Transformations.Count > 0)
                            {
                                var obj = this.RunTransformations(config.Transformations, nodeNavigator, newLogicalParents);

                                if (obj != null)
                                {
                                    containers.Add(obj);
                                }
                            }
                            else if (nodeNavigator.Value != null)
                            {
                                containers.Add(HtmlEntity.DeEntitize(nodeNavigator.Value).Trim());
                            }
                        }
                    }
                }
            }
            else
            {
                var container = new JObject();
                this.ExtractChildren(config: config, parentNode: parentNode, container: container, logicalParents: logicalParents);
                containers.Add(container);
            }

            if (!config.ForceArray && containers.Count == 0)
            {
                return new JObject();
            }
            else if (!config.ForceArray && containers.Count == 1)
            {
                return containers.First;
            }
            else
            {
                return containers;
            }
        }

        private void RemoveUnwantedTags(ConfigSection config, HtmlAgilityPack.HtmlNode parentNode)
        {
            if (parentNode != null && config != null && config.RemoveTags != null && config.RemoveTags.Count > 0)
            {
                parentNode.Descendants()
                        .Where(n => config.RemoveTags.Contains(n.Name.ToLowerInvariant()))
                        .ToList()
                        .ForEach(n => n.Remove());
            }
        }

        private void RemoveXPaths(ConfigSection config, HtmlAgilityPack.HtmlNode parentNode)
        {
            if (parentNode != null && config != null && config.RemoveXPathRules != null && config.RemoveXPathRules.Count > 0)
            {
                foreach (var removeXPathRule in config.RemoveXPathRules)
                {
                    var navigator = parentNode.CreateNavigator();
                    var nodes = navigator.Select(removeXPathRule);

                    foreach (HtmlNodeNavigator node in nodes)
                    {
                        node.CurrentNode.Remove();
                    }
                }
            }
        }

        private object RunTransformations(List<TransformationConfig> transformations, HtmlNodeNavigator nodeNavigator, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            object obj = null;

            foreach (var transformation in transformations)
            {
                var settings = transformation.ConfigAttributes;

                if (obj == null && this.transformationsFromHtml.ContainsKey(transformation.Type))
                {
                    obj = this.transformationsFromHtml[transformation.Type].Transform(settings, nodeNavigator, logicalParents);
                }
                else if (obj != null && this.transformationsFromContainer.ContainsKey(transformation.Type))
                {
                    obj = this.transformationsFromContainer[transformation.Type].Transform(settings, obj);
                }
                else
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Transformation chain broken at transformation type {0}", transformation.Type));
                }
            }

            return obj;
        }

        private void ExtractChildren(ConfigSection config, HtmlAgilityPack.HtmlNode parentNode, JObject container, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            foreach (var child in config.Children)
            {
                var childName = child.Key;
                var childConfig = child.Value;

                var childObject = this.Extract(name: childName, config: childConfig, parentNode: parentNode, logicalParents: logicalParents);

                if (childObject is JObject)
                {
                    if (((JObject)childObject).Count > 0)
                    {
                        container[childName] = (JToken)childObject;
                    }
                }
                else if (childObject is JArray)
                {
                    if (((JArray)childObject).Count > 0)
                    {
                        container[childName] = (JToken)childObject;
                    }
                }
                else
                {
                    container[childName] = (JToken)childObject;
                }
            }
        }
    }
}
