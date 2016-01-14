// -----------------------------------------------------------------------
// <copyright file="ExtractIntegerTransformation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class ExtractIntegerTransformation : ITransformationFromHtml
    {
        public object Transform(Dictionary<string, object> settings, HtmlAgilityPack.HtmlNode node, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            if (node != null)
            {
                var text = node.InnerText;

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
            }

            return null;
        }
    }
}
