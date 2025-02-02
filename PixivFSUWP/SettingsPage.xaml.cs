﻿using PixivFSUWP.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static PixivFSUWP.Data.OverAll;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace PixivFSUWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page, IGoBackFlag
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            _ = loadContentsAsync();
        }

        private bool _backflag { get; set; } = false;

        public void SetBackFlag(bool value)
        {
            _backflag = value;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ((Frame.Parent as Grid)?.Parent as MainPage)?.SelectNavPlaceholder(GetResourceString("SettingsPagePlain"));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (!_backflag)
            {
                Data.Backstack.Default.Push(typeof(SettingsPage), null);
                ((Frame.Parent as Grid)?.Parent as MainPage)?.UpdateNavButtonState();
            }
        }

        async Task loadContentsAsync()
        {
            var imgTask = LoadImageAsync(currentUser.Avatar170);
            txtVersion.Text = string.Format("版本：{0} version-{1}.{2}.{3} {4}",
                Package.Current.DisplayName,
                Package.Current.Id.Version.Major,
                Package.Current.Id.Version.Minor,
                Package.Current.Id.Version.Build,
                Package.Current.Id.Architecture);
            txtPkgName.Text = string.Format("包名：{0}", Package.Current.Id.Name);
            txtInsDate.Text = string.Format("时间：{0}", Package.Current.InstalledDate.ToLocalTime().DateTime);
            txtID.Text = currentUser.ID.ToString();
            txtName.Text = currentUser.Username;
            txtAccount.Text = "@" + currentUser.UserAccount;
            txtEmail.Text = currentUser.Email;
            imgAvatar.ImageSource = await imgTask;
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var myPictures = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            Windows.Storage.StorageFolder savePicturesFolder = myPictures.SaveFolder;
            txtWallpaperFolder.Text = localSettings.Values["WallpaperFolder"] as string ?? savePicturesFolder.Path;
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            txtAppDataFolder.Text = localFolder.Path;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var vault = new PasswordVault();
            try
            {
                vault.Remove(GetCredentialFromLocker(passwordResource));
                vault.Remove(GetCredentialFromLocker(refreshTokenResource));
            }
            catch { }
            finally
            {
                ((Frame.Parent as Grid).Parent as MainPage).Frame.Navigate(typeof(LoginPage));
            }
        }

        private void TxtWallpaperFolder_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SaveWallpaperFolder(txtWallpaperFolder.Text);
        }

        private void SaveWallpaperFolder(string path)
        {
// Save a setting locally on the device
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["WallpaperFolder"] = path;
        }

        private void BtnOpenFolder_OnClick(object sender, RoutedEventArgs e)
        {
            _ = PickWallpaperFolder();
        }

        private async Task PickWallpaperFolder()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("WallpaperFolder",
                    folder);
                txtWallpaperFolder.Text = folder.Path;
                SaveWallpaperFolder(folder.Path);
            }
        }

        private async void BtnGithub_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new
                Uri(@"https://github.com/tobiichiamane/pixivfs-uwp"));
        }
    }
}
