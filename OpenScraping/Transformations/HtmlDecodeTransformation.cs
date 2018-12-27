// -----------------------------------------------------------------------
// <copyright file="HtmlDecodeTransformation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using HtmlAgilityPack;
    using System.Collections.Generic;
    using System.Net;

    public class HtmlDecodeTransformation : ITransformationFromObject, ITransformationFromHtml
    {
        public object Transform(Dictionary<string, object> settings, object input)
        {
            if (input != null && input is string)
            {
                var text = (string)input;
                return WebUtility.HtmlDecode(text);
            }

            return null;
        }

        public object Transform(Dictionary<string, object> settings, HtmlNodeNavigator nodeNavigator, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            var text = nodeNavigator?.Value ?? nodeNavigator?.CurrentNode?.InnerText;

            if (text != null)
            {
                return WebUtility.HtmlDecode(text);
            }

            return null;
        }
    }
}
