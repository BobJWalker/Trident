using System;

namespace Trident.Web.Core.Models.PubSub
{
    public class DeploymentRequestMessageModel
    {
        public string DeploymentRequestId { get; set; }

        public string InstanceUrl { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime RequestQueueTime { get; set; }
    }
}
