using AForge.Video;
using AForge.Video.DirectShow;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ImageCapture
{
    public class ShellViewModel : BindableBase
    {
        private FilterInfoCollection devices;
        private FilterInfo currentDevice;

        private BitmapImage image;

        private IVideoSource videoSource;

        public ICommand TakeSnapshot { get; private set; }

        public BitmapImage Image
        {
            get { return image; }
            set { SetProperty(ref image, value); }
        }

        public FilterInfoCollection Devices
        {
            get { return devices; }
            set { SetProperty(ref devices, value); }
        }

        public FilterInfo CurrentDevice
        {
            get { return currentDevice; }
            set { SetProperty(ref currentDevice, value); }
        }

        public ShellViewModel()
        {
            TakeSnapshot = new DelegateCommand(OnTakeSnapshot);

            GetVideoDevices();
            StartCapture();
        }

        private void GetVideoDevices()
        {
            Devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (Devices.Count > 0)
            {
                if (Devices.Count == 1)
                {
                    CurrentDevice = Devices[0];
                }
                else
                {
                    CurrentDevice = Devices[1];
                }
            }
            else
            {
                MessageBox.Show("Es wurde keine Kamera gefunden");
            }
        }

        private void StartCapture()
        {
            if (CurrentDevice != null)
            {
                videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                videoSource.NewFrame += video_NewFrame;
                videoSource.Start();
            }
            else
            {
                MessageBox.Show("Current device can't be null");
            }
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    Image = bitmap.ToBitmapImage();
                }
                Image.Freeze(); // avoid cross thread operations and prevents leaks
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on videoSource_NewFrame:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopCamera();
            }
        }

        private void StopCamera()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.Stop();
                videoSource.NewFrame -= video_NewFrame;
            }
            //Image = null; // snapshot should be held in frame
        }

        private void OnTakeSnapshot()
        {
            try
            {
                StopCamera();
                Image.Freeze();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Bitte warten sie, bis die Kamera initialisiert wurde und versuchen sie es erneut.");
                StartCapture();
            }
        }
    }
}
