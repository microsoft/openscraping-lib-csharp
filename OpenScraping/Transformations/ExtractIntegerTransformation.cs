// -----------------------------------------------------------------------
// <copyright file="ExtractIntegerTransformation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using HtmlAgilityPack;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class ExtractIntegerTransformation : ITransformationFromHtml
    {
        public object Transform(Dictionary<string, object> settings, HtmlNodeNavigator nodeNavigator, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            var text = nodeNavigator?.Value ?? nodeNavigator?.CurrentNode?.InnerText;

            if (text != null)
            {
                var intStrMatch = Regex.Match(text, @"-?\d+");

                if (intStrMatch.Success && !string.IsNullOrEmpty(intStrMatch.Value))
                {
                    int intVal;

                    if (int.TryParse(intStrMatch.Value, out intVal))
                    {
                        return intVal;
                    }
                }
            }

            return null;
        }
    }
}
