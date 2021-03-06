﻿using Template10.Mvvm;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight.Command;
using Tweetinvi;
using System;
using Windows.UI.Xaml;
using TwitterDotNet.Services.AccountManager;
using Windows.Storage;
using System.Diagnostics;

namespace TwitterDotNet.ViewModels
{
    class LoginPageViewModel : ViewModelBase
    {
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            Views.Busy.SetBusy(true, "Veuillez patienter ...");

            var fileName = "accounts.json";
            var folderUsed = ApplicationData.Current.LocalFolder;

            try
            {
                var fileUsed = await folderUsed.GetFileAsync(fileName);
                if (fileUsed.IsAvailable)
                {
                    Views.Busy.SetBusy(true, "Reconnexion en cours ...");
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    AutoConnect();
                }
            }
            catch (System.IO.FileNotFoundException e)
            {
                WebViewUriSource = new Uri(parameter.ToString());
            }


            PinCodeTextBoxVisibility = Visibility.Visible;
            ValidateButtonVisibility = Visibility.Visible;

            Views.Busy.SetBusy(false);

            await Task.CompletedTask;
        }

        private Visibility _pinCodeTextBoxVisibility = Visibility.Collapsed;
        private Visibility _validateButtonVisibility = Visibility.Collapsed;
        public Visibility PinCodeTextBoxVisibility { get { return _pinCodeTextBoxVisibility; } set { _pinCodeTextBoxVisibility = value; } }
        public Visibility ValidateButtonVisibility { get { return _validateButtonVisibility; } set { _validateButtonVisibility = value; } }
        
        private Uri _webviewUriSource;
        public Uri WebViewUriSource { get { return _webviewUriSource; } set { _webviewUriSource = value; RaisePropertyChanged(); } }
        
        private RelayCommand _validatePinCodeCommand;
        public RelayCommand ValidatePinCodeCommand
        {
            get
            {
                if (_validatePinCodeCommand == null)
                    _validatePinCodeCommand = new RelayCommand(ValidatePinCode);

                return _validatePinCodeCommand;
            }
            set { _validatePinCodeCommand = value; }
        }

        private string _pinCode;
        public string PinCode { get { return _pinCode; } set { _pinCode = value; RaisePropertyChanged(); } }

        private async void ValidatePinCode()
        {
            if (!String.IsNullOrEmpty(PinCode))
            {
                var userCredentials = CredentialsCreator.GetCredentialsFromVerifierCode(PinCode, MainPageViewModel.AppCredentials);
                Auth.SetCredentials(userCredentials);

                if (Auth.ApplicationCredentials != null)
                {
                    var loggedUser = User.GetAuthenticatedUser();

                    if (!String.IsNullOrEmpty(loggedUser.ScreenName))
                    {

                        MainPageViewModel.TweetinviData.AccessToken = loggedUser.Credentials.AccessToken;
                        MainPageViewModel.TweetinviData.AccessTokenSecret = loggedUser.Credentials.AccessTokenSecret;

                        AccountManager ac = new AccountManager();
                        ac.CreateJsonData();
                        await ac.SaveDataToFile();

                        NavigationService.Navigate(typeof(Views.HomeTimelinePage));
                    }
                }
            }
        }

        private async void AutoConnect()
        {
            AccountManager ac = new AccountManager();
            await ac.LoadDataFromFile();
            ac.LoadAccountDataFromJson();

            Auth.SetUserCredentials(
                MainPageViewModel.TweetinviData.ConsumerKey,
                MainPageViewModel.TweetinviData.ConsumerSecret,
                ac.AccountData.AccountAccessToken,
                ac.AccountData.AccountAccessTokenSecret
            );

            if (Auth.ApplicationCredentials != null)
            {
                try
                {
                    var loggedUser = User.GetAuthenticatedUser();
                    if (!String.IsNullOrEmpty(loggedUser.ScreenName))
                        NavigationService.Navigate(typeof(Views.HomeTimelinePage));
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }
    }
}
