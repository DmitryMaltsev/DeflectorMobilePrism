using Plugin.BluetoothClassic.Abstractions;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;

using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

namespace OperationsModule.ViewModels
{
    public class ChangeModesViewModel : BindableBase, INavigationAware
    {
        private BluetoothDeviceModel selectedDevice { get; set; }
        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            selectedDevice = parameters.GetValue<BluetoothDeviceModel>("ChangeModes");
        }

    }
}
