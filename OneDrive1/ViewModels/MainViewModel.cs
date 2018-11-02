using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.WindowsForms;

namespace OneDrive1.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        #region フィールド

        private IOneDriveClient _OneDriveClient;

        private const string ClientId = "クライアントIDを入力";

        private const string ReturnUrl = "https://login.live.com/oauth20_desktop.srf";

        private static readonly string[] Scopes =
        {
            "onedrive.readwrite",
            "wl.offline_access",
            "wl.signin"
        };

        #endregion

        #region コンストラクタ

        public MainViewModel()
        {
            this.SingInCommand = new RelayCommand(this.ExecuteSingIn, CanExecuteSingIn);
        }

        #endregion

        #region プロパティ

        public RelayCommand SingInCommand
        {
            get;
            private set;
        }

        #endregion

        #region メソッド

        #region ヘルパーメソッド

        private bool CanExecuteSingIn()
        {
            return true;
        }

        private async void ExecuteSingIn()
        {
            if (this._OneDriveClient == null)
            {
                this._OneDriveClient = OneDriveClient.GetMicrosoftAccountClient(
                    ClientId,
                    ReturnUrl,
                    Scopes,
                    webAuthenticationUi: new FormsWebAuthenticationUi());
            }

            try
            {
                if (!this._OneDriveClient.IsAuthenticated)
                {
                    await this._OneDriveClient.AuthenticateAsync();
                }

                if (this._OneDriveClient.IsAuthenticated)
                {
                    MessageBox.Show(
                        "Authentication was successful",
                        "Authentication was successful",
                        MessageBoxButton.OK);

                   // var expandValue =  ClientType.Consumer
                   //? "thumbnails,children($expand=thumbnails)"
                   //: "thumbnails,children";

                    var rootItem = await _OneDriveClient
                         .Drive
                         .Root
                         .Request().Expand("thumbnails,children")
                         .GetAsync();

                    foreach (var item in rootItem.Children.CurrentPage)
                    {
                        System.Console.WriteLine(item.Name);
                    }



                }
            }
            catch (OneDriveException exception)
            {
                if (!exception.IsMatch(OneDriveErrorCode.AuthenticationCancelled.ToString()))
                {
                    if (exception.IsMatch(OneDriveErrorCode.AuthenticationFailure.ToString()))
                    {
                        MessageBox.Show(
                            "Authentication failed",
                            "Authentication failed",
                            MessageBoxButton.OK);

                        var httpProvider = this._OneDriveClient.HttpProvider as HttpProvider;
                        if (httpProvider != null)
                        {
                            httpProvider.Dispose();
                        }

                        this._OneDriveClient = null;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        #endregion

        #endregion

    }
}