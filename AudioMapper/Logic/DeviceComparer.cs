using AudioMapper.Models;
using DatabaseHelper.Extensions;
using System.Collections.Generic;

namespace AudioMapper.Logic
{
    public class DeviceComparer : IComparer<Device>
    {
        public int Compare(Device x, Device y)
        {
            //Outputs first, then Inputs, Then by name

            if (x.DeviceType == SoundDevices.DeviceType.Output && y.DeviceType == SoundDevices.DeviceType.Input)
            {
                return -1;
            }

            if (x.DeviceType == SoundDevices.DeviceType.Input && y.DeviceType == SoundDevices.DeviceType.Output)
            {
                return 1;
            }

            return string.Compare(x.Name.SafeTrim(), y.Name.SafeTrim(), true);
        }
    }
}