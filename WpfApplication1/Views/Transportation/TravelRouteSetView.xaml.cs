﻿using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using Common;
using Database.Tables.BasicElements;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Views.Transportation {
    /// <summary>
    ///     Interaktionslogik für DeviceActionView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class TravelRouteSetView {
        public TravelRouteSetView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private TravelRouteSetPresenter Presenter => (TravelRouteSetPresenter) DataContext;

        private void BtnAddDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbTravelRoutes.SelectedItem == null) {
                Logger.Error("Please select a travel route first!");
                return;
            }

            if (!(CmbTravelRoutes.SelectedItem is TravelRoute tp)) {
                Logger.Error("Please select a travel route first!");
                return;
            }

            Presenter.ThisRouteSet.AddRoute(tp);
            Presenter.RefreshRoutes();
        }

        private void BtnFindMissingSitesClick([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            Presenter.FindMissingRoutesAndCreateThem();
        }

        private void BtnRefreshRoutes([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            Presenter.RefreshRoutes();
        }

        private void BtnRefreshSitesClick([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            Presenter.RefreshDataTable();
        }

        private void BtnRefreshUsedIn_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            TravelRouteSetPresenter.RefreshUsedIn();

        private void BtnRemoveRoutesClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstSteps.SelectedItem == null) {
                Logger.Warning("Please select a step first");
                return;
            }
            if (!(LstSteps.SelectedItem is TravelRoute dap)) {
                Logger.Error("Please select a step first!");
                return;
            }

            Presenter.ThisRouteSet.DeleteEntry(dap);
            Presenter.RefreshRoutes();
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void DeleteClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisRouteSet.Name, Presenter.Delete);

        private void LstPersonDesiresMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstPersonDesires.SelectedItem == null) {
                return;
            }

            var ui = (UsedIn) LstPersonDesires.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(ui.Item);
        }

        private void LstSteps_OnMouseDoubleClick([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] MouseButtonEventArgs e)
        {
            if (LstSteps.SelectedItem == null) {
                return;
            }

            var entry = (TravelRoute) LstSteps.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(entry);
        }

        private void BtnFindMissingSitesForAllHouseholdsClick([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            Presenter.FindMissingRoutesForAllHouseholds();
        }

        private void BtnMakeCopy(object sender, RoutedEventArgs e)
        {
            Presenter.MakeCopy();
        }

        private void BtnRemoveWorkplaceRoutes(object sender, RoutedEventArgs e)
        {
            Presenter.RemoveWorkplaceRoutes();
            Presenter.RefreshRoutes();
        }

        private void BtnAddDistanceWorkplaceRoute(object sender, RoutedEventArgs e)
        {
            Presenter.AddDistanceMatchingWorkplaceRoutes();
            Presenter.RefreshRoutes();
        }
    }
}