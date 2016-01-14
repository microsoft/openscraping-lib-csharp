// -----------------------------------------------------------------------
// <copyright file="CastToIntegerTransformation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using System.Collections.Generic;

    public class CastToIntegerTransformation : ITransformationFromHtml
    {
        public object Transform(Dictionary<string, object> settings, HtmlAgilityPack.HtmlNode node, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            if (node != null)
            {
                var text = node.InnerText;

                if (text != null)
                {
                    int intVal;

                    if (int.TryParse(text, out intVal))
                    {
                        return intVal;
                    }
                }
            }

            return null;
        }
    }
}
