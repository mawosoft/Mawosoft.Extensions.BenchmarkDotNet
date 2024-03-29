// Copyright (c) 2021-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.Extensions.BenchmarkDotNet.ApiCompat;

internal static class GenerateResultWrapper
{
    public static GenerateResult Create(
        ArtifactsPaths artifactsPaths,
        bool isGenerateSuccess,
        Exception? generateException,
        IReadOnlyCollection<string> artifactsToCleanup)
    {
        return new GenerateResult(artifactsPaths, isGenerateSuccess, generateException!, artifactsToCleanup);
    }

    public static GenerateResult Success()
        => Create(ArtifactsPaths.Empty, true, null, []);
}
