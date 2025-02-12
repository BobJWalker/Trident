﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trident.Web.Core.Models
{
    [Table("Deployment")]
    public class DeploymentModel : BaseOctopusModel
    {
        public int ReleaseId { get; set; }
        public int EnvironmentId { get; set; }
        public int? TenantId { get; set; }
        public DateTime QueueTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public string DeploymentState { get; set; }
    }
}
