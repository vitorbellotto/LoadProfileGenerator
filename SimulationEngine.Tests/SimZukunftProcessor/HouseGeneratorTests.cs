﻿using System.Collections.Generic;
using Automation;
using Common.Enums;
using FluentAssertions;
using SimulationEngineLib.HouseJobProcessor;
using Xunit;


namespace SimulationEngine.Tests.SimZukunftProcessor
{
    public class HouseGeneratorTests
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunMatchingTest()
        {
            PersonCategory pc10Male = new PersonCategory(10, PermittedGender.Male);
            PersonCategory pc15Male = new PersonCategory(15, PermittedGender.Male);
            PersonCategory pc20Male = new PersonCategory(20,PermittedGender.Male);
            PersonCategory pc30Male = new PersonCategory(30, PermittedGender.Male);
            PersonCategory pc31Male = new PersonCategory(31, PermittedGender.Male);
            PersonCategory pc40Male = new PersonCategory(40, PermittedGender.Male);
            PersonCategory pc41Male = new PersonCategory(41, PermittedGender.Male);
            List<PersonCategory> demand = new List<PersonCategory>
            {
                pc10Male
            };
            {
                List<PersonCategory> offer = new List<PersonCategory>
                {
                    pc15Male
                };
                var success = HouseGenerator.AreOfferedCategoriesEnough(offer, demand);
                success.Should().BeTrue();
            }
            {
                List<PersonCategory> offer = new List<PersonCategory>
                {
                    pc20Male
                };
                var success = HouseGenerator.AreOfferedCategoriesEnough(offer, demand);
                success.Should().BeFalse();
            }
            demand.Clear();
            demand.Add(pc31Male);
            demand.Add(pc30Male);
            {
                List<PersonCategory> offer = new List<PersonCategory>
                {
                    pc40Male,
                    pc41Male
                };
                var success = HouseGenerator.AreOfferedCategoriesEnough(offer, demand);
                success.Should().BeTrue();
            }
            {
                List<PersonCategory> offer = new List<PersonCategory>
                {
                    pc40Male,
                    pc15Male
                };
                var success = HouseGenerator.AreOfferedCategoriesEnough(offer, demand);
                success.Should().BeFalse();
            }
        }

        //[Fact]
        //[Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        //public void HouseGeneratorTest()
        //{
        //    Logger.Get().StartCollectingAllMessages();
        //    HouseData hd = new HouseData(Guid.NewGuid().ToStrGuid(),
        //        "HT01",10000,1000, "MyFirstHouse");
        //    var hh = new HouseholdData(Guid.NewGuid().ToStrGuid(), false,
        //        "blub",null,null,null,null, HouseholdDataSpecifictionType.ByPersons);
        //    hd.Households.Add(hh);
        //    hh.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(new List<PersonData>() {
        //        new PersonData(30, Gender.Male)
        //    });
        //    HouseGenerator hg = new HouseGenerator();
        //    DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
        //    Simulator sim = new Simulator(db.ConnectionString);
        //    WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
        //    StreamWriter sw = new StreamWriter(districtDefinitionFile);
        //    var jcs = new JsonCalcSpecification
        //    {
        //        DefaultForOutputFiles = OutputFileDefault.Reasonable,
        //        StartDate = new DateTime(2017, 1, 1),
        //        EndDate = new DateTime(2017, 1, 3),
        //        GeographicLocation = sim.GeographicLocations.FindFirstByName("Berlin", FindMode.Partial)?.GetJsonReference() ?? throw new LPGException("No Berlin in the DB"),
        //        TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference()
        //    };
        //    sw.WriteLine(JsonConvert.SerializeObject(tkd, Formatting.Indented));
        //    sw.Close();
        //    string calcspec = wd.Combine("CalculationSpecification.json");
        //    StreamWriter sw1 = new StreamWriter(calcspec);
        //    sw1.WriteLine(JsonConvert.SerializeObject(jcs, Formatting.Indented));
        //    sw1.Close();
        //    string errorLog = wd.Combine("HouseholdCreationErrorlog.csv");
        //    hg.Run( districtDefinitionFile, db.FileName, wd.Combine("Results"),
        //        errorLog, calcspec);
        //    var resultsDir = wd.Combine("Results");
        //    DirectoryInfo di = new DirectoryInfo(resultsDir);
        //    var jsons = di.GetFiles("*.json");
        //    foreach (var info in jsons) {
        //        JsonCalculator jc = new JsonCalculator();
        //        JsonDirectoryOptions jo = new JsonDirectoryOptions
        //        {
        //            Input = info.FullName
        //        };
        //        jc.Calculate(jo);
        //    }
        //}
        //[Fact]
        //[Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        //public void HouseGeneratorTransportationTest()
        //{
        //    DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
        //    Simulator sim = new Simulator(db.ConnectionString);
        //    sim.MyGeneralConfig.PerformCleanUpChecks = "false";
        //    SimIntegrityChecker.Run(sim);
        //    Logger.Get().StartCollectingAllMessages();
        //    HouseData hd = new HouseData(Guid.NewGuid().ToStrGuid(),
        //        "HT01", 10000, 1000, "MyFirstHouse");

