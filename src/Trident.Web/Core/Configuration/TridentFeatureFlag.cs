using OpenFeature;
using System.Threading.Tasks;

namespace Trident.Web.Core.Configuration
{
    public interface ITridentFeatureFlag
    {
        public Task<bool> IsEnabledAsync(string featureFlagName);
    }

    public class TridentFeatureFlag (IFeatureClient featureClient) : ITridentFeatureFlag
    {
        public async Task<bool> IsEnabledAsync(string featureFlagName)
        {
            var isEnabled = await featureClient.GetBooleanValueAsync(featureFlagName, false);

            return isEnabled;
        }
    }

    public static class TridentFeatureFlagList
    {
        public const string BackgroundSyncJob = "BackgroundSyncJob";
        public const string ViewFeedList = "view-feed-list";
    }
}
