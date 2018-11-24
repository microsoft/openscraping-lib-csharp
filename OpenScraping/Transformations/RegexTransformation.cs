// -----------------------------------------------------------------------
// <copyright file="RegexTransformation.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class RegexTransformation : ITransformationFromHtml
    {
        public object Transform(Dictionary<string, object> settings, HtmlAgilityPack.HtmlNode node, List<HtmlAgilityPack.HtmlNode> logicalParents)
        {
            if (node != null)
            {
                var text = node.InnerText;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    object regexPatternObj;

                    if (!settings.TryGetValue("_regex", out regexPatternObj))
                    {
                        throw new ArgumentException("Could not find a _regex setting");
                    }

                    var forceArray = false;

                    object forceArrayObj;

                    if (settings.TryGetValue("_forceArray", out forceArrayObj))
                    {
                        forceArray = bool.Parse(forceArrayObj.ToString());
                    }

                    var regexOptions = RegexOptions.None;

                    if (settings.ContainsKey("_regexOption") && ((JToken)settings["_regexOption"]).Type == JTokenType.String)
                    {
                        var rawOption = settings["_regexOption"].ToString();
                        regexOptions = (RegexOptions)Enum.Parse(typeof(RegexOptions), rawOption);
                    }

                    if (settings.ContainsKey("_regexOptions") && ((JToken)settings["_regexOptions"]).Type == JTokenType.Array)
                    {
                        var rawOptions = (JArray)settings["_regexOptions"];

                        foreach (var rawOption in rawOptions)
                        {
                            regexOptions = regexOptions | (RegexOptions)Enum.Parse(typeof(RegexOptions), (string)rawOption);
                        }
                    }

                    var regexPattern = regexPatternObj.ToString();

                    var regex = new Regex(regexPattern, regexOptions);
                    var matches = regex.Matches(text);

                    if (!forceArray
                        && matches.Count == 1
                        && matches[0].Groups.Count == 2)
                    {

                        return matches[0].Groups[1].Value;
                    }

                    var returnedMatches = new List<JObject>();

                    foreach (Match match in matches)
                    {
                        var returnedMatch = new JObject();

                        // Ignore first group
                        for (var i = 1; i < match.Groups.Count; i++)
                        {
                            var group = match.Groups[i];
                            var groupName = regex.GroupNameFromNumber(i);

                            if (!forceArray && group.Captures.Count == 1)
                            {
                                returnedMatch[groupName] = group.Value;
                            }
                            else
                            {
                                var captures = new List<string>();

                                foreach (var capture in group.Captures)
                                {
                                    captures.Add(capture.ToString());
                                }

                                returnedMatch[groupName] = new JArray(captures);
                            }
                        }

                        returnedMatches.Add(returnedMatch);
                    }

                    if (!forceArray
                        && returnedMatches.Count == 1)
                    {
                        return returnedMatches[0];
                    }

                    return new JArray(returnedMatches);
                }
            }

            return null;
        }
    }
}
