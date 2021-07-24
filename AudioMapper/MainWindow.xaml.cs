using AudioMapper.Controllers;
using AudioMapper.Helpers;
using AudioMapper.Models;
using MaterialDesignExtensions.Controls;
using System;
using System.Windows;
using System.Windows.Input;

namespace AudioMapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MaterialWindow
    {
        private Point lastMouseDown;

        public MainWindow()
        {
            //Need to set context for some reason it doesn't default
            DataContext = this;

            ThemeHelper.BindTheme();

            SettingsHelper.LoadSettings(false);

            InitializeComponent();
        }

        public DeviceController Controller { get; } = new DeviceController();

        private void BoundDevices_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => lastMouseDown = e.GetPosition(TvSoundDevices);

        private void BoundDevices_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => lastMouseDown = default;

        private void BoundDevices_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Device origin)
            {
                if (Controller?.SourceExistsDeviceById(origin.Id) ?? false)
                {
                    Device destination = Controller?.GetDestinationDeviceBySourceDeviceId(origin.Id);

                    if (destination != null)
                    {
                        Controller?.UpRemoveProposedMap(origin, destination);
                    }
                }
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            FunctionHelper.ConsumeExceptions(() => Controller?.Start());
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            FunctionHelper.ConsumeExceptions(() => Controller?.Stop());
        }

        private void btnUpdateMap_Click(object sender, RoutedEventArgs e)
        {
            FunctionHelper.ConsumeExceptions(() => Controller?.PushMapsToLive());
        }

        private bool IsValidDropTarget(DragEventArgs e, ref Device origin, ref Device destination)
        {
            if (!(e.Data.GetData(typeof(Device)) is Device childDevice) || !(e.OriginalSource is FrameworkElement targetElement) || !(targetElement.DataContext is Device parentDevice))
            {
                return false;
            }

            destination = parentDevice;
            origin = childDevice;

            return Controller?.CanMap(origin, destination) ?? false;
        }

        private void TvSoundDevices_DragOver(object sender, DragEventArgs e)
        {
            Device origin = null;
            Device destination = null;

            if (IsValidDropTarget(e, ref origin, ref destination))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void TvSoundDevices_Drop(object sender, DragEventArgs e)
        {
            lastMouseDown = default;

            Device origin = null;
            Device destination = null;

            if (IsValidDropTarget(e, ref origin, ref destination))
            {
                Controller?.AddMap(origin, destination);
            }
        }

        private void TvSoundDevices_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed &&
                    e.MiddleButton == MouseButtonState.Released &&
                    e.RightButton == MouseButtonState.Released &&
                    lastMouseDown != default)
                {
                    Point currentPosition = e.GetPosition(TvSoundDevices);

                    if ((Math.Abs(currentPosition.X - lastMouseDown.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(currentPosition.Y - lastMouseDown.Y) > SystemParameters.MinimumVerticalDragDistance) &&
                        TvSoundDevices.SelectedValue is Device)
                    {
                        DragDrop.DoDragDrop(TvSoundDevices, TvSoundDevices.SelectedValue, DragDropEffects.Copy | DragDropEffects.None);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            FunctionHelper.ConsumeExceptions(() => Controller?.Dispose());
        }
    }
}