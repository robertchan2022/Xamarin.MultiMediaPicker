using MultiMediaPickerSample.Models;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiMediaPickerSample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MediaPopupPage : PopupPage
    {
        private List<MediaFile> _mediaFiles = new List<MediaFile>();
        private int _index = 0;

        public MediaPopupPage(List<MediaFile> mediaFiles, int index)
        {
            InitializeComponent();

            _mediaFiles = mediaFiles;
            _index = index;
            
            var mediaFile = mediaFiles[index];
            this.BindingContext = mediaFile;

            

            //var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;

            //// Orientation (Landscape, Portrait, Square, Unknown)
            //var orientation = mainDisplayInfo.Orientation;

            //// Rotation (0, 90, 180, 270)
            //var rotation = mainDisplayInfo.Rotation;

            //// Width (in pixels)
            //var width = mainDisplayInfo.Width;

            //// Width (in xamarin.forms units)
            //var xamarinWidth = width / mainDisplayInfo.Density;

            //// Height (in pixels)
            //var height = mainDisplayInfo.Height;

            //// Screen density
            //var density = mainDisplayInfo.Density;

            //popupContainer.WidthRequest = width * 0.4;
            //popupContainer.HeightRequest = height * 0.7;
        }

        private void OnCloseButtonTapped(object sender, EventArgs e)
        {
            CloseAllPopup();
        }

        private async void CloseAllPopup()
        {
            await PopupNavigation.Instance.PopAllAsync();
        }

        private void OnPreviousButtonTapped(object sender, EventArgs e)
        {
            if (_index > 0)
            {
                _index -= 1;
            }

            var mediaFile = _mediaFiles[_index];
            this.BindingContext = mediaFile;
        }

        private void OnNextButtonTapped(object sender, EventArgs e)
        {
            if (_index < _mediaFiles.Count - 1)
            {
                _index += 1;
            }

            var mediaFile = _mediaFiles[_index];
            this.BindingContext = mediaFile;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            //imageViewer.BatchBegin();
            //imageViewer.
            //imageViewer.BatchCommit();
        }
    }
}