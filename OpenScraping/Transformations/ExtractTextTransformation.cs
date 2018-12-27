// -----------------------------------------------------------------------
// <copyright file="ExtractTextTransformation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using HtmlAgilityPack;
    using System.Collections.Generic;
    using System.Text;

    public class ExtractTextTransformation : ITransformationFromHtml
    {
        public object Transform(Dictionary<string, object> settings, HtmlNodeNavigator nodeNavigator, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            var ret = new StringBuilder();

            if (nodeNavigator?.CurrentNode != null)
            {
                foreach (HtmlNode node in nodeNavigator.CurrentNode.SelectNodes(".//text()"))
                {
                    var nodeInnerText = node.InnerText;

                    if (nodeInnerText != null)
                    {
                        if (ret.Length > 0)
                        {
                            ret.Append(" ");
                        }

                        ret.Append(nodeInnerText);
                    }
                }
            }

            if (ret.Length == 0)
            {
                var navVal = nodeNavigator?.Value;

                if (navVal != null)
                {
                    return navVal;
                }
            }

            return ret.ToString();
        }
    }
}
