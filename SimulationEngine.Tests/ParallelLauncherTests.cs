﻿using System.Collections.Generic;
using System.IO;
using Automation;
using Common;
using Common.Tests;
using NUnit.Framework;
using SimulationEngineLib.SettlementCalculation;

namespace SimulationEngine.Tests
{
    [TestFixture]
    public class ParallelLauncherTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void FindNumberOfCoresTest() => Logger.Info(ParallelLauncher.FindNumberOfCores().ToString());

        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            const string path = @"G:\2016.2.1..20.6";
            Directory.SetCurrentDirectory(path);
            List<string> arguments = new List<string>
            {
                "--LaunchParallel",
                "--NumberCores",
                "3",
                "--Batchfile",
                "Start-DissR2NoBridge.cmd",
                "--Archive",
                @"x:\R2_BridgeDaysWithout"
            };
            Program.Main(arguments.ToArray());
        }
    }
}