// -----------------------------------------------------------------------
// <copyright file="AbbreviatedIntegerTranformation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using System;
    using System.Collections.Generic;

    public class AbbreviatedIntegerTranformation : ITransformationFromHtml
    {
        public object Transform(Dictionary<string, object> settings, HtmlAgilityPack.HtmlNode node, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            if (node != null)
            {
                var text = node.InnerText;

                if (text != null)
                {
                    var parts = text.Split(new char[] { ' ' });

                    foreach (var part in parts)
                    {
                        var number = this.ConvertAbbreviatedNumber(part);

                        if (number.HasValue)
                        {
                            return number.Value;
                        }
                    }
                }
            }

            return null;
        }

        // Input: "6.8k views" (string), Output: 6800 (integer)
        private int? ConvertAbbreviatedNumber(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var index = 0;

            do
            {
                var currentChar = text[index];

                if (currentChar != '.' && currentChar < '0' && currentChar > '9')
                {
                    break;
                }

                index++;
            }
            while (index < text.Length - 1);

            if (index != 0)
            {
                var firstPart = text.Substring(0, index);

                double number;

                if (double.TryParse(firstPart, out number))
                {
                    if (firstPart.Length < text.Length)
                    {
                        var secondPart = text.Substring(index).ToLower();

                        switch (secondPart)
                        {
                            case "k":
                                return Convert.ToInt32(number * 1000);
                            case "m":
                                return Convert.ToInt32(number * 1000 * 1000);
                            case "b":
                                return Convert.ToInt32(number * 1000 * 1000 * 1000);
                            default:
                                break;
                        }
                    }
                    else
                    {
                        return Convert.ToInt32(number);
                    }
                }
            }

            return null;
        }
    }
}
