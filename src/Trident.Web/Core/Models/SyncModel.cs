using System;
using Dapper;

namespace Trident.Web.Core.Models
{
    [Table("Sync")]
    public class SyncModel : BaseModel
    {        
        public string State { get; set; }        
        public DateTime Created { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Completed { get; set; }        
        public int? RetryAttempts { get; set; }
    }
}
