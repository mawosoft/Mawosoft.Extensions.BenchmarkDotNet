// Copyright (c) 2021-2024 Matthias Wolf, Mawosoft.

#if BDN_NIGHTLY
using Perfolizer.Metrology;
#else
using Perfolizer.Mathematics.SignificanceTesting;
using Perfolizer.Mathematics.Thresholds;
#endif
using static Mawosoft.Extensions.BenchmarkDotNet.ColumnCategoryExtensions;

namespace Mawosoft.Extensions.BenchmarkDotNet.Tests;

public class ColumnCategoryExtensionsTests
{
    private static List<Type> GetTypesWrapper(Assembly assembly)
    {
        List<Type> types = [];
        try
        {
            types.AddRange(assembly.GetTypes());
        }
        catch (ReflectionTypeLoadException rtle)
        {
            types.AddRange(rtle.Types.Where(t => t is not null).Select(t => t!));

        }
        return types;
    }

    [Fact]
    public void ToExtended_ExistingCategoriesMatch()
    {
        string[] categoryNamesArray = Enum.GetNames(typeof(ColumnCategory));
        string[] extendedNamesArray = Enum.GetNames(typeof(ExtendedColumnCategory));
        HashSet<string> categoryNames = new(categoryNamesArray);
        HashSet<string> extendedNames = new(extendedNamesArray);
        Assert.Equal(categoryNamesArray.Length, categoryNames.Count);
        Assert.Equal(extendedNamesArray.Length, extendedNames.Count);
        Assert.ProperSuperset(categoryNames, extendedNames);
        foreach (string cname in categoryNames)
        {
            Assert.Equal(
                (ExtendedColumnCategory)Enum.Parse(typeof(ExtendedColumnCategory), cname),
                ColumnCategoryExtensions.ToExtended(
                    (ColumnCategory)Enum.Parse(typeof(ColumnCategory), cname)));
        }
    }

    [Fact]
    public void ToExtended_NonExistingCategories_Unknown()
    {
        Assert.Equal(
            ExtendedColumnCategory.Unknown,
            ColumnCategoryExtensions.ToExtended(ColumnCategory.Metric + 1));
    }

    private class GetExtendedColumnCategory_TheoryData : TheoryData<IColumn, ExtendedColumnCategory>
    {
        private class MockMetricDescriptor : IMetricDescriptor
        {
            public string Id => nameof(MockMetricDescriptor);
            public string DisplayName => nameof(MockMetricDescriptor);
            public string Legend => nameof(MockMetricDescriptor);
            public string NumberFormat => "0.##";
            public UnitType UnitType => UnitType.Size;
#if BDN_NIGHTLY
            public string Unit => SizeUnit.B.Abbreviation;
#else
            public string Unit => SizeUnit.B.Name;
#endif
            public bool TheGreaterTheBetter => false;
            public int PriorityInCategory => 0;
            public bool GetIsAvailable(Metric metric) => true;
        }

        // We get all existing IColumn classes via reflection and handle them by name
        // to discover any new additions not covered by test.
        public GetExtendedColumnCategory_TheoryData()
        {
            static bool predicate(Type t) => t.IsClass && !t.IsAbstract && typeof(IColumn).IsAssignableFrom(t);

            IEnumerable<Type> columnTypes =
                GetTypesWrapper(typeof(IColumn).Assembly).Where(predicate)
                .Concat(typeof(ColumnCategoryExtensions).Assembly.GetTypes().Where(predicate)
                .Distinct());

            List<string> unexpectedTypes = [];

            foreach (Type type in columnTypes)
            {
                switch (type.Name)
                {
                    // BenchmarkDotNet 0.13.1
                    case "BaselineColumn":
                        Add(new BaselineColumn(), ExtendedColumnCategory.Meta);
                        break;
                    case "BaselineRatioColumn":
                        Add(BaselineRatioColumn.RatioMean, ExtendedColumnCategory.Baseline);
                        break;
                    case "BaselineScaledColumn":
#pragma warning disable CS0618 // Type or member is obsolete
                        Add(BaselineScaledColumn.Scaled, ExtendedColumnCategory.Baseline);
#pragma warning restore CS0618 // Type or member is obsolete
                        break;
                    case "CategoriesColumn":
                        Add(new CategoriesColumn(), ExtendedColumnCategory.Category);
                        break;
                    case "JobCharacteristicColumn":
                        Add(JobCharacteristicColumn.AllColumns[0], ExtendedColumnCategory.Job);
                        break;
                    case "LogicalGroupColumn":
                        Add(new LogicalGroupColumn(), ExtendedColumnCategory.Meta);
                        break;
                    case "MetricColumn":
                        Add(new MetricColumn(new MockMetricDescriptor()), ExtendedColumnCategory.Metric);
                        break;
                    case "ParamColumn":
                        Add(new ParamColumn("head"), ExtendedColumnCategory.Params);
                        break;
                    case "RankColumn":
                        Add(RankColumn.Arabic, ExtendedColumnCategory.Custom);
                        break;
                    case "StatisticalTestColumn":
#if BDN_NIGHTLY
                        Add(new StatisticalTestColumn(new Threshold()), ExtendedColumnCategory.Baseline);
#else
                        Add(new StatisticalTestColumn(StatisticalTestKind.Welch, Threshold.Create(ThresholdUnit.Ratio, 0), true), ExtendedColumnCategory.Baseline);
#endif
                        break;
                    case "StatisticColumn":
                        Add(StatisticColumn.Mean, ExtendedColumnCategory.Statistics);
                        break;
                    case "TagColumn":
                        Add(new TagColumn("tag", p => "tag"), ExtendedColumnCategory.Custom);
                        break;
                    case "TargetMethodColumn":
                        Add(TargetMethodColumn.Namespace, ExtendedColumnCategory.TargetMethod);
                        Add(TargetMethodColumn.Type, ExtendedColumnCategory.TargetMethod);
                        Add(TargetMethodColumn.Method, ExtendedColumnCategory.TargetMethod);
                        break;
                    // BenchmarkDotNet 0.13.2
                    case "BaselineAllocationRatioColumn":
                        Add(BaselineAllocationRatioColumn.RatioMean, ExtendedColumnCategory.Metric);
                        break;
                    // Mawosoft.Extensions.BenchmarkDotNet
                    case "CombinedParamsColumn":
                        Add(new CombinedParamsColumn(), ExtendedColumnCategory.Params);
                        break;
                    case "JobCharacteristicColumnWithLegend":
                        Add(new JobCharacteristicColumnWithLegend(JobCharacteristicColumn.AllColumns[0], "legend"), ExtendedColumnCategory.Job);
                        break;
                    case "RecyclableParamColumn":
                        Add(new RecyclableParamColumn(1, "param", false), ExtendedColumnCategory.Params);
                        break;
                    default:
                        unexpectedTypes.Add(type.Name);
                        break;
                }
            }

            Assert.Empty(unexpectedTypes);
        }
    }

