﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Trident.Web.Core.Models
{
    [Table("Space")]
    public class SpaceModel : BaseOctopusModel
    {
        public int InstanceId { get; set; }
        public string Name { get; set; }
    }
}
