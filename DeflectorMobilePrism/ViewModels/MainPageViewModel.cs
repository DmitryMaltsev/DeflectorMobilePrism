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

        private BluetoothDeviceModel _currentDevice { get; set; }
        private int _discoveringCounter = 0;
        private IBluetoothAdapter _bluetoothAdapter { get; set; }
        IBluetoothConnection currentConnection { get; set; }
        private bool startConnection;

        #region Rising properties
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

        #endregion

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


        public MainPageViewModel(INavigationService navigationService, IBlueToothService blueToothService)
            : base(navigationService)
        {
            Title = "Стартовая страница";
            NavigationService = navigationService;
            BlueToothService = blueToothService;
            AvailableDevices = new ObservableCollection<BluetoothDevice>();
            _bluetoothAdapter = DependencyService.Resolve<IBluetoothAdapter>();
            Device.StartTimer(TimeSpan.FromMilliseconds(500), TimerTickCallBack);
            startConnection = false;
        }

        private bool TimerTickCallBack()
        {
            //Если найдено устройство, проверяем на уникальность, иначе не добавляем
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

                MainActivityModel.BluetoothDevices.Clear();
                //if (SelectedDevice != null && SelectedDevice.BondState == Bond.Bonded)
                //{

                //    string sendingParameters = "0,20,30,40,50,60" + '\n';
                //    char[] byteBuffer = sendingParameters.ToCharArray();
                //    Encoding utf8 = Encoding.UTF8;
                //    byte[] buffer = utf8.GetBytes(byteBuffer);
                //    socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                //}

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

            //Если нажали кнопку подключиться
            if (startConnection)
            {
                _ = TryToConnect();
            }
            return true;
        }

        /// <summary>
        /// Подключаемся
        /// </summary>
        /// <returns></returns>
        async Task TryToConnect()
        {
            if (await currentConnection.RetryConnectAsync(retriesCount: 3))
            {
                NavigationParameters parameter = new NavigationParameters();
                parameter.Add("CurrentDevice", _currentDevice);
                _ = NavigationService.NavigateAsync("ChangeModes", parameter);
            }
            else
            {
                startConnection = false;
            }
        }

        #region Execute commands

        void ExecuteConnectBondedDeviceCommand(BluetoothDeviceModel bluetoothDeviceModel)
        {
            if (bluetoothDeviceModel != null)
            {
                _currentDevice = bluetoothDeviceModel;
                using (currentConnection = BlueToothService.CreateConnection(bluetoothDeviceModel)) ;
                startConnection = true;
            }
        }

        void ExecuteConnnectDeviceCommand(BluetoothDevice bluetoothDevice)
        {
            if (bluetoothDevice != null)
            {
                NavigationParameters parameter = new NavigationParameters();
                //Шаманство^^
                BluetoothDeviceModel currentDevice = new BluetoothDeviceModel(bluetoothDevice.Address, bluetoothDevice.Name);
                _currentDevice = currentDevice;
                using (currentConnection = BlueToothService.CreateConnection(currentDevice));
                startConnection = true;
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
