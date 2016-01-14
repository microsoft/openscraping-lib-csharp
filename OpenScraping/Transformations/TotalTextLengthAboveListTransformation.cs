// -----------------------------------------------------------------------
// <copyright file="TotalTextLengthAboveListTransformation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using System.Collections.Generic;
    using System.Text;
    using HtmlAgilityPack;
    using Newtonsoft.Json.Linq;

    public class TotalTextLengthAboveListTransformation : ITransformationFromHtml
    {
        public object Transform(Dictionary<string, object> settings, HtmlAgilityPack.HtmlNode node, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            var ret = new StringBuilder();
            var foundParent = false;

            HtmlAgilityPack.HtmlNode currentNode = node;

            if (logicalParents != null && logicalParents.Count >= 2)
            {
                // We will skip out immediate parent because that's the list, we need the parent of the list, which is out grandparent
                var grandParentNode = logicalParents[logicalParents.Count - 2];
                HtmlAgilityPack.HtmlNode parentNode = grandParentNode;

                if (settings != null && settings["_startingXPath"] != null && ((JValue)settings["_startingXPath"]).Type == JTokenType.String)
                {
                    var startingXPath = ((JValue)settings["_startingXPath"]).ToObject<string>();

                    var nodes = parentNode.SelectNodes(startingXPath);

                    if (nodes != null && nodes.Count > 0)
                    {
                        parentNode = nodes[0];
                    }
                    else
                    {
                        return 0;
                    }
                }

                while (currentNode != null && currentNode != parentNode && !foundParent)
                {
                    var siblingText = this.GetTextFromSiblings(currentNode, parentNode, ref foundParent);

                    if (!string.IsNullOrEmpty(siblingText))
                    {
                        ret.Append(siblingText);
                        ret.Append(" ");
                    }

                    currentNode = currentNode.ParentNode;
                }
            }

            var text = ret.ToString().Trim();

            return text.Length;
        }

        public string GetTextFromSiblings(HtmlAgilityPack.HtmlNode node, HtmlAgilityPack.HtmlNode parentNode, ref bool foundParent)
        {
            var ret = new StringBuilder();

            if (node != null)
            {
                HtmlAgilityPack.HtmlNode sibling = null;

                do
                {
                    sibling = sibling != null ? sibling.PreviousSibling : node.PreviousSibling;

                    if (sibling == parentNode)
                    {
                        foundParent = true;
                    }

                    if (sibling != null && sibling != parentNode)
                    {
                        var siblingInnerText = sibling.InnerText;

                        if (!string.IsNullOrWhiteSpace(siblingInnerText))
                        {
                            var text = HtmlEntity.DeEntitize(siblingInnerText).Trim();
                            ret.Append(text);
                            ret.Append(" ");
                        }
                    }
                }
                while (sibling != null);
            }

            return ret.ToString().Trim();
        }
    }
}
