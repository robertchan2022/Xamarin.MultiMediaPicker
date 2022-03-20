using MultiMediaPickerSample.Models;
using MultiMediaPickerSample.ViewModels;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MultiMediaPickerSample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public void ScrollToTop()
        {
            var vm = (this.BindingContext as MainViewModel);
            listView.ScrollTo(vm.Media[0], ScrollToPosition.Start, true);
        }

        public void UpdateFileSelectionStatus()
        {
            var vm = (this.BindingContext as MainViewModel);
            lblSelectedFileCount.Text = vm.Media.Where(w => w.IsSelected).Count().ToString();
            lblTotalFileCount.Text = vm.Media.Count().ToString();

            listViewBorderTop.IsVisible = vm.Media.Count() > 0;
            lblNoFileSelected.IsVisible = vm.Media.Count() == 0;
            chkSelectAll.IsVisible = vm.Media.Count() > 0;
        }

        private void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var selectedItem = e.Item as MediaFile;
            selectedItem.IsSelected = !selectedItem.IsSelected;
        }

        private void listView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            base.OnAppearing();
            
            //var vm = (this.BindingContext as MainViewModel);
            //listView.HeightRequest = (vm.Media.Count * 130) + 80;
        }

        private void btnPreview_Pressed(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Grid parent = (Grid)button.Parent;
            var selectedItem = parent.BindingContext as MediaFile;

            PopupNavigation.Instance.PushAsync(new MediaPopupPage(selectedItem));
            //Navigation.PushPopupAsync(new MediaPopupPage());
        }

        private void btnSubmit_Pressed(object sender, EventArgs e)
        {
            var vm = (this.BindingContext as MainViewModel);
            var selectedItems = vm.Media.Select(s => s.IsSelected).ToList();

            if (selectedItems.Count > 6)
            {
                this.DisplayAlert("Attachment upload limit reached...", "Maximum of 6 attachments can be uploaded.", "Dismiss");
            }
        }

        private void ChkSelectAll_IsCheckedChanged(object sender, TappedEventArgs e)
        {
            var vm = (this.BindingContext as MainViewModel);
            var model = vm.Media;

            foreach (var item in model)
            {
                item.IsSelected = (bool)e.Parameter;
            }
        }
    }
}
