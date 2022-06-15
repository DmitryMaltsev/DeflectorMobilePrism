using IServices;

using Plugin.BluetoothClassic.Abstractions;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Xamarin.Forms;

namespace OperationsModule.ViewModels
{
    public class ChangeModesViewModel : BindableBase, INavigationAware
    {

        #region Rising properties
        private string _systemLogMessage;
        public string SystemLogMessage
        {
            get { return _systemLogMessage; }
            set { SetProperty(ref _systemLogMessage, value); }
        }

        private string _bluetoothLogMessage;
        public string BliuetoothLogMessage
        {
            get { return _bluetoothLogMessage; }
            set { SetProperty(ref _bluetoothLogMessage, value); }
        }


        private bool _isRecievingData = true;
        public bool IsRecievingData
        {
            get { return _isRecievingData; }
            set
            {
                SetProperty(ref _isRecievingData, value);
            }
        }


        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set { SetProperty(ref _deviceName, value); }
        }

        /// <summary>
        /// Если ручной мод и подключение свободно
        /// </summary>
        private bool _canChangePower = false;
        public bool CanChangePower
        {
            get { return _canChangePower; }
            set
            {
                SetProperty(ref _canChangePower, value);
            }
        }
        #endregion

        private string _bluetoothMessage;
        private double[] _currentParameters;
        private bool _pageIsActive;
        public ISensorsDataRepository SensorsDataRepository { get; }
        public IBlueToothService BlueToothService { get; }
        IBluetoothConnection _currentConnection { get; set; }
        BluetoothDeviceModel _currentDevice { get; set; }
        private event EventHandler ChangeModeEvent;

        #region Delegatecommands

        private DelegateCommand _decimalOnCommand;
        public DelegateCommand DecimalOnCommand =>
            _decimalOnCommand ?? (_decimalOnCommand = new DelegateCommand(ExecuteDecimalOnCommand));

        private DelegateCommand _decimalOffCommand;
        public DelegateCommand DecimalOffCommand =>
            _decimalOffCommand ?? (_decimalOffCommand = new DelegateCommand(ExecuteDecimalOffCommand));

        private DelegateCommand _unitOnCommand;
        public DelegateCommand UnitOnCommand =>
            _unitOnCommand ?? (_unitOnCommand = new DelegateCommand(ExecuteUnitOnCommand));

        private DelegateCommand _unitOffCommand;
        public DelegateCommand UnitOffCommand =>
            _unitOffCommand ?? (_unitOffCommand = new DelegateCommand(ExecuteUnitOffCommand));

        private DelegateCommand _acceptPowerCommand;
        public DelegateCommand AcceptPowerCommand =>
            _acceptPowerCommand ?? (_acceptPowerCommand = new DelegateCommand(ExecuteAcceptPowerCommand));

        private DelegateCommand _changeModeCommand;
        public DelegateCommand ChangeModeCommand =>
            _changeModeCommand ?? (_changeModeCommand = new DelegateCommand(ExecuteChangeModeCommand));

        private DelegateCommand _changeFloorNumsCommand;
        public DelegateCommand ChangeFloorNumsCommand =>
            _changeFloorNumsCommand ?? (_changeFloorNumsCommand = new DelegateCommand(ExecuteChangeFloorNumsCommand));

        #endregion

        public ChangeModesViewModel(ISensorsDataRepository sensorsDataRepository, IBlueToothService blueToothService)
        {
            SensorsDataRepository = sensorsDataRepository;
            BlueToothService = blueToothService;
            _currentParameters = new double[5];
            _bluetoothMessage = "";

        }

        #region ExecuteMethods

        void ExecuteUnitOnCommand()
        {

            if (SensorsDataRepository.UnitNum < 9 && SensorsDataRepository.DecimalNum < 5)
            {
                SensorsDataRepository.UnitNum += 1;
            }
        }

        void ExecuteUnitOffCommand()
        {
            if (SensorsDataRepository.UnitNum > 0)
            {
                SensorsDataRepository.UnitNum -= 1;
            }
        }

        void ExecuteDecimalOnCommand()
        {
            if (SensorsDataRepository.DecimalNum < 5)
            {
                SensorsDataRepository.DecimalNum += 1;
            }
            if (SensorsDataRepository.DecimalNum == 5)
            {
                SensorsDataRepository.UnitNum = 0;
            }
        }


