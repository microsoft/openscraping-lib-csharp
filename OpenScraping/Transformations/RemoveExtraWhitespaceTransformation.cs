// -----------------------------------------------------------------------
// <copyright file="RemoveExtraWhitespaceTransformation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using HtmlAgilityPack;

    public class RemoveExtraWhitespaceTransformation : ITransformationFromHtml
    {
        public object Transform(Dictionary<string, object> settings, HtmlAgilityPack.HtmlNode node, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            if (node != null)
            {
                var text = node.InnerText;

                if (text != null)
                {
                    text = HtmlEntity.DeEntitize(text).Trim();
                    text = Regex.Replace(text, @"\s\s+", " ");
                    return text;
                }
            }

            return null;
        }
    }
}
