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

    public class RemoveExtraWhitespaceTransformation : ITransformationFromObject, ITransformationFromHtml
    {
        private Regex ExtraWhitespacesRegex = new Regex(@"\s\s+", RegexOptions.Compiled);

        public object Transform(Dictionary<string, object> settings, object input)
        {
            if (input != null && input is string)
            {
                var text = (string)input;
                text = ExtraWhitespacesRegex.Replace(text, " ");
                return text;
            }

            return null;
        }

        public object Transform(Dictionary<string, object> settings, HtmlNodeNavigator nodeNavigator, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            var node = nodeNavigator?.CurrentNode;

            if (node != null)
            {
                var text = node.InnerText;

                if (text != null)
                {
                    text = HtmlEntity.DeEntitize(text).Trim();
                    text = ExtraWhitespacesRegex.Replace(text, " ");
                    return text;
                }
            }

            return null;
        }
    }
}
