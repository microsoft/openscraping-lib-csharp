// -----------------------------------------------------------------------
// <copyright file="ParseDateTransformation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public class ParseDateTransformation : ITransformationFromObject, ITransformationFromHtml
    {
        public object Transform(Dictionary<string, object> settings, object input)
        {
            if (input != null && input is string)
            {
                var rawDate = (string)input;
                return ParseDate(settings, rawDate);
            }

            return null;
        }

        public object Transform(Dictionary<string, object> settings, HtmlAgilityPack.HtmlNode node, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            if (node != null)
            {
                var rawDate = node.InnerText;
                return ParseDate(settings, rawDate);
            }

            return null;
        }

        private object ParseDate(Dictionary<string, object> settings, string rawDate)
        {
            string format = null;
            var formatProvider = CultureInfo.InvariantCulture;
            var dateStyle = DateTimeStyles.None;

            if (settings != null)
            {
                if (settings.ContainsKey("_format") && ((JValue)settings["_format"]).Type == JTokenType.String)
                {
                    format = settings["_format"].ToString();
                }

                if (settings.ContainsKey("_formatProvider") && ((JValue)settings["_formatProvider"]).Type == JTokenType.String)
                {
                    var rawFormatProvider = settings["_formatProvider"].ToString();
                    formatProvider = new CultureInfo(rawFormatProvider);
                }

                if (settings.ContainsKey("_dateStyle") && ((JValue)settings["_dateStyle"]).Type == JTokenType.String)
                {
                    var rawDateStyle = settings["_dateStyle"].ToString();
                    dateStyle = (DateTimeStyles)Enum.Parse(typeof(DateTimeStyles), rawDateStyle);
                }
            }

            if (format != null)
            {
                format = settings["_format"].ToString();

                if (DateTime.TryParseExact(rawDate, format, formatProvider, dateStyle, out DateTime date))
                {
                    return date;
                }
            }
            else if (DateTime.TryParse(rawDate, formatProvider, dateStyle, out DateTime date))
            {
                return date;
            }

            return null;
        }
    }
}
