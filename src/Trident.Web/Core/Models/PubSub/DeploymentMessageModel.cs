using System;

namespace Trident.Web.Core.Models.PubSub
{
    internal class DeploymentMessageModel
    {
        public string InstanceId { get; set; }
        public string DeploymentId { get; set; }
        
        public string SpaceId { get; set; }
        public string SpaceName { get; set; }

        public string EnvironmentId { get; set; }
        public string EnvironmentName { get; set; }

        public string ProjectId { get; set; }
        public string ProjectName { get; set; }        

        public string TenantId { get; set; }
        public string TenantName { get; set; }

        public string ReleaseId { get; set; }
        public string ReleaseNumber { get; set; }
        public DateTime ReleaseCreationDate { get; set; }

        public DateTime QueueTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime CompletedTime { get; set; }
        public string DeploymentState { get; set; }
    }
}
