﻿using Automation;
using Automation.ResultFiles;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using Database.Tables.BasicElements;

namespace CalculationController.InputLoggers {
    /*public class UsedTemperatures : ITypeDescriber {
        public UsedTemperatures([JetBrains.Annotations.NotNull] string dateTime, int timeStep, double temperature)
        {
            DateTime = dateTime;
            TimeStep = timeStep;
            Temperature = temperature;
            HouseholdKey = Constants.GeneralHouseholdKey;
        }

        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public string DateTime { get; set; }

        [UsedImplicitly]
        public double Temperature { get; set; }

        [UsedImplicitly]
        public int TimeStep { get; set; }

        public string GetTypeDescription() => "A list of the calculated daylight times";

        public HouseholdKey HouseholdKey { get; set; }

        public int ID { get; set; }
    }*/

    public class TemperatureDataLogger : DataSaverBase {
        [JetBrains.Annotations.NotNull] private readonly CalcParameters _calcParameters;

        [JetBrains.Annotations.NotNull] private readonly SqlResultLoggingService _srls;

        public TemperatureDataLogger([JetBrains.Annotations.NotNull] SqlResultLoggingService srls, [JetBrains.Annotations.NotNull] CalcParameters calcParameters) : base(
            typeof(TemperatureProfile),
            new ResultTableDefinition("Temperatures", ResultTableID.Temperatures, "Used Temperatures", CalcOption.TemperatureFile),
            srls)
        {
            _srls = srls;
            _calcParameters = calcParameters;
        }

        public override void Run(HouseholdKey key, object o)
        {
            TemperatureProfile tp = (TemperatureProfile)o;
            var tempProfile = tp.GetTemperatureArray(_calcParameters.InternalStartTime,
                _calcParameters.OfficialEndTime,
                _calcParameters.InternalStepsize);
            DateStampCreator dsc = new DateStampCreator(_calcParameters);
            if (!_calcParameters.IsSet(CalcOption.TemperatureFile)) {
                return;
            }

            // var allTemperatures = new List<UsedTemperatures>();
            SaveableEntry se = new SaveableEntry(key, ResultTableDefinition);
            se.AddField("Timestep", SqliteDataType.Integer);
            se.AddField("DateTime", SqliteDataType.Text);
            se.AddField("Temperature", SqliteDataType.Double);
            for (var i = 0; i < _calcParameters.OfficalTimesteps; i++) {
                string timestamp = dsc.MakeDateStringFromTimeStep(new TimeStep(i, 0, false));
                se.AddRow(RowBuilder.Start("Timestep", i).Add("DateTime", timestamp).Add("Temperature", tempProfile[i]).ToDictionary());
            }

            _srls.SaveResultEntry(se);
        }
    }
}