using AudioMapper.Controllers;
using AudioMapper.Helpers;
using AudioMapper.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AudioMapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AudioMapsController controller;
        private Point lastMouseDown;

        public MainWindow()
        {
            InitializeComponent();

            controller = new AudioMapsController();

            TvSoundDevices.ItemsSource = controller.Devices;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                controller.Map();
                controller.Start();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private Device GetVisualParentDevice(DependencyObject item)
        {
            //Dodgey work around for .net having no nice Parent for databound object
            //We are looping through all elements looking for our Device
            if (!(VisualTreeHelper.GetParent(item) is FrameworkElement visualParent))
            {
                return null;
            }

            if (visualParent.DataContext is Device parentDevice)
            {
                return parentDevice;
            }
            else
            {
                return GetVisualParentDevice(visualParent);
            }
        }

        private bool IsValidDropTarget(DragEventArgs e, ref Device source, ref Device target)
        {
            if (!(e.Data.GetData(typeof(Device)) is Device sourceDevice) || !(e.OriginalSource is TextBlock targetElement) || !(targetElement.DataContext is Device targetDevice))
            {
                return false;
            }

            source = sourceDevice;
            target = targetDevice;

            return source.Id != target.Id && //Not the same device
                    target.DeviceType != SoundDevices.DeviceType.Input && //Can't output to a microphone
                    !target.MappedDevices.Any(d => d.Id == sourceDevice.Id) && //Has not already been added
                    !source.MappedDevices.Any(d => d.Id == targetDevice.Id); //Destination is not mapped the origin already (will cause self reference);
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => lastMouseDown = e.GetPosition(TvSoundDevices);

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => lastMouseDown = default;

        private void StackPanel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Grid grid) || !(grid.DataContext is MappedDevice selectedDevice))
            {
                return;
            }

            Device parentDevice = GetVisualParentDevice(grid);

            controller.RemoveMapIfExists(selectedDevice?.Id, parentDevice?.Id);
            parentDevice?.MappedDevices.Remove(selectedDevice);

            if (!parentDevice.MappedDevices.Any())
            {
                parentDevice.MapState = SoundDevices.MapState.Inactive;
            }
        }

        private void TvSoundDevices_DragOver(object sender, DragEventArgs e)
        {
            Device source = null;
            Device target = null;

            if (IsValidDropTarget(e, ref source, ref target))
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

            Device source = null;
            Device target = null;

            if (IsValidDropTarget(e, ref source, ref target))
            {
                target.MappedDevices.Add(MappedDevice.FromDevice(source));
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
            Helper.ConsumeExceptions(() => controller?.Dispose());
        }
    }
}