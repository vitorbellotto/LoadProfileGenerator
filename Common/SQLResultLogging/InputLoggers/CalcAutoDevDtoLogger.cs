﻿using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class CalcAutoDevDtoLogger : DataSaverBase
    {
        public CalcAutoDevDtoLogger([NotNull] SqlResultLoggingService srls)
            : base(typeof(CalcAutoDevDto),  new ResultTableDefinition("AutonomousDeviceDefinitions",ResultTableID.AutonomousDeviceDefinitions, "Json Specification of the autonomous Devices", CalcOption.HouseholdContents), srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            SaveableEntry se = GetStandardSaveableEntry(key);
            var objects = (List<IHouseholdKey>)o;
            var persons = objects.ConvertAll(x => (CalcAutoDevDto) x).ToList();
            foreach (var calcPersonDto in persons) {
                se.AddRow(RowBuilder.Start("Name", calcPersonDto.Name)
                    .Add("Json", JsonConvert.SerializeObject(calcPersonDto, Formatting.Indented)).ToDictionary());
            }
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            Srls.SaveResultEntry(se);
        }
        [ItemNotNull]
        [NotNull]
        public List<CalcAutoDevDto> Load([NotNull] HouseholdKey key)
        {
            if (Srls == null)
            {
                throw new LPGException("Data Logger was null.");
            }
            return Srls.ReadFromJson<CalcAutoDevDto>(ResultTableDefinition, key, ExpectedResultCount.OneOrMore);
        }
    }
}
