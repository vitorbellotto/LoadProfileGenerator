﻿using System.Collections.Generic;
using System.IO;
using Automation;
using Common;
using Common.JSON;
using Database;
using Database.Tables.Houses;
using Database.Tests;
using JetBrains.Annotations;
using NUnit.Framework;
using SimulationEngineLib.SimZukunftProcessor;

namespace SimulationEngine.Tests.SimZukunftProcessor
{
    public class ParallelJsonLauncherTests
    {
        [Test]
        public void TestParallelLaunch()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            ParallelJsonLauncher.CopyAll( DatabaseSetup.AssemblyDirectory, wd.WorkingDirectory);
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Logger.Info("Assembly directory: "  + DatabaseSetup.AssemblyDirectory);
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            HouseCreationAndCalculationJob hcj = new HouseCreationAndCalculationJob();
            Simulator sim = new Simulator(db.ConnectionString);
            JsonCalcSpecification cspec = JsonCalcSpecification.MakeDefaultsForTesting();
            cspec.DefaultForOutputFiles = OutputFileDefault.OnlySums;
            cspec.CalcOptions = new List<CalcOption>();
            cspec.CalcOptions.Add(CalcOption.SumProfileExternalEntireHouse);
            hcj.CalcSpec = cspec;

            MakeSingleHouse(cspec, wd, hcj, sim.Houses[0], "house1");
            MakeSingleHouse(cspec, wd, hcj, sim.Houses[1], "house2");
            var fn = wd.Combine("profilegenerator.db3");
            hcj.PathToDatabase = fn;
            HouseJobSerializer.WriteJsonToFile(wd.Combine("hj2.json"), hcj);
            File.Copy(db.FileName,fn,true);
            ParallelJsonLauncher.ThrowOnInvalidFile = true;
            ParallelJsonLauncher.ParallelJsonLauncherOptions options = new ParallelJsonLauncher.ParallelJsonLauncherOptions();
            options.JsonDirectory = wd.WorkingDirectory;
            ParallelJsonLauncher.LaunchParallel(options);
            //wd.CleanUp(1);
        }

        private static void MakeSingleHouse([NotNull] JsonCalcSpecification cspec, [NotNull] WorkingDir wd, [NotNull] HouseCreationAndCalculationJob hcj,
                                            [NotNull] House house1, string housename)
        {
            cspec.OutputDirectory = housename;
            var dirtodelete = wd.Combine(cspec.OutputDirectory);
            if (Directory.Exists(dirtodelete)) {
                Directory.Delete(dirtodelete, true);
            }

            hcj.House = house1.MakeHouseData();
            HouseJobSerializer.WriteJsonToFile(wd.Combine( housename+ ".json"), hcj);
        }
    }
}
