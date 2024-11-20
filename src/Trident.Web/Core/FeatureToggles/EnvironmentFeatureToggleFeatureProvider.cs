#nullable enable

using OpenFeature;
using OpenFeature.Model;

namespace Trident.Web.Core.FeatureToggles
{
    public class EnvironmentFeatureToggleFeatureProvider : FeatureProvider
    {
        public override Metadata GetMetadata() => new("environment-variable-featuretoggles");

        public override async Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            return new ResolutionDetails<bool>(flagKey, defaultValue);
        }

        public override Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}

#nullable disable