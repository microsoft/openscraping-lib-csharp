// -----------------------------------------------------------------------
// <copyright file="ITransformationFromContainer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Transformations
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public interface ITransformationFromObject
    {
        object Transform(Dictionary<string, object> settings, object input);
    }
}
