﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace ChartCreator2.Steps
{
#pragma warning disable CA1040 // Avoid empty interfaces
    public interface IStepParameters {
#pragma warning restore CA1040 // Avoid empty interfaces
    }
    public class GeneralStepParameters:IStepParameters {
    }
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class HouseholdStepParameters : IStepParameters
    {
        public HouseholdStepParameters([JetBrains.Annotations.NotNull] HouseholdKeyEntry key) => Key = key;

        [JetBrains.Annotations.NotNull]
        public HouseholdKeyEntry Key { get;  }
    }

    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class HouseholdLoadtypeStepParameters : IStepParameters
    {
        public HouseholdLoadtypeStepParameters([JetBrains.Annotations.NotNull] HouseholdKeyEntry key, [JetBrains.Annotations.NotNull] CalcLoadTypeDto loadType,
                                                      [JetBrains.Annotations.NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows)
        {
            Key = key;
            LoadType = loadType;
            EnergyFileRows = energyFileRows;
        }

        [JetBrains.Annotations.NotNull]
        public HouseholdKeyEntry Key { get; }
        [JetBrains.Annotations.NotNull]
        public CalcLoadTypeDto LoadType { get; }
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<OnlineEnergyFileRow> EnergyFileRows { get; }
    }

    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class LoadtypeStepParameters : IStepParameters
    {
        [JetBrains.Annotations.NotNull]
        public CalcLoadTypeDto LoadType { get; }
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<OnlineEnergyFileRow> EnergyFileRows { get; }

        public LoadtypeStepParameters([JetBrains.Annotations.NotNull] CalcLoadTypeDto loadType,
                                            [JetBrains.Annotations.NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows)
        {
            LoadType = loadType;
            EnergyFileRows = energyFileRows;
        }
    }

    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class LoadtypeSumStepParameters : IStepParameters
    {
        [JetBrains.Annotations.NotNull]
        public CalcLoadTypeDto LoadType { get; }
        public LoadtypeSumStepParameters([JetBrains.Annotations.NotNull] CalcLoadTypeDto loadType)
        {
            LoadType = loadType;
        }
    }

    public interface IRequireOptions {
        public List<CalcOption> NeededOptions { get; }
        List<CalcOption> Options { get; }
        public string StepName { get; }
    }
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public  abstract class BasicChartProcessingStep: IRequireOptions
    {

        public int Priority { get; }
        [JetBrains.Annotations.NotNull] private readonly List<CalcOption> _options;

        protected BasicChartProcessingStep([JetBrains.Annotations.NotNull] CalcDataRepository repository, [JetBrains.Annotations.NotNull] List<CalcOption> options,
                                             [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                             [JetBrains.Annotations.NotNull] string stepName, int priority)
        {
            Repository = repository;
            _options = options;
            _calculationProfiler = calculationProfiler;
            _stepName = stepName;
            Priority = priority;
        }
        public void Run([JetBrains.Annotations.NotNull] IStepParameters parameters)
        {
            if (!IsEnabled()) {
                return;
            }
            _calculationProfiler.StartPart(StepName);
            try {
                PerformActualStep(parameters);
            }
            finally {
                _calculationProfiler.StopPart(StepName);
            }
        }

        protected abstract void PerformActualStep([JetBrains.Annotations.NotNull] IStepParameters parameters);

        [JetBrains.Annotations.NotNull]
        protected CalcDataRepository Repository { get; }

        [JetBrains.Annotations.NotNull]
        public List<CalcOption> Options => _options;

        [JetBrains.Annotations.NotNull]
        private readonly ICalculationProfiler _calculationProfiler;
        [JetBrains.Annotations.NotNull]
        private readonly string _stepName;

        public bool IsEnabled()
        {
            if (_options.Any(x=> Repository.CalcParameters.IsSet(x)))
            {
                return true;
            }
            return false;
        }

        public  abstract List<CalcOption> NeededOptions { get; }

        [JetBrains.Annotations.NotNull]
        public string StepName => _stepName;
    }
}
