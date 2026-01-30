namespace Ceres9.Core.Domain;

public readonly record struct StatsSnapshot(int SampleCount, long Sum, int Min, int Max);

