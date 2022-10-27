// Copyright (c) 2021-2022 Matthias Wolf, Mawosoft.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Toolchains.Results;

namespace Mawosoft.Extensions.BenchmarkDotNet.ApiCompat
{
    internal static class ExecuteResultWrapper
    {
        private static readonly ConstructorInfo? s_ctorInternalStable;
        private static readonly ConstructorInfo? s_ctorInternalNightly;

        static ExecuteResultWrapper()
        {
            s_ctorInternalStable = typeof(ExecuteResult).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                new Type[] { typeof(List<Measurement>), typeof(GcStats), typeof(ThreadingStats) },
                null);
            if (s_ctorInternalStable == null)
            {
                s_ctorInternalNightly = typeof(ExecuteResult).GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                    new Type[] { typeof(List<Measurement>), typeof(GcStats), typeof(ThreadingStats), typeof(double) },
                    null);
            }
        }

        public static ExecuteResult Create(
            IEnumerable<Measurement> measurements,
            GcStats gcStats,
            ThreadingStats threadingStats,
            double exceptionFrequency)
        {
            if (s_ctorInternalStable != null)
            {
                return (ExecuteResult)s_ctorInternalStable.Invoke(new object[] { measurements.ToList(), gcStats, threadingStats });
            }
            else if (s_ctorInternalNightly != null)
            {
                return (ExecuteResult)s_ctorInternalNightly.Invoke(new object[] { measurements.ToList(), gcStats, threadingStats, exceptionFrequency });
            }
            else
            {
                throw new MissingMethodException(nameof(ExecuteResult), ".ctor");
            }
        }
    }
}
