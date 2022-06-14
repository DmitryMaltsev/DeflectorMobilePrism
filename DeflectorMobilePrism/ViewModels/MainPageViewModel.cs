using Android.Bluetooth;

using DeflectoreMobilePrism.Core;

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
        #region Cопряженные устройства
        private BluetoothDeviceModel _selectedBondedDevice;
        public BluetoothDeviceModel SelectedBondedDevice
        {
            get { return _selectedBondedDevice; }
            set
            {
                SetProperty(ref _selectedBondedDevice, value);
                if (_selectedBondedDevice != null)
                {
                    ExecuteDeviceSelectedCommand();
                }
            }
        }
        private IEnumerable<BluetoothDeviceModel> _availableBondedDevices;
        public IEnumerable<BluetoothDeviceModel> AvailableBondedDevices
        {
            get { return _availableBondedDevices; }
            set { SetProperty(ref _availableBondedDevices, value); }
        }

        #endregion

        #region Найденные устройства
        private ObservableCollection<BluetoothDevice> _availabledevices;
        public ObservableCollection<BluetoothDevice> AvailableDevices
        {
            get { return _availabledevices; }
            set { SetProperty(ref _availabledevices, value); }
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
        #endregion

        private DelegateCommand _scanDevicesCommand;
        public DelegateCommand ScanDevicesCommand =>
            _scanDevicesCommand ?? (_scanDevicesCommand = new DelegateCommand(ExecuteScanDevicesCommand));

        public MainPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "Стартовая страница";
            NavigationService = navigationService;
            AvailableDevices = new ObservableCollection<BluetoothDevice>();
            Device.StartTimer(TimeSpan.FromMilliseconds(200), TimerTickCallBack);
          FillBondedDevices();
        }

        private bool TimerTickCallBack()
        {
            if (MainActivityModel.BluetoothDevices.Count > 0)
            {
                foreach (BluetoothDevice foundedDevice in MainActivityModel.BluetoothDevices)
                {
                    int coincidenceCount = AvailableDevices.Where(p => p.Name == foundedDevice.Name).Count();
                    if (AvailableDevices.Count == 0 || coincidenceCount == 0)
                    {
                        AvailableDevices.Add(foundedDevice);
                    }
                }

                //if (SelectedDevice != null && SelectedDevice.BondState == Bond.Bonded)
                //{

                //    string sendingParameters = "0,20,30,40,50,60" + '\n';
                //    char[] byteBuffer = sendingParameters.ToCharArray();
                //    Encoding utf8 = Encoding.UTF8;
                //    byte[] buffer = utf8.GetBytes(byteBuffer);
                //    socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                //}
            }
            return true;
        }

        private void FillBondedDevices()
        {
            IBluetoothAdapter bluetoothAdapter = DependencyService.Resolve<IBluetoothAdapter>();
            AvailableBondedDevices = bluetoothAdapter.BondedDevices;
        }

        private void ExecuteDeviceSelectedCommand()
        {
            if (SelectedBondedDevice != null)
            {
                NavigationParameters parameter = new NavigationParameters();
                parameter.Add("SelectedDevice", SelectedBondedDevice);
                SelectedBondedDevice = null;
                NavigationService.NavigateAsync("ChangeModes", parameter);
            }
        }


        void ExecuteScanDevicesCommand()
        {
            MainActivityModel.BluetoothDevices.Clear();
           AvailableDevices.Clear();
            try
            {
                if (MainActivityModel.BlutoothAdapter.IsDiscovering)
                {
                    MainActivityModel.BlutoothAdapter.CancelDiscovery();
                }

                MainActivityModel.BlutoothAdapter.StartDiscovery();
            }

            // var res=mBluetoothAdapter.

            catch (Exception ex)
            {
                //  DisplayAlert("ex", ex.ToString(), "ok");
            }
        }
    }
}
