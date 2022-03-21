using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace MultiMediaPickerSample.Models
{
    public enum MediaFileType
    {
        Image,
        Video,
        Pdf
    }

    public class MediaFile : INotifyPropertyChanged
    {
        public string ThumbnailPath { get; set; }
        public byte[] FileBytes { get; set; }
        public string Path { get; set; }
        public MediaFileType Type { get; set; }

        private bool _isSelected = false;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                (Application.Current.MainPage as MainPage).UpdateFileSelectionStatus();
                OnPropertyChanged("IsSelected");
            }
        }
        public string FileName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Path))
                {
                    return System.IO.Path.GetFileName(Path);
                }

                return "";
            }
        }

        public string TypeName => Type.ToString();

        public string FileSize => $"{Math.Round((double)FileBytes.Length / 1024).ToString()} KB";

        public bool IsImage => Type == MediaFileType.Image;

        public bool IsVideo => Type == MediaFileType.Video;

        public event PropertyChangedEventHandler PropertyChanged;
        bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
