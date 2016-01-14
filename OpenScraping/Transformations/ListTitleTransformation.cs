// -----------------------------------------------------------------------
// <copyright file="ListTitleTransformation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using System.Collections.Generic;
    using HtmlAgilityPack;
    using Newtonsoft.Json.Linq;

    public class ListTitleTransformation : ITransformationFromHtml
    {
        private static HashSet<string> allowedTags = new HashSet<string>()
        {
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "h7",
            "h8",
            "h9",
            "h10",
            "span",
            "div",
            "b",
            "em",
            "strong",
            "i",
            "p",
            "a"
        };

        public object Transform(Dictionary<string, object> settings, HtmlAgilityPack.HtmlNode node, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            HtmlAgilityPack.HtmlNode sibling = null;
            var level = 0;
            var maxLevel = 3;
            var maxTitleLength = 200;

            if (settings != null && settings["_maxStepsUpward"] != null && ((JValue)settings["_maxStepsUpward"]).Type == JTokenType.Integer)
            {
                maxLevel = ((JValue)settings["_maxStepsUpward"]).ToObject<int>();
            }

            if (settings != null && settings["_maxTitleLength"] != null && ((JValue)settings["_maxTitleLength"]).Type == JTokenType.Integer)
            {
                maxTitleLength = ((JValue)settings["_maxTitleLength"]).ToObject<int>();
            }

            do
            {
                level++;
                sibling = sibling != null ? sibling.PreviousSibling : node.PreviousSibling;

                if (sibling != null && this.IsAllowedTypeRecursive(sibling))
                {
                    var siblingInnerText = sibling.InnerText;

                    if (!string.IsNullOrWhiteSpace(siblingInnerText))
                    {
                        var text = HtmlEntity.DeEntitize(siblingInnerText).Trim();

                        if (text.Length <= maxTitleLength)
                        {
                            return text;
                        }
                        else if (text.Length > 0)
                        {
                            // We will stop if the first title candidate we find is not valid, but we will continue if the text was empty
                            return null;
                        }
                    }
                }

                // At this point we did not return any text, so the text node is empty, or this is a comment.
                // We will decrement level so we ignore this node completely
                if (sibling != null && (sibling.NodeType == HtmlAgilityPack.HtmlNodeType.Text || sibling.NodeType == HtmlAgilityPack.HtmlNodeType.Comment))
                {
                    level--;
                }
            }
            while (sibling != null && level < maxLevel);

            return null;
        }

        private bool IsAllowedTypeRecursive(HtmlAgilityPack.HtmlNode node)
        {
            if (node.NodeType == HtmlAgilityPack.HtmlNodeType.Text)
            {
                return true;
            }

            if (node.NodeType != HtmlAgilityPack.HtmlNodeType.Element)
            {
                return false;
            }

            if (!allowedTags.Contains(node.Name))
            {
                return false;
            }

            var children = node.ChildNodes;

            if (children != null)
            {
                foreach (var child in children)
                {
                    if (!this.IsAllowedTypeRecursive(child))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
