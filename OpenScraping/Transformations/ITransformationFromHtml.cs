// -----------------------------------------------------------------------
// <copyright file="ITransformationFromHtml.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public interface ITransformationFromHtml
    {
        object Transform(Dictionary<string, object> settings, HtmlAgilityPack.HtmlNode node, List<HtmlAgilityPack.HtmlNode> logicalParents);
    }
}
