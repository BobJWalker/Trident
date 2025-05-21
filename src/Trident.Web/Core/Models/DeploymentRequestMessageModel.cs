using System;

namespace Trident.Web.Core.Models
{
    public class DeploymentRequestMessageModel
    {
        public string InstanceId { get; set; }
        public string ApiKey { get; set; }
        public string DeploymentRequestId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime RequestQueueTime { get; set; }
    }
}
