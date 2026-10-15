using System;
using Trident.Web.Core.Constants;
using Trident.Web.Core.Models;

namespace Trident.Web.BusinessLogic.Factories
{
    public interface ISyncModelFactory
    {
        SyncModel CreateModel(string feedUrl);
    }

    public class SyncModelFactory : ISyncModelFactory
    {
        public SyncModel CreateModel(string feedUrl)
        {
            return new SyncModel
            {                
                Created = DateTime.Now,                
                State = SyncState.Queued,                
            };
        }
    }
}
