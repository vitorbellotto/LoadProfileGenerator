﻿//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class DeviceSelection : DBBaseElement {
        public const string TableName = "tblDeviceSelections";
        [ItemNotNull] [NotNull] private readonly ObservableCollection<DeviceSelectionDeviceAction> _selectionActions;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<DeviceSelectionItem> _selectionItems;
        [CanBeNull] private string _description;

        public DeviceSelection([NotNull] string pName, [CanBeNull]int? id, [CanBeNull] string description,
                               [NotNull] string connectionString, [NotNull] StrGuid guid)
            : base(pName, TableName, connectionString, guid)
        {
            ID = id;
            TypeDescription = "Device Selection";
            _description = description;
            _selectionItems = new ObservableCollection<DeviceSelectionItem>();
            _selectionActions = new ObservableCollection<DeviceSelectionDeviceAction>();
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<DeviceSelectionDeviceAction> Actions => _selectionActions;

        [CanBeNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<DeviceSelectionItem> Items => _selectionItems;

        public void AddAction([NotNull] DeviceActionGroup dag, [NotNull] DeviceAction da)
        {
            var dsi =
                _selectionActions.FirstOrDefault(dagroup => dagroup.DeviceActionGroup == dag);
            if (dsi != null) {
                if (dsi.DeviceAction != da) {
                    dsi.DeleteFromDB();
                }
                else {
                    return;
                }
            }
            var newdsi = new DeviceSelectionDeviceAction(null, IntID,
                dag, da, ConnectionString,dag.Name, System.Guid.NewGuid().ToStrGuid());
            _selectionActions.Add(newdsi);
            _selectionActions.Sort();
            SaveToDB();
        }

        public void AddItem([NotNull] DeviceCategory dc, [NotNull] RealDevice rd)
        {
            var dsi = _selectionItems.FirstOrDefault(devcat => devcat.DeviceCategory == dc);
            if (dsi != null) {
                if (dsi.Device != rd) {
                    dsi.DeleteFromDB();
                }
                else {
                    return;
                }
            }
            var newdsi = new DeviceSelectionItem(null, IntID, dc,
                rd, ConnectionString, dc.Name, System.Guid.NewGuid().ToStrGuid());
            _selectionItems.Add(newdsi);
            _selectionItems.Sort();
            SaveToDB();
        }

        [NotNull]
        private static DeviceSelection AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var hhid =dr.GetIntFromLong("ID");
            var name =  dr.GetString("Name","(no name)");
            var description = dr.GetString("Description", false);
            var guid = GetGuid(dr, ignoreMissingFields);

            var chh = new DeviceSelection(name, hhid, description, connectionString, guid);
            return chh;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new
            DeviceSelection(FindNewName(isNameTaken, "New Device Selection "),
                null, "(no description yet)",
                connectionString, System.Guid.NewGuid().ToStrGuid());

        public void DeleteActionFromDB([NotNull] DeviceSelectionDeviceAction dsi)
        {
            dsi.DeleteFromDB();
            _selectionActions.Remove(dsi);
        }

        public override void DeleteFromDB()
        {
            foreach (var item in _selectionItems) {
                item.DeleteFromDB();
            }
            foreach (var item in _selectionActions) {
                item.DeleteFromDB();
            }
            base.DeleteFromDB();
        }

        public void DeleteItemFromDB([NotNull] DeviceSelectionItem dsi)
        {
            dsi.DeleteFromDB();
            _selectionItems.Remove(dsi);
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([NotNull] DeviceSelection otherDeviceSelection,
            [NotNull] Simulator dstSim)
        {
            var deviceSelection = new DeviceSelection(otherDeviceSelection.Name, null,
                otherDeviceSelection.Description, dstSim.ConnectionString,otherDeviceSelection.Guid);
            deviceSelection.SaveToDB();
            foreach (var item in otherDeviceSelection._selectionItems) {
                var rd = GetItemFromListByName(dstSim.RealDevices.MyItems, item.Device.Name);
                var dc = GetItemFromListByName(dstSim.DeviceCategories.MyItems, item.DeviceCategory.Name);
                if (rd == null || dc == null) {
                    throw new LPGException("Device selection import failed!");
                }
                deviceSelection.AddItem(dc, rd);
            }
            foreach (var item in otherDeviceSelection.Actions) {
                var da = GetItemFromListByName(dstSim.DeviceActions.MyItems, item.DeviceAction.Name);
                var dag = GetItemFromListByName(dstSim.DeviceActionGroups.MyItems,
                    item.DeviceActionGroup.Name);
                if (da == null || dag == null) {
                    throw new LPGException("Device selection import failed!");
                }
                deviceSelection.AddAction(dag, da);
            }
            deviceSelection.SaveToDB();
            return deviceSelection;
        }

        private static bool IsCorrectDeviceSelectionActionParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var dsi = (DeviceSelectionDeviceAction) child;
            if (parent.ID == dsi.DeviceSelectionID) {
                var ds = (DeviceSelection) parent;
                ds._selectionActions.Add(dsi);
                return true;
            }
            return false;
        }

        private static bool IsCorrectDeviceSelectionItemParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var dsi = (DeviceSelectionItem) child;
            if (parent.ID == dsi.DeviceSelectionID) {
                var ds = (DeviceSelection) parent;
                ds._selectionItems.Add(dsi);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<DeviceSelection> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<DeviceCategory> deviceCategories, [ItemNotNull] [NotNull] ObservableCollection<RealDevice> devices,
            [ItemNotNull] [NotNull] ObservableCollection<DeviceAction> deviceActions,
            [ItemNotNull] [NotNull] ObservableCollection<DeviceActionGroup> deviceActionGroups,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var dsis = new ObservableCollection<DeviceSelectionItem>();
            DeviceSelectionItem.LoadFromDatabase(dsis, connectionString, deviceCategories, devices,
                ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(dsis), IsCorrectDeviceSelectionItemParent,
                ignoreMissingTables);
            var dsda =
                new ObservableCollection<DeviceSelectionDeviceAction>();
            DeviceSelectionDeviceAction.LoadFromDatabase(dsda, connectionString, deviceActions, deviceActionGroups,
                ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(dsda), IsCorrectDeviceSelectionActionParent,
                ignoreMissingTables);
            // sort
            foreach (var chht in result) {
                chht._selectionItems.Sort();
            }
            // cleanup
            result.Sort();
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var item in _selectionItems) {
                item.SaveToDB();
            }
            foreach (var item in _selectionActions) {
                item.SaveToDB();
            }
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Name", "@myname", Name);
            if(_description!=null) {
                cmd.AddParameter("Description", "@Description", _description);
            }
        }

        [NotNull]
        public override DBBase ImportFromGenericItem([NotNull] DBBase toImport, [NotNull] Simulator dstSim)
            => ImportFromItem((DeviceSelection)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}