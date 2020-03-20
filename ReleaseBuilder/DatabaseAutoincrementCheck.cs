﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tests;
using NUnit.Framework;

namespace ReleaseBuilder {
    [TestFixture]
    public class DatabaseAutoincrementCheck {
        [Test]
        [Category("BasicTest")]
        public void RunDatabaseTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.ReleaseBuilder);

            using (var con = new Connection(db.ConnectionString)) {
                con.Open();
                using (var cmd = new Command(con)) {
                    var dr = cmd.ExecuteReader("SELECT name FROM sqlite_master WHERE type='table';");
                    var tables = new List<string>();
                    while (dr.Read()) {
                        tables.Add((string) dr["name"]);
                    }
                    dr.Dispose();
                    tables.Sort();
                    foreach (var s in tables) {
                        if (s.StartsWith("tbl", StringComparison.Ordinal)) {
                            var o = cmd.ExecuteScalar("SELECT COUNT(*) FROM sqlite_sequence WHERE name='" + s +
                                                      "';");
                            var i = o as long?;
                            if (i == null) {
                                Console.WriteLine(o.GetType());
                            }
                            if (i != 1) {
                                var max = cmd.ExecuteScalar("SELECT max(ID) FROM " +s);

                                throw new LPGException("table " + s + " is missing an autoincrement! Max ID was " + max);
                            }
                            var drPragma = cmd.ExecuteReader("PRAGMA TABLE_INFO(" + s + ");");
                            var idFound = false;
                            while (drPragma.Read()) {
                                var name = (string) drPragma["name"];
                                if (name == "ID") {
                                    idFound = true;
                                    var notnull = drPragma["notnull"];
                                    if (notnull == null) {
                                        throw new InvalidOperationException();
                                    }
                                    var notnulli = (long) notnull;
                                    if (notnulli != 1) {
                                        Logger.Error("not null missing:" + s);
                                    }
                                    long pk = drPragma.GetLong("pk");
                                    //var pki = (long) pk;
                                    if (pk != 1) {
                                        Logger.Error("pk missing: " + s);
                                    }
                                }
                            }
                            drPragma.Dispose();
                            if (!idFound) {
                                throw new LPGException("No ID field in " + s);
                            }
                        }
                    }
                }
            }
            db.Cleanup();
        }
        [Test]
        [Category("BasicTest")]
        public void RunDatabaseGuidTest()
        {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.ReleaseBuilder);

            using (var con = new Connection(db.ConnectionString))
            {
                con.Open();
                using (var cmd = new Command(con))
                {
                    var dr = cmd.ExecuteReader("SELECT name FROM sqlite_master WHERE type='table';");
                    var tables = new List<string>();
                    while (dr.Read())
                    {
                        tables.Add((string)dr["name"]);
                    }
                    dr.Dispose();
                    tables.Sort();
                    foreach (var tablename in tables)
                    {
                        if (tablename.StartsWith("tbl", StringComparison.Ordinal))
                        {
                            var drPragma = cmd.ExecuteReader("PRAGMA TABLE_INFO(" + tablename + ");");
                            var guidFound = false;
                            while (drPragma.Read())
                            {
                                var name = (string)drPragma["name"];
                                Debug.Assert(name != null, nameof(name) + " != null");
                                if (name.ToLower() == "guid")
                                {
                                    guidFound = true;
                                }
                            }
                            drPragma.Dispose();
                            if (!guidFound)
                            {
                                Logger.Info("No guid in " + tablename);
                            }
                        }
                    }
                }
            }
            db.Cleanup();
        }
    }
}