        //    var chargingstationSet = sim.ChargingStationSets.FindFirstByName("Charging At Work with 22 kW");
        //    if (chargingstationSet == null) {
        //        throw new LPGException("missing chargingstationset");
        //    }
        //    var transportationdevices = sim.TransportationDeviceSets.FindFirstByName("Bus and two slow Cars",FindMode.Partial);
        //    if (transportationdevices == null) {
        //        throw new LPGException(("missing transportation devices"));
        //    }
        //    var travelRoutes = sim.TravelRouteSets.FindFirstByName("Travel Route Set for 30km to Work");
        //    if (travelRoutes == null) {
        //        throw new LPGException("missing travel routes");
        //    }

        //    {
        //        var transportationModifiers = new List<TransportationDistanceModifier> {
        //            new TransportationDistanceModifier("Work", "Car", 50000),
        //            new TransportationDistanceModifier("Entertainment", "Car", 70000)
        //        };
        //        var hh = new HouseholdData(Guid.NewGuid().ToStrGuid(),
        //            true,
        //            "blub",
        //            chargingstationSet.GetJsonReference(),
        //            transportationdevices.GetJsonReference(),
        //            travelRoutes.GetJsonReference(),
        //            transportationModifiers, HouseholdDataSpecifictionType.ByPersons);
        //        hh.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(new List<PersonData>() {
        //            new PersonData(30, Gender.Male)
        //        });
        //        hd.Households.Add(hh);
        //    }
        //    {
        //        var transportationModifiers = new List<TransportationDistanceModifier> {
        //            new TransportationDistanceModifier("Work", "Car", 70000),
        //            new TransportationDistanceModifier("Entertainment", "Car", 70000)
        //        };
        //        var hh = new HouseholdData(Guid.NewGuid().ToStrGuid(),
        //            true,
        //            "blub",
        //            chargingstationSet.GetJsonReference(),
        //            transportationdevices.GetJsonReference(),
        //            travelRoutes.GetJsonReference(),
        //            transportationModifiers, HouseholdDataSpecifictionType.ByPersons);
        //        hh.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(new List<PersonData>() {
        //            new PersonData(31, Gender.Male)
        //        });
        //        hd.Households.Add(hh);
        //    }

        //    HouseGenerator hg = new HouseGenerator();