        void ExecuteDecimalOffCommand()
        {
            if (SensorsDataRepository.DecimalNum > 0)
            {
                SensorsDataRepository.DecimalNum -= 1;
            }
        }

        /// <summary>
        /// Меняем мощность в ручном режиме
        /// </summary>
        void ExecuteAcceptPowerCommand()
        {
            string symbols = "p" + SensorsDataRepository.DecimalNum.ToString() + SensorsDataRepository.UnitNum.ToString();
            SensorsDataRepository.NumsOn = false;
            SendBlueToothCommand(symbols);
            SensorsDataRepository.NumsOn = true;
        }


        /// <summary>
        /// Меняем режим
        /// </summary>
        void ExecuteChangeModeCommand()
        {
            string symbols = "m" + SensorsDataRepository.SelectedModeIndex;
            SendBlueToothCommand(symbols);
        }

        /// <summary>
        /// Меняем количество этажей
        /// </summary>
        void ExecuteChangeFloorNumsCommand()
        {
            string symbols = "f" + SensorsDataRepository.FloorNumber;
            SendBlueToothCommand(symbols);
        }

        /// <summary>
        /// Присылаем команду BlueTooth модулю
        /// </summary>
        /// <param name="symbols"></param>
        async void SendBlueToothCommand(string symbols)
        {
            IsRecievingData = false;
            //using (_currentConnection = BlueToothService.CreateConnection(_selectedDevice))
            //{
            if (await _currentConnection.RetryConnectAsync(retriesCount: 3))
            {
                _bluetoothMessage = await Task.Run(() => BlueToothService.SendMode(_currentConnection, symbols));
            }
            else
            {
                _bluetoothMessage = "Нет подключения при отправке";
            }
            //   }
            IsRecievingData = true;
            RecieveData(true);
        }
        #endregion

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            IsRecievingData = false;
            _pageIsActive = false;
            _currentConnection.Dispose();
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            _currentDevice = parameters.GetValue<BluetoothDeviceModel>("CurrentDevice");
            RecieveData(true);
            _pageIsActive = true;
        }


        /// <summary>
        /// Получаем данные с BlueTooth
        /// </summary>
        /// <param name="canChangeMode"></param>
        private async void RecieveData(bool canChangeMode)
        {
            using (_currentConnection = BlueToothService.CreateConnection(_currentDevice))
            {
                try
                {
                    await _currentConnection.RetryConnectAsync(retriesCount: 3);
                    while (IsRecievingData)
                    {
                        (_currentParameters, BliuetoothLogMessage) = await Task.Run(() => BlueToothService.RecieveSensorsData(_currentConnection));
                            //Дополнительная проверка на полученные значения, т.к. проверка на соединение не всегда работает
                            double dataSumm = _currentParameters[0] + _currentParameters[1] + _currentParameters[2] + _currentParameters[3] + _currentParameters[4];
                            if (dataSumm != 0)
                            {
                                SensorsDataRepository.CurrentTemperature = _currentParameters[0];
                                SensorsDataRepository.CurrentPressure = _currentParameters[1];
                                SensorsDataRepository.CurrentPower = _currentParameters[2];
                                int modeIndex = Convert.ToInt32(_currentParameters[3]);
                                int floorNum = Convert.ToInt32(_currentParameters[5]);
                                //Для отображения начального режима
                                if (SensorsDataRepository.SelectedModeIndex == -1)
                                {
                                    SensorsDataRepository.SelectedModeIndex = modeIndex;
                                }
                                if (SensorsDataRepository.FloorNumber == -1)
                                {
                                    SensorsDataRepository.FloorNumber = floorNum;
                                }
                                //Потому допишу. Не знаю чем это будет
                                //_ = _currentParameters[4] == 1 ? SystemLogMessage = "Реле замкнуто" : SystemLogMessage = "Реле разомкнуто";
                                _ = SensorsDataRepository.Mode == "Ручной" ? SensorsDataRepository.NumsOn = true : SensorsDataRepository.NumsOn = false;
                            }
                    }
                }
                catch (Exception ex)
                {

                    SystemLogMessage = $"{ ex.Message} {ex.StackTrace}";
                }

            }
        }
    }
}
