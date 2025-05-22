using System;

namespace Trident.Web.Core.Models.PubSub
{
    public class DeploymentRequestMessageModel
    {
        public string InstanceUrl { get; set; }
        public string ApiKey { get; set; }
        public string DeploymentRequestId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime RequestQueueTime { get; set; }
    }
}
