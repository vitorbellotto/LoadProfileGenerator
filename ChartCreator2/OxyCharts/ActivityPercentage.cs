﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Automation;
using Automation.ResultFiles;
using Common;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    public class ActivityPercentage : ChartBaseFileStep
    {
        public ActivityPercentage([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
            [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
        [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
        calculationProfiler,new List<ResultFileID>() { ResultFileID.ActivityPercentages
        },
        "Activity Percentages",FileProcessingResult.ShouldCreateFiles
        ) {
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcResultFileEntry) {
            string plotName = "Activity Percentages " + srcResultFileEntry.HouseholdKey + " " +
                              srcResultFileEntry.LoadTypeInformation?.Name;

            Profiler.StartPart(Utili.GetCurrentMethodAndClass());
            var consumption =
                new Dictionary<string, List<Tuple<string, double>>>();
            var lastname = string.Empty;
            if (srcResultFileEntry.FullFileName == null) {
                throw new LPGException("Srcfile was null");
            }
            using (var sr = new StreamReader(srcResultFileEntry.FullFileName)) {
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }

                    var cols = s.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                    if (cols.Length == 1) {
                        consumption.Add(cols[0], new List<Tuple<string, double>>());
                        lastname = cols[0];
                        sr.ReadLine();
                    }
                    else {
                        var val = Convert.ToDouble(cols[1], CultureInfo.CurrentCulture);
                        consumption[lastname].Add(new Tuple<string, double>(cols[0], val));
                    }
                }
            }
            foreach (var pair in consumption) {
                var personname = pair.Key;

                var plotModel1 = new PlotModel();
                Legend l = new Legend();
                plotModel1.Legends.Add(l);
                l.LegendBorderThickness = 0;
                l.LegendOrientation = LegendOrientation.Horizontal;
                l.LegendPlacement = LegendPlacement.Outside;
                l.LegendPosition = LegendPosition.BottomCenter;
                if (Parameters.ShowTitle) {
                    plotModel1.Title = plotName + " " + personname;
                }

                var pieSeries1 = new PieSeries
                {
                    InsideLabelColor = OxyColors.White,
                    InsideLabelPosition = 0.8,
                    StrokeThickness = 2,
                    AreInsideLabelsAngled = true
                };
                OxyPalette p;
                if (pair.Value.Count > 2) {
                    p = OxyPalettes.HueDistinct(pair.Value.Count);
                }
                else {
                    p = OxyPalettes.Hue64;
                }
                pieSeries1.InsideLabelColor = OxyColor.FromRgb(0, 0, 0);
                var i = 0;
                foreach (var tuple in pair.Value) {
                    var name = tuple.Item1.Trim();
                    if (name.Length > 30) {
                        name = name.Substring(0, 20) + "...";
                    }
                    var slice = new PieSlice(name, tuple.Item2)
                    {
                        Fill = p.Colors[i++]
                    };
                    pieSeries1.Slices.Add(slice);
                }

                plotModel1.Series.Add(pieSeries1);
                Save(plotModel1, plotName, srcResultFileEntry.FullFileName + "." + personname, Parameters.BaseDirectory, CalcOption.ActivationFrequencies);
            }
            Profiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }
    }
}