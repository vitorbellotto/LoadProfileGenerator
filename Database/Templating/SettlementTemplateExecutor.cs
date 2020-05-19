﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace Database.Templating {
    public class SettlementTemplateExecutor {
        [ItemNotNull] [NotNull] private readonly ObservableCollection<HouseEntry> _previewHouseEntries = new ObservableCollection<HouseEntry>();
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<HouseEntry> PreviewHouseEntries => _previewHouseEntries;

        public void CreateSettlementFromPreview([NotNull] Simulator sim, [NotNull] SettlementTemplate template)
        {
            if (string.IsNullOrEmpty(template.NewName)) {
                Logger.Error("The new name for the settlement can not be empty.");
                return;
            }
            var sett = sim.Settlements.CreateNewItem(sim.ConnectionString);
            sett.Name = GetSettlementName(sim, template);
            sett.EnergyIntensityType = EnergyIntensityType.AsOriginal;
            sett.Source = "Generated by " + template.Name + " at " + DateTime.Now.ToShortDateString() + " " +
                          DateTime.Now.ToShortTimeString();
            sett.CreationType = CreationType.TemplateCreated;
            if (template.TemperatureProfile != null) {
                sett.TemperatureProfile = template.TemperatureProfile;
            }
            if (template.GeographicLocation != null) {
                sett.GeographicLocation = template.GeographicLocation;
            }

            sett.SaveToDB();
            var creationcount = 1;
            foreach (var housePreviewEntry in _previewHouseEntries) {
                // look if this entry already exits
                House newhouse = null;
                foreach (var house in sim.Houses.It) {
                    var foundMissingEntry = false;
                    if (house.HouseType != housePreviewEntry.HouseType) {
                        continue;
                    }
                    var calcObjects = house.Households.Select(x => x.CalcObject).ToList();
                    foreach (var household in housePreviewEntry.Households) {
                        if (!calcObjects.Contains(household)) {
                            foundMissingEntry = true;
                            break;
                        }
                    }
                    if (!foundMissingEntry && sett.Households.All(x => x.CalcObject != house)) {
                        newhouse = house;
                    }
                }
                if (newhouse == null) {
                    Logger.Info("Creating new house " + creationcount);
                    newhouse = sim.Houses.CreateNewItem(sim.ConnectionString);
                    newhouse.HouseType = housePreviewEntry.HouseType;
                    newhouse.EnergyIntensityType = housePreviewEntry.EnergyIntensityType;
                    newhouse.Name = GetHouseName("(" + sett.Name + ") House", ref creationcount, sim,
                        housePreviewEntry.Households, housePreviewEntry.HouseType);
                    newhouse.CreationType = CreationType.TemplateCreated;
                    foreach (var household in housePreviewEntry.Households) {
                        newhouse.AddHousehold(household,false, null,null,null);
                    }
                    newhouse.Source = "Generated by " + template.Name + " at " + DateTime.Now.ToShortDateString() +
                                      " " + DateTime.Now.ToShortTimeString();
                    if (template.TemperatureProfile != null) {
                        newhouse.TemperatureProfile = template.TemperatureProfile;
                    }
                    if (template.GeographicLocation != null) {
                        newhouse.GeographicLocation = template.GeographicLocation;
                    }
                    newhouse.SaveToDB();

                    creationcount++;
                }
                sett.AddHousehold(newhouse, 1);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void GenerateSettlementPreview( [NotNull] Simulator sim,
            [NotNull] SettlementTemplate template)
        {
            if (Math.Abs(template.HousePercentage - 1) > Constants.Ebsilon) {
                throw new DataIntegrityException(
                    "The house size distribution doesn't add up to 100%. It is right now " +
                    template.HousePercentage * 100 + "%");
            }
            if (Math.Abs(template.HouseholdPercentage - 1) > Constants.Ebsilon) {
                throw new DataIntegrityException(
                    "The household size distribution doesn't add up to 100%. It is right now " +
                    template.HouseholdPercentage * 100 + "%");
            }
            if (template.DesiredHHCount == 0) {
                throw new DataIntegrityException("Can't make a settlement with 0 households.");
            }
            var resultinghouseholds = new List<ICalcObject>();
            var r = new Random();
            MakeHouseholdDistribution(sim.ModularHouseholds.It, sim, resultinghouseholds, r, template);
            // find a house distribution iteratively by increasing the numbers of each house type.
            InitializeHouseSizes(template);
            var previewEntries = new List<HouseEntry>();
            // make the basic assignment
            var housecount = 1;
            foreach (var stHouseSize in template.HouseSizes) {
                for (var i = 0; i < stHouseSize.HouseCount; i++) {
                    var span = stHouseSize.MaximumHouseSize - stHouseSize.MinimumHouseSize;
                    var housesize = stHouseSize.MinimumHouseSize + r.Next(span);
                    var he = new HouseEntry(housesize, stHouseSize.MaximumHouseSize, housecount++,
                        EnergyIntensityType.AsOriginal);
                    previewEntries.Add(he);
                    for (var j = 0; j < housesize && resultinghouseholds.Count > 0; j++) {
                        var householdIndex = r.Next(resultinghouseholds.Count);
                        he.Households.Add(resultinghouseholds[householdIndex]);
                        resultinghouseholds.RemoveAt(householdIndex);
                    }
                }
            }
            // assign the leftovers
            var loopcount = 0;
            while (resultinghouseholds.Count > 0) {
                foreach (var houseEntry in previewEntries) {
                    if (houseEntry.Households.Count <= houseEntry.MaximumHouseSize && resultinghouseholds.Count > 0) {
                        var householdIndex = r.Next(resultinghouseholds.Count);
                        houseEntry.Households.Add(resultinghouseholds[householdIndex]);
                        resultinghouseholds.RemoveAt(householdIndex);
                    }
                }
                loopcount++;
                if (loopcount > 300) {
                    throw new LPGException("Could not assign all households to houses. " + resultinghouseholds.Count +
                                           " was left over. Please fix somehow.");
                }
            }
            var filteredEntries = previewEntries.Where(x => x.Households.Count > 0).ToList();
            // assign the housetypes
            foreach (var houseEntry in filteredEntries) {
                var validHouseTypes = template.HouseTypes
                    .Where(x => houseEntry.Households.Count >= x.HouseType?.MinimumHouseholdCount &&
                                houseEntry.Households.Count <= x.HouseType?.MaximumHouseholdCount)
                    .Select(x => x.HouseType).ToList();
                if (validHouseTypes.Count == 0) {
                    throw new DataIntegrityException("It was not possible to find a house type for a house with " +
                                                     houseEntry.Households.Count +
                                                     " households. Please adjust either the house sizes, add more house types " +
                                                     "or adjust the existing house types to accept a wider range of household counts.");
                }
                var idx = r.Next(validHouseTypes.Count);
                houseEntry.HouseType = validHouseTypes[idx];
            }
            Logger.Get().SafeExecuteWithWait(() => _previewHouseEntries.SynchronizeWithList(filteredEntries));
        }

        [ItemNotNull]
        [NotNull]
        private static List<HouseholdTemplate> GetFittingtemplates([NotNull] STHouseholdDistribution distribution,
            [NotNull] SettlementTemplate template)
        {
            var fittingtemplates = template.HouseholdTemplates
                .Where(x => x.HouseholdTemplate?.Persons.Count >= distribution.MinimumNumber &&
                            x.HouseholdTemplate?.Persons.Count <= distribution.MaximumNumber)
                .Select(x => x.HouseholdTemplate).ToList();
            foreach (var tag in distribution.Tags) {
                var todelete = new List<HouseholdTemplate>();
                foreach (var householdTemplate in fittingtemplates) {
                    if (householdTemplate.TemplateTags.All(x => x.Tag != tag.Tag)) {
                        todelete.Add(householdTemplate);
                    }
                }
                foreach (var householdTemplate in todelete) {
                    fittingtemplates.Remove(householdTemplate);
                }
            }
            if (fittingtemplates.Count == 0) {
                throw new DataIntegrityException("For the household with size >=" + distribution.MinimumNumber +
                                                 " and <=" + distribution.MaximumNumber + " and the tags " +
                                                 distribution.TagDescription +
                                                 " not enough households could be found and " +
                                                 "no household template was assigned.");
            }
            return fittingtemplates;
        }

        [NotNull]
        private static string GetHouseName([NotNull] string basis, ref int offset, [NotNull] Simulator sim,
                                           [ItemNotNull] [NotNull] List<ICalcObject> households,
            [CanBeNull] HouseType ht)
        {
            var houselist = string.Empty;

            var housetype = ht?.Name??"";
            if (ht!=null && ht.Name.Contains(" ")) {
                housetype = ht.Name.Substring(0, ht.Name.IndexOf(" ", StringComparison.Ordinal));
            }
            var builder = new StringBuilder();
            builder.Append(houselist);
            foreach (var calcObject in households) {
                var name = calcObject.Name;
                if (name.ToUpperInvariant().StartsWith("X ", StringComparison.Ordinal)) {
                    name = name.Substring(2);
                }
                if (name.Contains(" ")) {
                    name = name.Substring(0, name.IndexOf(" ", StringComparison.Ordinal));
                }
                builder.Append(name).Append(", ");
            }
            houselist = builder.ToString();
            if (houselist.Length > 2) {
                houselist = houselist.Substring(0, houselist.Length - 2);
            }

            string GetName(int i) => basis + " " + i.ToString("D2", CultureInfo.CurrentCulture) + " " + housetype + " with " + houselist;
            while (sim.Houses.IsNameTaken(GetName(offset))) {
                offset++;
            }
            return GetName(offset);
        }

        private static void GetMachtingCHH([ItemNotNull] [NotNull] List<ModularHousehold> matchingCHH, [NotNull] SettlementTemplate template)
        {
            var chhIdx = 0;
            while (chhIdx < matchingCHH.Count) {
                var chh = matchingCHH[chhIdx];
                var remove = false;
                foreach (var stTraitLimit in template.TraitLimits) {
                    if (stTraitLimit.IsPermitted(chh)) {
                        stTraitLimit.RegisterMHH(chh);
                    }
                    else {
                        remove = true;
                    }
                }
                if (remove) {
                    matchingCHH.Remove(chh);
                    chhIdx--;
                }
                chhIdx++;
            }
        }

        [NotNull]
        private static string GetSettlementName([NotNull] Simulator sim, [NotNull] SettlementTemplate template)
        {
            var nameoffset = 1;

            string GetName(int i)
            {
                var s = template.NewName + " " + i.ToString("D2", CultureInfo.CurrentCulture);
                return s.Replace("  ", " ");
            }

            while (sim.Settlements.IsNameTaken(GetName(nameoffset))) {
                nameoffset++;
            }
            return GetName(nameoffset);
        }

        public static void InitializeHouseSizes([NotNull] SettlementTemplate template)
        {
            var foundSolution = false;
            var multiplier = 0.01;
            while (!foundSolution) {
                foreach (var housesize in template.HouseSizes) {
                    housesize.HouseCount = (int) Math.Round(housesize.Percentage * multiplier);
                }
                var miniumHousehold = template.HouseSizes.Sum(x => x.MinimumHouseholdCount);
                var maximumHousehold = template.HouseSizes.Sum(x => x.MaximumHouseholdCount);
                if (template.DesiredHHCount >= miniumHousehold && template.DesiredHHCount <= maximumHousehold) {
                    foundSolution = true;
                }
                else {
                    multiplier += 0.01;
                }
                if (multiplier > 1000) {
                    throw new DataIntegrityException(
                        "Could not find a suitable house distribution in 1000 iterations. Please modify the house size distribution.");
                }
            }
        }

        /// <summary>
        ///     Calculates the correct number of households for the distribution
        /// </summary>
        /// <param name="template">Template to calculate the counts for</param>
        private static void MakeHouseholdCounts([NotNull] SettlementTemplate template)
        {
            var totalCount = 0;
            foreach (var distribution in template.HouseholdDistributions) {
                distribution.PreciseHHCount = template.DesiredHHCount * distribution.PercentOfHouseholds;
                distribution.RoundedHHCount = (int) Math.Round(distribution.PreciseHHCount);
                totalCount += distribution.RoundedHHCount;
            }
            var hhDistributions =
                template.HouseholdDistributions.Where(x => x.RoundedHHCount > 0).ToList();
            if (hhDistributions.Count == 0) {
                throw new LPGException("Not a single household distribution was found.");
            }
            if (totalCount < template.DesiredHHCount) {
                hhDistributions.Sort((x, y) => y.HHCountRoundingChange.CompareTo(x.HHCountRoundingChange));
                var tooFewCount = template.DesiredHHCount - totalCount;
                for (var i = 0; i < tooFewCount; i++) {
                    hhDistributions[i].RoundedHHCount++;
                }
            }
            if (totalCount > template.DesiredHHCount) {
                hhDistributions.Sort((x, y) => y.HHCountRoundingChange.CompareTo(x.HHCountRoundingChange));
                var tooManyCount = totalCount - template.DesiredHHCount;
                for (var i = 0; i < tooManyCount; i++) {
                    hhDistributions[i].RoundedHHCount--;
                }
            }
            var doubleCheckCount = 0;
            foreach (var stHouseholdDistribution in template.HouseholdDistributions) {
                doubleCheckCount += stHouseholdDistribution.RoundedHHCount;
            }
            if (doubleCheckCount != template.DesiredHHCount) {
                throw new LPGException("Wrong number of households. This is a bug. Please report.");
            }
        }

        private static void MakeHouseholdDistribution([ItemNotNull] [NotNull] ObservableCollection<ModularHousehold> modularHouseholds,
            [NotNull] Simulator sim, [ItemNotNull] [NotNull] List<ICalcObject> resultinghouseholds, [NotNull] Random r, [NotNull] SettlementTemplate template)
        {
            foreach (var limit in template.TraitLimits) {
                limit.Init(template.DesiredHHCount);
            }
            var limits = new List<STTraitLimit>(template.TraitLimits);
            var totalHouseholds = 0;
            MakeHouseholdCounts(template);
            var templateStarts = template.HouseholdTemplates
                .Select(x => x.HouseholdTemplate?.Name.Substring(0, 5)).ToList();
            foreach (var distribution in template.HouseholdDistributions) {
                var matchingCHH = modularHouseholds
                    .Where(x => x.Persons.Count >= distribution.MinimumNumber &&
                                x.Persons.Count <= distribution.MaximumNumber &&
                                x.EnergyIntensityType == distribution.EnergyIntensity).ToList();
                var toDelete = new List<ModularHousehold>();
                foreach (var modularHousehold in matchingCHH) {
                    if (!templateStarts.Any(x => modularHousehold.Name.Contains(x))) {
                        toDelete.Add(modularHousehold);
                    }
                }
                foreach (var modularHousehold in toDelete) {
                    matchingCHH.Remove(modularHousehold);
                }
                foreach (var tag in distribution.Tags) {
                    var todelete = new List<ModularHousehold>();
                    foreach (var household in matchingCHH) {
                        if (household.ModularHouseholdTags.All(x => x.Tag != tag.Tag)) {
                            todelete.Add(household);
                        }
                    }
                    foreach (var modularHousehold in todelete) {
                        matchingCHH.Remove(modularHousehold);
                    }
                }
                GetMachtingCHH(matchingCHH, template);
                var matchingObjects = new List<ICalcObject>();
                matchingObjects.AddRange(matchingCHH);
                for (var i = 0; i < distribution.RoundedHHCount; i++) {
                    totalHouseholds++;
                    if (matchingObjects.Count > 0) {
                        var idx = r.Next(matchingObjects.Count);
                        resultinghouseholds.Add(matchingObjects[idx]);
                        matchingObjects.RemoveAt(idx);
                    }
                    else {
                        var fittingtemplates = GetFittingtemplates(distribution, template);
                        var templateidx = r.Next(fittingtemplates.Count);
                        var ht = fittingtemplates[templateidx];
                        ht.Count = 1;
                        ht.EnergyIntensityType = distribution.EnergyIntensity;

                        Logger.Info("Creating new household with the template " + ht.Name);
                        var newhh = ht.GenerateHouseholds(sim, false, limits);
                        resultinghouseholds.AddRange(newhh);
                    }
                }
            }
            if (totalHouseholds > template.DesiredHHCount) {
                throw new LPGException("Too many households generated. This is a bug. Please report!");
            }
            if (totalHouseholds < template.DesiredHHCount) {
                throw new LPGException("Too few households generated. This is a bug. Please report!");
            }
        }

        public sealed class HouseEntry : IComparable, IEquatable<HouseEntry> {
            public HouseEntry(int houseSize, int maximumHouseSize, int number,
                EnergyIntensityType energyIntensityType)
            {
                HouseSize = houseSize;
                MaximumHouseSize = maximumHouseSize;
                Number = number;
                EnergyIntensityType = energyIntensityType;
                Households = new List<ICalcObject>();
            }

            public EnergyIntensityType EnergyIntensityType { get; }

            [NotNull]
            [UsedImplicitly]
            public string HouseholdList {
                get {
                    var builder = new StringBuilder();
                    foreach (var calcObject in Households) {
                        builder.Append(calcObject.Name).Append(", ");
                    }
                    var s = builder.ToString();
                    return s.Substring(0, s.Length - 2);
                }
            }

            [ItemNotNull]
            [NotNull]
            public List<ICalcObject> Households { get; }

            [UsedImplicitly]
            public int HouseSize { get; }

            [CanBeNull]
            public HouseType HouseType { get; set; }
            public int MaximumHouseSize { get; }

            [UsedImplicitly]
            public int Number { get; }

            public int CompareTo([NotNull] object obj)
            {
                if (!(obj is HouseEntry other))
                {
                    return -1;
                }
                if (HouseType != other.HouseType) {
                    return HouseType?.CompareTo(other.HouseType)??-1;
                }
                return string.Compare(HouseholdList, other.HouseholdList, StringComparison.Ordinal);
            }

            public bool Equals(HouseEntry other)
            {
                if (ReferenceEquals(this, other)) {
                    return true;
                }
                return false;
            }

            public override bool Equals(object obj)
            {
                if (obj == null) {
                    return false;
                }
                if (ReferenceEquals(this, obj)) {
                    return true;
                }
                return false;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 23 + EnergyIntensityType.GetHashCode();
                    hash = hash * 23 + HouseholdList.GetHashCode();
                    hash = hash * 23 + HouseSize.GetHashCode();
                    hash = hash * 23 + MaximumHouseSize.GetHashCode();
                    return hash * 23 + Number.GetHashCode();
                }
            }
        }
    }
}