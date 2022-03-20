using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.Graphics.Pdf;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Widget;
using Java.IO;
using MultiMediaPickerSample.Helpers;
using MultiMediaPickerSample.Models;
using MultiMediaPickerSample.Services;
using Plugin.CurrentActivity;
using Xamarin.Essentials;
using static Android.Graphics.Pdf.PdfRenderer;

namespace MultiMediaPickerSample.Droid.Services
{
    public class MultiMediaPickerService : IMultiMediaPickerService
    {
        public static MultiMediaPickerService SharedInstance = new MultiMediaPickerService();
        const string tempFolder = "TmpMedia";

        MultiMediaPickerService()
        {
            Clean();
        }

        public event EventHandler<MediaFile> OnMediaPicked;
        public event EventHandler<IList<MediaFile>> OnMediaPickedCompleted;

        TaskCompletionSource<IList<MediaFile>> mediaPickedTcs;

        public async Task<IList<MediaFile>> PickPhotosAsync()
        {
            mediaPickedTcs = new TaskCompletionSource<IList<MediaFile>>();
            var mediaPicked = await pickMultipleFilesAsync(MediaFileType.Image);

            foreach (var media in mediaPicked)
            {
                OnMediaPicked?.Invoke(this, media);
            }

            OnMediaPickedCompleted?.Invoke(this, mediaPicked);
            mediaPickedTcs?.TrySetResult(mediaPicked);

            return await mediaPickedTcs.Task;
        }

        public async Task<IList<MediaFile>> PickVideosAsync()
        {
            mediaPickedTcs = new TaskCompletionSource<IList<MediaFile>>();
            var mediaPicked = await pickMultipleFilesAsync(MediaFileType.Video);

            foreach (var media in mediaPicked)
            {
                OnMediaPicked?.Invoke(this, media);
            }

            OnMediaPickedCompleted?.Invoke(this, mediaPicked);
            mediaPickedTcs?.TrySetResult(mediaPicked);

            return await mediaPickedTcs.Task;
        }

        public async Task<IList<MediaFile>> PickFilesAsync()
        {
            mediaPickedTcs = new TaskCompletionSource<IList<MediaFile>>();
            var mediaPicked = await pickMultipleFilesAsync(MediaFileType.Pdf);

            foreach (var media in mediaPicked)
            {
                OnMediaPicked?.Invoke(this, media);
            }

            OnMediaPickedCompleted?.Invoke(this, mediaPicked);
            mediaPickedTcs?.TrySetResult(mediaPicked);

            return await mediaPickedTcs.Task;
        }

        public async Task<IList<MediaFile>> CaptureImagesAsync()
        {
            mediaPickedTcs = new TaskCompletionSource<IList<MediaFile>>();

            var fileResult = await MediaPicker.CapturePhotoAsync();

            if (fileResult != null)
            {
                var media = await CreateMediaFileFromFileResult(MediaFileType.Image, fileResult, true);
                OnMediaPicked?.Invoke(this, media);

                OnMediaPickedCompleted?.Invoke(this, new List<MediaFile> { media });
                mediaPickedTcs?.TrySetResult(new List<MediaFile> { media });
            }

            return await mediaPickedTcs.Task;
        }

        public void Clean()
        {
            var mediaDirectory = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), tempFolder);

