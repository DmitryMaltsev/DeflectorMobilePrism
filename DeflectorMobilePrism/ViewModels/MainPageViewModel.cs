using Plugin.BluetoothClassic.Abstractions;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace DeflectorMobilePrism.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public new INavigationService NavigationService { get; }
        private BluetoothDeviceModel _selectedDevice;
        public BluetoothDeviceModel SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                SetProperty(ref _selectedDevice, value);
                if (_selectedDevice != null)
                {
                    ExecuteDeviceSelectedCommand();
                }
            }
        }
        private IEnumerable<BluetoothDeviceModel> _available_Devices;
        public IEnumerable<BluetoothDeviceModel> Available_Devices
        {
            get { return _available_Devices; }
            set { SetProperty(ref _available_Devices, value); }
        }


        public MainPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "Main Page";
            NavigationService = navigationService;
            FillBondedDevices();
        }

        private void FillBondedDevices()
        {
            IBluetoothAdapter bluetoothAdapter = DependencyService.Resolve<IBluetoothAdapter>();
            Available_Devices = bluetoothAdapter.BondedDevices;
        }

        private void ExecuteDeviceSelectedCommand()
        {

            NavigationParameters parameter = new NavigationParameters();
            parameter.Add("SelectedDevice", SelectedDevice);
            SelectedDevice = null;
            NavigationService.NavigateAsync("ChangeModes", parameter);
                    
        }
    }
}