    [Theory]
    [ClassData(typeof(GetExtendedColumnCategory_TheoryData))]
    internal void GetExtendedColumnCategory_Succeeds(IColumn column, ExtendedColumnCategory extendedColumnCategory)
    {
        Assert.Equal(extendedColumnCategory, ColumnCategoryExtensions.GetExtendedColumnCategory(column));
    }

    private class GetExtendedColumnCategories_TheoryData : TheoryData<IColumnProvider, IEnumerable<ExtendedColumnCategory>>
    {
        // We get all existing IColumnProvider classes via reflection and handle them by name
        // to discover any new additions not covered by test.
        public GetExtendedColumnCategories_TheoryData()
        {
            static bool predicate(Type t) => t.IsClass && !t.IsAbstract && typeof(IColumnProvider).IsAssignableFrom(t);

            IEnumerable<Type> providerTypes =
                GetTypesWrapper(typeof(IColumnProvider).Assembly).Where(predicate)
                .Concat(typeof(ColumnCategoryExtensions).Assembly.GetTypes().Where(predicate)
                .Distinct());

            List<string> unexpectedTypes = [];

            foreach (Type type in providerTypes)
            {
                switch (type.Name)
                {
                    // BenchmarkDotNet
                    case "CompositeColumnProvider":
                        Add(new CompositeColumnProvider(
                                DefaultColumnProviders.Descriptor, DefaultColumnProviders.Job),
                            [ExtendedColumnCategory.TargetMethod, ExtendedColumnCategory.Job]);
                        Add(new CompositeColumnProvider(
                                new JobColumnSelectionProvider("-all +job", true), new RecyclableParamsColumnProvider()),
                            [ExtendedColumnCategory.Job, ExtendedColumnCategory.Params]);
                        break;
                    case "EmptyColumnProvider":
                        Add(EmptyColumnProvider.Instance, []);
                        break;
                    case "SimpleColumnProvider":
                        Add(new SimpleColumnProvider(
                                StatisticColumn.Mean, StatisticColumn.Error, TargetMethodColumn.Method),
                            [ExtendedColumnCategory.Statistics, ExtendedColumnCategory.TargetMethod]);
                        Add(new SimpleColumnProvider(new CategoriesColumn()),
                            [ExtendedColumnCategory.Category]);
                        break;
                    case "DescriptorColumnProvider":
                        Add(DefaultColumnProviders.Descriptor, [ExtendedColumnCategory.TargetMethod]);
                        break;
                    case "JobColumnProvider":
                        Add(DefaultColumnProviders.Job, [ExtendedColumnCategory.Job]);
                        break;
                    case "StatisticsColumnProvider":
                        Add(DefaultColumnProviders.Statistics, [ExtendedColumnCategory.Statistics]);
                        break;
                    case "ParamsColumnProvider":
                        Add(DefaultColumnProviders.Params, [ExtendedColumnCategory.Params]);
                        break;
                    case "MetricsColumnProvider":
                        Add(DefaultColumnProviders.Metrics, [ExtendedColumnCategory.Metric]);
                        break;
                    // Mawosoft.Extensions.BenchmarkDotNet
                    case "JobColumnSelectionProvider":
                        Add(new JobColumnSelectionProvider("+all", true), [ExtendedColumnCategory.Job]);
                        break;
                    case "RecyclableParamsColumnProvider":
                        Add(new RecyclableParamsColumnProvider(), [ExtendedColumnCategory.Params]);
                        break;
                    default:
                        unexpectedTypes.Add(type.Name);
                        break;
                }
            }

            Assert.Empty(unexpectedTypes);
        }
    }

    [Theory]
    [ClassData(typeof(GetExtendedColumnCategories_TheoryData))]
    internal void GetExtendedColumnCategories_Succeeds(IColumnProvider provider, IEnumerable<ExtendedColumnCategory> extendedColumnCategories)
    {
        Assert.Equal(
            new HashSet<ExtendedColumnCategory>(extendedColumnCategories),
            new HashSet<ExtendedColumnCategory>(ColumnCategoryExtensions.GetExtendedColumnCategories(provider)));
    }

    // For ColumnCategoryExtensions.RemoveColumnsByCategory see RemoveColumnsByCategory_Succeeds
    // in ConfigExtensionsTests.Columns.cs
}
