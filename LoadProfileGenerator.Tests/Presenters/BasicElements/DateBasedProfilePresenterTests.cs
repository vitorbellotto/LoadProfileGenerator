﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Tests;
using Database;
using Database.Tests;
using FluentAssertions;
using LoadProfileGenerator.Presenters.BasicElements;
using Xunit;
using Xunit.Abstractions;


namespace LoadProfileGenerator.Tests.Presenters.BasicElements {
    public class DateBasedProfilePresenterTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void DateBasedProfilePresenterTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);

                Exception? ex = null;
                var t = new Thread(() =>
                {
                    try
                    {
                        var dbp = sim.DateBasedProfiles.Items.First();
                        var ap = new ApplicationPresenter(null, sim, null);
                        var dp = new DateBasedProfilePresenter(ap, null, dbp);
                        dbp.Should().Be(dp.ThisProfile);
                        dp.AddDataPoint(new DateTime(2010, 1, 1), 1);
                        dp.DeleteAllDataPoints();
                        var hashCode = dp.GetHashCode();
                        if (hashCode == 0)
                        {
                            throw new LPGException("Hashcode was 0");
                        }
                        dp.ImportData();
                        var mypoint = dp.AddDataPoint(new DateTime(2010, 1, 1), 1);

                        dp.RemoveTimepoint(mypoint);
                    }
                    catch (Exception e)
                    {
                        ex = e;
                    }
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
                if (ex != null)
                {
                    throw ex;
                }
                db.Cleanup();
            }
        }

        public DateBasedProfilePresenterTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}