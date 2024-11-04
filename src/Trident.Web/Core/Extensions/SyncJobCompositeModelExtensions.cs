using Trident.Web.Core.Models.CompositeModels;

namespace Trident.Web.Core.Extensions
{
    public static class SyncJobCompositeModelExtensions
    {
        public static string GetMessagePrefix(this SyncJobCompositeModel syncJobCompositeModel)
        {
            return $"Sync {syncJobCompositeModel.SyncModel.Id}: ";
        }        
    }
}