        //    WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
        //    string districtDefinitionFile = wd.Combine("DistrictData.json");
        //    StreamWriter sw = new StreamWriter(districtDefinitionFile);
        //    tkd.Houses.Add(hd);
        //    var jcs = new JsonCalcSpecification
        //    {
        //        DefaultForOutputFiles = OutputFileDefault.Reasonable,
        //        StartDate = new DateTime(2017, 1, 1),
        //        EndDate = new DateTime(2017, 1, 30),
        //        GeographicLocation = sim.GeographicLocations.FindFirstByName("Berlin", FindMode.Partial)?.GetJsonReference() ?? throw new LPGException("No Berlin in the DB"),
        //        TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference()
        //    };
        //    string jsonDistrictDefinition = JsonConvert.SerializeObject(tkd, Formatting.Indented);
        //    Logger.Info(jsonDistrictDefinition);
        //    sw.WriteLine(jsonDistrictDefinition);
        //    sw.Close();
        //    string calcspec = wd.Combine("CalculationSpecification.json");
        //    StreamWriter sw1 = new StreamWriter(calcspec);
        //    sw1.WriteLine(JsonConvert.SerializeObject(jcs, Formatting.Indented));
        //    sw1.Close();
        //    string errorLog = wd.Combine("HouseholdCreationErrorlog.csv");
        //    hg.Run(districtDefinitionFile, db.FileName, wd.Combine("Results"),
        //        errorLog, calcspec);
        //    var resultsDir = wd.Combine("Results");
        //    DirectoryInfo di = new DirectoryInfo(resultsDir);
        //    var jsons = di.GetFiles("*.json");
        //    foreach (var info in jsons)
        //    {
        //        JsonCalculator jc = new JsonCalculator();
        //        JsonDirectoryOptions jo = new JsonDirectoryOptions
        //        {
        //            Input = info.FullName
        //        };
        //        jc.Calculate(jo);
        //    }
        //    SimIntegrityChecker.Run(sim);
        //}


        //[Fact]
        //[Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        //public void HouseGeneratorModifiedRouteTest()
        //{
        //    DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
        //    Simulator sim = new Simulator(db.ConnectionString);
        //    sim.MyGeneralConfig.PerformCleanUpChecks = "false";
        //    Logger.Get().StartCollectingAllMessages();
        //    HouseData hd = new HouseData(Guid.NewGuid().ToStrGuid(),
        //        "HT01", 10000, 1000, "MyFirstHouse");

        //    var chargingstationSet = sim.ChargingStationSets.FindFirstByName("Charging At Work with 22 kW");
        //    if (chargingstationSet == null)
        //    {
        //        throw new LPGException("missing chargingstationset");
        //    }
        //    var transportationdevices = sim.TransportationDeviceSets.FindFirstByName("Bus and two slow Cars", FindMode.Partial);
        //    if (transportationdevices == null)
        //    {
        //        throw new LPGException(("missing transportation devices"));
        //    }
        //    var travelRoutes = sim.TravelRouteSets.FindFirstByName("Travel Route Set for 30km to Work");
        //    if (travelRoutes == null)
        //    {
        //        throw new LPGException("missing travel routes");
        //    }
        //    Random rnd = new Random();
        //    for (int i = 0; i < 10; i++) {
        //        int distance = rnd.Next(100) * 10000;
        //        MakeRandomHousehold(distance, chargingstationSet, transportationdevices, travelRoutes, hd);
        //    }

        //    HouseGenerator hg = new HouseGenerator();

        //    WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
        //    string districtDefinitionFile = wd.Combine("DistrictData.json");
        //    StreamWriter sw = new StreamWriter(districtDefinitionFile);
        //    DistrictData tkd = new DistrictData
        //    {
        //        Name = "hi",
        //        Houses = new List<HouseData>()
        //    };
        //    tkd.Houses.Add(hd);
        //    var jcs = new JsonCalcSpecification
        //    {
        //        DefaultForOutputFiles = OutputFileDefault.Reasonable,
        //        StartDate = new DateTime(2017, 1, 1),
        //        EndDate = new DateTime(2017, 1, 3),
        //        GeographicLocation = sim.GeographicLocations.FindFirstByName("Berlin", FindMode.Partial)?.GetJsonReference() ?? throw new LPGException("No Berlin in the DB"),
        //        TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference()
        //    };
        //    string jsonDistrictDefinition = JsonConvert.SerializeObject(tkd, Formatting.Indented);
        //    Logger.Info(jsonDistrictDefinition);
        //    sw.WriteLine(jsonDistrictDefinition);
        //    sw.Close();
        //    string calcspec = wd.Combine("CalculationSpecification.json");
        //    StreamWriter sw1 = new StreamWriter(calcspec);
        //    sw1.WriteLine(JsonConvert.SerializeObject(jcs, Formatting.Indented));
        //    sw1.Close();
        //    string errorLog = wd.Combine("HouseholdCreationErrorlog.csv");
        //    hg.Run(districtDefinitionFile, db.FileName, wd.Combine("Results"),
        //        errorLog, calcspec);
        //    Simulator sim2 = new Simulator(db.ConnectionString);
        //    SimIntegrityChecker.Run(sim2);
        //}

