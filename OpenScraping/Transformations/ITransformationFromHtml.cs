// -----------------------------------------------------------------------
// <copyright file="ITransformationFromHtml.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using System.Collections.Generic;
    using HtmlAgilityPack;
    using Newtonsoft.Json.Linq;

    public interface ITransformationFromHtml
    {
        object Transform(Dictionary<string, object> settings, HtmlNodeNavigator nodeNavigator, List<HtmlAgilityPack.HtmlNode> logicalParents);
    }
}
