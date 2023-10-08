﻿#nullable enable

using Microsoft.Toolkit.Uwp.UI;
using Screenbox.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Uwp.Helpers;
using Screenbox.Pages;
using Windows.System;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit;
using ReswPlusLib;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Screenbox.Controls
{
    public sealed partial class PlaylistView : UserControl
    {
        private DispatcherQueue? dispatcherQueue;

        public static readonly DependencyProperty IsFlyoutProperty = DependencyProperty.Register(
            "IsFlyout",
            typeof(bool),
            typeof(PlaylistView),
            new PropertyMetadata(false));

        public bool IsFlyout
        {
            get => (bool)GetValue(IsFlyoutProperty);
            set => SetValue(IsFlyoutProperty, value);
        }

        internal PlaylistViewModel ViewModel => (PlaylistViewModel)DataContext;

        internal CommonViewModel Common { get; }

        public PlaylistView()
        {
            this.InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<PlaylistViewModel>();
            Common = Ioc.Default.GetRequiredService<CommonViewModel>();

            ViewModel.ScrollActiveItemIntoView += ViewModel_ScrollingActiveItemIntoView;
        }

        private void ViewModel_ScrollingActiveItemIntoView(PlaylistViewModel sender, object args)
        {
            Task.Run(async () =>
            {
                await dispatcherQueue.EnqueueAsync(async () => await SmoothScrollActiveItemIntoViewAsync());
            });
        }

        public async Task SmoothScrollActiveItemIntoViewAsync()
        {
            if (ViewModel.Playlist.CurrentItem == null || !ViewModel.HasItems) return;
            await PlaylistListView.SmoothScrollIntoViewWithItemAsync(ViewModel.Playlist.CurrentItem, ScrollItemPlacement.Center);

            ViewModel.ShouldScrollActiveItemIntoView = false;
        }

        private void SelectionCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectionCheckBox.IsChecked ?? false)
            {
                PlaylistListView.SelectAll();
            }
            else
            {
                PlaylistListView.SelectedItems.Clear();
            }
        }

        private void PlaylistListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionCheckBox.IsChecked = PlaylistListView.SelectedItems.Count == ViewModel.Playlist.Items.Count;
            if (ViewModel.EnableMultiSelect)
            {
                VisualStateManager.GoToState(this,
                    PlaylistListView.SelectedItems.Count == 1 ? "MultipleSingleSelected" : "Multiple", true);
            }

            ViewModel.SelectionCount = PlaylistListView.SelectedItems.Count;
        }

        private async void PlaylistListView_OnDrop(object sender, DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;
            e.Handled = true;
            IReadOnlyList<IStorageItem>? items = await e.DataView.GetStorageItemsAsync();
            if (items?.Count > 0)
            {
                ViewModel.EnqueuePlaylist(items);
            }
        }

        private void PlaylistListView_OnDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.AcceptedOperation = e.DataView.Contains(StandardDataFormats.StorageItems)
                ? DataPackageOperation.Copy
                : DataPackageOperation.None;
            if (e.DragUIOverride != null)
            {
                e.DragUIOverride.Caption = Strings.Resources.AddToQueue;
            }
        }

        private void CommandBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateLayoutState();
        }

        private void UpdateLayoutState()
        {
            if (IsFlyout)
            {
                VisualStateManager.GoToState(this, "Minimal", true);
                return;
            }

            VisualStateManager.GoToState(this, SelectionCommandBar.ActualWidth <= 620 ? "Compact" : "Normal", true);
        }

        private void GoToCurrentItem()
        {
            if (ViewModel.Playlist.CurrentItem != null)
            {
                PlaylistListView.SmoothScrollIntoViewWithItemAsync(ViewModel.Playlist.CurrentItem, ScrollItemPlacement.Center);
                (PlaylistListView.ContainerFromItem(ViewModel.Playlist.CurrentItem) as Control)?.Focus(FocusState.Programmatic);
            }
        }

        private void PlaylistView_OnLoaded(object sender, RoutedEventArgs e)
        {
            dispatcherQueue = MainPage.DispatcherQueue;

            UpdateLayoutState();
            GoToCurrentItem();
        }
    }
}
