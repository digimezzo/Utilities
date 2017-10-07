using System;

namespace Digimezzo.Utilities.Settings
{
    public class SettingChangedEventArgs : EventArgs
    {
        public string SettingNamespace { get; set; }
        public string SettingName { get; set; }
    }
}
