// -----------------------------------------------------------------------
// <copyright file="ConfigSection.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Config
{
    using System.Collections.Generic;

    public sealed class ConfigSection
    {
        public ConfigSection()
        {
            this.ConfigName = string.Empty;
            this.RemoveTags = new HashSet<string>();
            this.UrlPatterns = new List<string>();
            this.RemoveXPathRules = new HashSet<string>();
            this.XPathRules = new List<string>();
            this.Transformations = new List<TransformationConfig>();
            this.Children = new Dictionary<string, ConfigSection>();
            this.ForceArray = false;
        }

        // Friendly internal name we assign to this config
        public string ConfigName { get; set; }

        // Only used in the root of the config tree
        public List<string> UrlPatterns { get; set; }

        // List of descendent tags to remove before extracting any data from this node
        public HashSet<string> RemoveTags { get; set; }

        // List of XPath rules to apply for deleting descendant nodes before extracting any data from this node
        public HashSet<string> RemoveXPathRules { get; set; }

        // The list of XPaths to use to find these kinds of items in the HTML
        public List<string> XPathRules { get; set; }

        // List of transformations to run one by one in order on the extracted content
        public List<TransformationConfig> Transformations { get; set; }

        // Children attributes to extract from the HTML of this parent HTML block
        public Dictionary<string, ConfigSection> Children { get; set; }

        // Force the value of this item to be an array
        public bool ForceArray { get; set; }
    }
}
