// -----------------------------------------------------------------------
// <copyright file="TransformationConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenScraping.Config
{
    using System.Collections.Generic;

    public class TransformationConfig
    {
        public TransformationConfig()
        {
            this.Type = null;
            this.ConfigAttributes = new Dictionary<string, object>();
        }

        public string Type { get; set; }

        public Dictionary<string, object> ConfigAttributes { get; set; }
    }
}