        //private static void MakeRandomHousehold(int distance, [JetBrains.Annotations.NotNull] ChargingStationSet chargingstationSet, [JetBrains.Annotations.NotNull] TransportationDeviceSet transportationdevices, [JetBrains.Annotations.NotNull] TravelRouteSet travelRoutes, [JetBrains.Annotations.NotNull] HouseData hd)
        //{
        //    var transportationModifiers = new List<TransportationDistanceModifier> {
        //        new TransportationDistanceModifier("Work", "Car", distance),
        //        new TransportationDistanceModifier("Entertainment", "Car", distance)
        //    };
        //    var hh = new HouseholdData(Guid.NewGuid().ToStrGuid(),
        //        true,
        //        "blub",
        //        chargingstationSet.GetJsonReference(),
        //        transportationdevices.GetJsonReference(),
        //        travelRoutes.GetJsonReference(),
        //        transportationModifiers,
        //        HouseholdDataSpecifictionType.ByPersons);
        //    hh.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(new List<PersonData>() {
        //        new PersonData(30, Gender.Male)
        //    });
        //    hd.Households.Add(hh);
        //}

        //[Fact]
        //[Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        //public void HouseGeneratorWithSkipExisitingTest()
        //{
        //    Logger.Get().StartCollectingAllMessages();
        //    HouseData hd = new HouseData(Guid.NewGuid().ToStrGuid(),
        //        "HT01", 10000, 1000, "MyFirstHouse");
        //    var hh = new HouseholdData(Guid.NewGuid().ToStrGuid(), false,
        //        "blub", null, null, null,null,
        //        HouseholdDataSpecifictionType.ByPersons);
        //    hh.HouseholdDataPersonSpecification = new HouseholdDataPersonSpecification(new List<PersonData>() {
        //        new PersonData(30, Gender.Male)
        //    });
        //    hd.Households.Add(hh);
        //    HouseGenerator hg = new HouseGenerator();
        //    DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
        //    Simulator sim = new Simulator(db.ConnectionString);
        //    WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
        //    var jcs = new JsonCalcSpecification
        //    {
        //        DefaultForOutputFiles = OutputFileDefault.Reasonable,
        //        StartDate = new DateTime(2017, 1, 1),
        //        EndDate = new DateTime(2017, 1, 3),
        //        GeographicLocation = sim.GeographicLocations.FindFirstByName("Berlin", FindMode.Partial)?.GetJsonReference() ?? throw new LPGException("No Berlin in the DB"),
        //        TemperatureProfile = sim.TemperatureProfiles[0].GetJsonReference()
        //    };
        //    string calcspec = wd.Combine("CalculationSpecification.json");
        //    StreamWriter sw1 = new StreamWriter(calcspec);
        //    sw1.WriteLine(JsonConvert.SerializeObject(jcs, Formatting.Indented));
        //    sw1.Close();
        //    string errorLog = wd.Combine("HouseholdCreationErrorlog.csv");
        //    Logger.Info("first run");
        //    hg.Run(districtDefinitionFile, db.FileName, wd.Combine("Results"),
        //        errorLog, calcspec);
        //    var resultsDir = wd.Combine("Results");
        //    DirectoryInfo di = new DirectoryInfo(resultsDir);
        //    var jsons = di.GetFiles("*.json");
        //    foreach (var info in jsons)
        //    {
        //        JsonCalculator jc = new JsonCalculator();
        //        JsonDirectoryOptions jo = new JsonDirectoryOptions
        //        {
        //            Input = info.FullName
        //        };
        //        jc.Calculate(jo);
        //    }
        //    Logger.Info("second run");
        //    hg.Run(districtDefinitionFile, db.FileName, wd.Combine("Results"),
        //        errorLog, calcspec);
        //}

    }
}