            if (Directory.Exists(mediaDirectory))
            {
                Directory.Delete(mediaDirectory, true);
            }
        }

        public void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {

        }




        #region Private Functions
        private async Task<ObservableCollection<MediaFile>> pickMultipleFilesAsync(MediaFileType mediaType)
        {
            string pickerTitle = null;
            FilePickerFileType fileTypes = null;

            if (mediaType == MediaFileType.Image)
            {
                pickerTitle = "Please select photo(s)";
                fileTypes = FilePickerFileType.Images;
            }
            else if (mediaType == MediaFileType.Video)
            {
                pickerTitle = "Please select video(s)";
                fileTypes = FilePickerFileType.Videos;
            }
            else if (mediaType == MediaFileType.Pdf)
            {
                pickerTitle = "Please select pdf(s)";
                fileTypes = FilePickerFileType.Pdf;
            }

            var options = new PickOptions
            {
                PickerTitle = pickerTitle,
                FileTypes = fileTypes,
            };

            var mediaPicked = new ObservableCollection<MediaFile>();
            var fileResults = await FilePicker.PickMultipleAsync(options);

            if (fileResults != null)
            {
                foreach (var fileResult in fileResults)
                {
                    MediaFile media = await CreateMediaFileFromFileResult(mediaType, fileResult);

                    mediaPicked.Add(media);
                    OnMediaPicked?.Invoke(this, media);
                }
            }

            return mediaPicked;
        }

        private async Task<MediaFile> CreateMediaFileFromFileResult(MediaFileType mediaFileType, FileResult fileResult, bool isCaptureImage = false)
        {
            string fullPath = fileResult.FullPath;

            var fullFileStream = await fileResult.OpenReadAsync();
            var fullFileBytes = Array.Empty<byte>();
            var thumbnailFileBytes = Array.Empty<byte>();

            if (isCaptureImage == false)
            {    
                fullFileBytes = ConvertToBytes(fullFileStream);

                thumbnailFileBytes = await GenerateThumbnailFromFileResult(mediaFileType, fileResult);
            }
            else
            {
                var tempFilePath = CopyFileToLocalPath(fullPath);
                fullFileBytes = Helpers.ImageHelpers.RotateImage(tempFilePath, 1);
                var fullFileBitmap = BitmapFactory.DecodeByteArray(fullFileBytes, 0, fullFileBytes.Length);
                var thumbnailBitmap = ThumbnailUtils.ExtractThumbnail(fullFileBitmap, 100, 100);
                thumbnailFileBytes = GetThumbnailBytesFromBitmap(thumbnailBitmap);

            }

            var thumbnailFilePath = GetNewLocalFilePath(fullPath);
            await CopyFileToLocalPath(
                path: thumbnailFilePath,
                fileBytes: thumbnailFileBytes
            );

            var media = new MediaFile()
            {
                Path = fullPath,
                Type = mediaFileType,
                FileBytes = fullFileBytes,
                ThumbnailPath = thumbnailFilePath
            };

            return media;
        }

        private ParcelFileDescriptor GetSeekableFileDescriptor(string filePath)
        {
            ParcelFileDescriptor fileDescriptor = null;
            try
            {
                fileDescriptor = ParcelFileDescriptor.Open(new Java.IO.File(filePath), ParcelFileMode.ReadOnly);
            }
            catch (Java.IO.FileNotFoundException e)
            {

            }

            return fileDescriptor;
        }

        private byte[] ConvertToBytes(System.IO.Stream input)
        {
            using MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }

        private async Task<byte[]> GenerateThumbnailFromFileResult(MediaFileType mediaFileType, FileResult fileResult)
        {
            var stream = await fileResult.OpenReadAsync();
            var fileBytes = ConvertToBytes(stream);

            Bitmap fileBitmap = null;
            var thumbnailFileBytes = Array.Empty<byte>();

            if (mediaFileType == MediaFileType.Image)
            {
                fileBitmap = await BitmapFactory.DecodeByteArrayAsync(fileBytes, 0, fileBytes.Length);
                
            }
            else if (mediaFileType == MediaFileType.Video)
            {
                var clonedFilePath = CopyFileToLocalPath(fileResult.FullPath);
                fileBitmap = await ThumbnailUtils.CreateVideoThumbnailAsync(clonedFilePath, ThumbnailKind.MiniKind);
                System.IO.File.Delete(clonedFilePath);
            }
            else if (mediaFileType == MediaFileType.Pdf)
            {
                PdfRenderer renderer = new PdfRenderer(GetSeekableFileDescriptor(fileResult.FullPath));
                Page page = renderer.OpenPage(0);

                //Creates bitmap
                fileBitmap = Bitmap.CreateBitmap(page.Width, page.Height, Bitmap.Config.Argb8888, true);

                //renderes page as bitmap, to use portion of the page use second and third parameter
                page.Render(fileBitmap, null, null, PdfRenderMode.ForDisplay);

                page.Close();
                renderer.Close();
            }

            var fileThumbnailBmp = ThumbnailUtils.ExtractThumbnail(fileBitmap, 100, 100);
            thumbnailFileBytes = GetThumbnailBytesFromBitmap(fileThumbnailBmp);

            return thumbnailFileBytes;
        }

        private string GetNewLocalFilePath(string fileFullPath)
        {
            var destinationFilePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), tempFolder);
            var fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileFullPath);
            var fileExt = System.IO.Path.GetExtension(fileFullPath);

            Directory.CreateDirectory(destinationFilePath);
            string timestamp = DateTime.Now.ToString("yyyMMdd_HHmmss");
            destinationFilePath = System.IO.Path.Combine(destinationFilePath, $"{fileNameWithoutExt}_{timestamp}{fileExt}");

            return destinationFilePath;
        }

        private string CopyFileToLocalPath(string fileFullPath)
        {
            var destinationFilePath = GetNewLocalFilePath(fileFullPath);
            System.IO.File.Copy(fileFullPath, destinationFilePath);
            return destinationFilePath;
        }

        private async Task CopyFileToLocalPath(string path, byte[] fileBytes) => await System.IO.File.WriteAllBytesAsync(path, fileBytes);

        private byte[] GetThumbnailBytesFromBitmap(Bitmap thumbnailBitmap)
        {
            using MemoryStream ms = new MemoryStream();
            thumbnailBitmap.Compress(Bitmap.CompressFormat.Png, 50, ms);
            var thumbnailFileBytes = ms.ToArray();

            return thumbnailFileBytes;
        }

        #endregion
    }
}
