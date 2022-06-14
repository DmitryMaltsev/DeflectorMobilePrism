using Android.Bluetooth;

using Plugin.BluetoothClassic.Abstractions;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace DeflectorMobilePrism.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {


        public new INavigationService NavigationService { get; }
        private BluetoothDeviceModel _selectedBoundedDevice;
        public BluetoothDeviceModel SelectedBoundedDevice
        {
            get { return _selectedBoundedDevice; }
            set
            {
                SetProperty(ref _selectedBoundedDevice, value);
                if (_selectedBoundedDevice != null)
                {
                    ExecuteDeviceSelectedCommand();
                }
            }
        }
        private IEnumerable<BluetoothDeviceModel> _available_Bounded_Devices;
        public IEnumerable<BluetoothDeviceModel> Available_Bounded_Devices
        {
            get { return _available_Bounded_Devices; }
            set { SetProperty(ref _available_Bounded_Devices, value); }
        }

        private ObservableCollection<BluetoothDevice> _available_devices;
        public ObservableCollection<BluetoothDevice> Available_Devices
        {
            get { return _available_devices; }
            set { SetProperty(ref _available_devices, value); }
        }

        private BluetoothDevice _selectedDevice;
        public BluetoothDevice SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                SetProperty(ref _selectedDevice, value);
                if (_selectedDevice != null)
                {
                  //  ExecuteConnectDeviceCommand();
                }
            }
        }


        public MainPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "Стартовая страница";
            NavigationService = navigationService;
            FillBondedDevices();
        }

        private void FillBondedDevices()
        {
            IBluetoothAdapter bluetoothAdapter = DependencyService.Resolve<IBluetoothAdapter>();
            Available_Bounded_Devices = bluetoothAdapter.BondedDevices;
        }

        private void ExecuteDeviceSelectedCommand()
        {
            if (SelectedBoundedDevice != null)
            {
                NavigationParameters parameter = new NavigationParameters();
                parameter.Add("SelectedDevice", SelectedBoundedDevice);
                SelectedBoundedDevice = null;
                NavigationService.NavigateAsync("ChangeModes", parameter);
            }
        }
    }
}
