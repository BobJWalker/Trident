﻿using System;

namespace Trident.Web.Core.Models.OctopusServerModels
{
    public class ReleaseOctopusModel : BaseOctopusServerModel
    {
        public DateTime Assembled { get; set; }
        public string Version { get; set; }
    }
}
