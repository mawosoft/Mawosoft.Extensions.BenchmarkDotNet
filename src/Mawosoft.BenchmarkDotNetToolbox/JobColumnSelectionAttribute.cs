// Copyright (c) 2021 Matthias Wolf, Mawosoft.

using System;
using BenchmarkDotNet.Configs;

namespace Mawosoft.BenchmarkDotNetToolbox
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class JobColumnSelectionAttribute : Attribute, IConfigSource
    {
        public JobColumnSelectionAttribute(string filterExpression, bool showHiddenValuesInLegend = true)
            => Config = ManualConfig.CreateEmpty().AddColumnProvider(
                new JobColumnSelectionProvider(filterExpression, showHiddenValuesInLegend));
        public IConfig Config { get; }
    }
}
