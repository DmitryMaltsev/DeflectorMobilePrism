using Android.Bluetooth;

using DeflectoreMobilePrism.Core;

using IServices;

using Plugin.BluetoothClassic.Abstractions;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DeflectorMobilePrism.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {


        public new INavigationService NavigationService { get; }
        public IBlueToothService BlueToothService { get; }

        private IEnumerable<BluetoothDeviceModel> _availableBondedDevices;
        public IEnumerable<BluetoothDeviceModel> AvailableBondedDevices
        {
            get { return _availableBondedDevices; }
            set { SetProperty(ref _availableBondedDevices, value); }
        }

        private ObservableCollection<BluetoothDevice> _availabledevices;
        public ObservableCollection<BluetoothDevice> AvailableDevices
        {
            get { return _availabledevices; }
            set { SetProperty(ref _availabledevices, value); }
        }


        #region Delegate commands

        private DelegateCommand<BluetoothDeviceModel> _connectBondedDeviceCommand;
        public DelegateCommand<BluetoothDeviceModel> ConnectBondedDeviceCommand =>
            _connectBondedDeviceCommand ?? (_connectBondedDeviceCommand = new DelegateCommand<BluetoothDeviceModel>(ExecuteConnectBondedDeviceCommand));

        private DelegateCommand<BluetoothDevice> _connectDeviceCommand;
        public DelegateCommand<BluetoothDevice> ConnnectDeviceCommand =>
         _connectDeviceCommand ?? (_connectDeviceCommand = new DelegateCommand<BluetoothDevice>(ExecuteConnnectDeviceCommand));

        private DelegateCommand _scanDevicesCommand;
        public DelegateCommand ScanDevicesCommand =>
            _scanDevicesCommand ?? (_scanDevicesCommand = new DelegateCommand(ExecuteScanDevicesCommand));


        #endregion



        private int _discoveringCounter = 0;
        private IBluetoothAdapter _bluetoothAdapter { get; set; }
        IBluetoothConnection currentConnection { get; set; }
        public MainPageViewModel(INavigationService navigationService, IBlueToothService blueToothService)
            : base(navigationService)
        {
            Title = "Стартовая страница";
            NavigationService = navigationService;
            BlueToothService = blueToothService;
            AvailableDevices = new ObservableCollection<BluetoothDevice>();
            _bluetoothAdapter = DependencyService.Resolve<IBluetoothAdapter>();
            Device.StartTimer(TimeSpan.FromMilliseconds(500), TimerTickCallBack);
        }

        private bool TimerTickCallBack()
        {

            if (MainActivityModel.BluetoothDevices.Count > 0)
            {
                foreach (BluetoothDevice foundedDevice in MainActivityModel.BluetoothDevices)
                {
                    int coincidenceCount = AvailableDevices.Where(p => p.Address == foundedDevice.Address).Count();
                    if (AvailableDevices.Count == 0 || coincidenceCount == 0)
                    {
                        AvailableDevices.Add(foundedDevice);
                    }
                }
                if (currentConnection != null)
                {
                    _ = TryToConnect();
                }

                async Task TryToConnect()
                {
                    if ( await currentConnection.RetryConnectAsync(retriesCount: 3))
                    {
                        NavigationParameters parameter = new NavigationParameters();
                        parameter.Add("CurrentConnection", currentConnection);
                        _ = NavigationService.NavigateAsync("ChangeModes", parameter);
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
                MainActivityModel.BluetoothDevices.Clear();
            }

            //N секунд поиск устройств
            if (MainActivityModel.BlutoothAdapter.IsDiscovering)
            {
                _discoveringCounter += 1;
                if (_discoveringCounter > 30)
                {
                    MainActivityModel.BlutoothAdapter.CancelDiscovery();
                    _discoveringCounter = 0;
                }
            }
            return true;
        }

        #region Execute commands

        void ExecuteConnectBondedDeviceCommand(BluetoothDeviceModel bluetoothDeviceModel)
        {
            if (bluetoothDeviceModel != null)
            {
                using (currentConnection = BlueToothService.CreateConnection(bluetoothDeviceModel)) ;
            }
        }

        void ExecuteConnnectDeviceCommand(BluetoothDevice bluetoothDevice)
        {
            if (bluetoothDevice != null)
            {
                NavigationParameters parameter = new NavigationParameters();
                //Шаманство^^
                BluetoothDeviceModel currentDevice = new BluetoothDeviceModel(bluetoothDevice.Address, bluetoothDevice.Name);
                using (currentConnection = BlueToothService.CreateConnection(currentDevice)) ;
            }
        }




        void ExecuteScanDevicesCommand()
        {
            MainActivityModel.BluetoothDevices.Clear();
            AvailableDevices.Clear();
            //Список сопрояженных устройств(уже есть в тлф)
            AvailableBondedDevices = _bluetoothAdapter.BondedDevices;
            try
            {
                if (MainActivityModel.BlutoothAdapter.IsDiscovering)
                {
                    MainActivityModel.BlutoothAdapter.CancelDiscovery();
                }

                MainActivityModel.BlutoothAdapter.StartDiscovery();
            }
            catch (Exception ex)
            {
                //  DisplayAlert("ex", ex.ToString(), "ok");
            }
        }
        #endregion
    }
}
