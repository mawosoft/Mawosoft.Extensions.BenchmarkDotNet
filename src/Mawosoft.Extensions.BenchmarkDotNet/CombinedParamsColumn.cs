// Copyright (c) 2021-2024 Matthias Wolf, Mawosoft.

namespace Mawosoft.Extensions.BenchmarkDotNet;

/// <summary>
/// An alternative to <see cref="DefaultColumnProviders.Params"/> that displays all parameters in a single,
/// customizable column.
/// </summary>
public class CombinedParamsColumn : IColumn
{
    private readonly string _formatNameValue;
    private readonly string _separator;
    private readonly string _prefix;
    private readonly string _suffix;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombinedParamsColumn"/> class with optional custom
    /// formatting.
    /// </summary>
    public CombinedParamsColumn(string formatNameValue = "{0}={1}",
        string separator = ", ", string prefix = "", string suffix = "")
    {
        _formatNameValue = formatNameValue;
        _separator = separator;
        _prefix = prefix;
        _suffix = suffix;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Id => nameof(CombinedParamsColumn) + "." + ColumnName;
    public string ColumnName => "Params";
    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    public string GetValue(Summary summary, BenchmarkCase benchmarkCase) => GetValue(summary, benchmarkCase, summary?.Style);
    public bool IsAvailable(Summary summary) => true;
    public bool AlwaysShow => false;
    public ColumnCategory Category => ColumnCategory.Params;
    public int PriorityInCategory => 0;
    public override string ToString() => ColumnName;
    public bool IsNumeric => false;
    public UnitType UnitType => UnitType.Dimensionless;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle? style)
        => benchmarkCase is not null && benchmarkCase.Parameters.Items.Any()
           ? _prefix
             + string.Join(
                 _separator,
                 benchmarkCase.Parameters.Items.Select(p => string.Format(
                     style?.CultureInfo,
                     _formatNameValue,
                     p.Name,
                     p.Value?.ToString() ?? ParameterInstance.NullParameterTextRepresentation)))
             + _suffix
           : ParameterInstance.NullParameterTextRepresentation;

    public string Legend => $"All parameter values";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
