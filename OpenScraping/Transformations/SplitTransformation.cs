// -----------------------------------------------------------------------
// <copyright file="SplitTransformation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using System;
    using System.Collections.Generic;
    using HtmlAgilityPack;
    using Newtonsoft.Json.Linq;

    public class SplitTransformation : ITransformationFromHtml
    {
        public object Transform(Dictionary<string, object> settings, HtmlAgilityPack.HtmlNode node, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            var separator = ",";
            var trim = false;

            if (node != null)
            {
                var text = node.InnerText;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (settings != null && settings.ContainsKey("_separator") && ((JValue)settings["_separator"]).Type == JTokenType.String)
                    {
                        separator = settings["_separator"].ToString();
                    }

                    if (settings != null && settings.ContainsKey("_trim") && ((JValue)settings["_trim"]).Type == JTokenType.Boolean)
                    {
                        trim = (bool)((JValue)settings["_trim"]).Value;
                    }
                }

                try
                {
                    var textParts = text.Split(new string[] { separator }, StringSplitOptions.None);

                    if (trim)
                    {
                        for (var i = 0; i < textParts.Length; i++)
                        {
                            textParts[i] = HtmlEntity.DeEntitize(textParts[i]).Trim();
                        }
                    }

                    return new JArray(textParts);
                }
                catch (ArgumentException)
                {
                }
            }

            return null;
        }
    }
}
