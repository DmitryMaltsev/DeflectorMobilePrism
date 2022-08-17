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
        public ISensorsDataRepository SensorsDataRepository { get; }
        public IBlueToothService BlueToothService { get; }
        IBluetoothConnection _currentConnection { get; set; }
        IBluetoothConnection _sendingConnection { get; set; }
        BluetoothDeviceModel _currentDevice { get; set; }
        private double[] _currentParameters;
        private bool _pageIsActive;
        private bool _isRecievingData;
        private bool _needToSendData;
        private string _symbols;

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
        }

        #region ExecuteMethods

        void ExecuteUnitOnCommand()
        {

            if (SensorsDataRepository.UnitNum < 9 && SensorsDataRepository.DecimalNum < 8)
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
            if (SensorsDataRepository.DecimalNum < 8)
            {
                SensorsDataRepository.DecimalNum += 1;
            }
            if (SensorsDataRepository.DecimalNum == 8)
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
            _symbols = "p" + SensorsDataRepository.DecimalNum.ToString() + SensorsDataRepository.UnitNum.ToString();
            SensorsDataRepository.NumsOn = false;
            // SendBlueToothCommand(symbols);
            SensorsDataRepository.NumsOn = true;
            _needToSendData = true;
        }


        /// <summary>
        /// Меняем режим
        /// </summary>
        void ExecuteChangeModeCommand()
        {
            _symbols = "m" + SensorsDataRepository.SelectedModeIndex;
            //  await SendBlueToothCommand(symbols);
            _needToSendData = true;
        }

        /// <summary>
        /// Меняем количество этажей
        /// </summary>
        void ExecuteChangeFloorNumsCommand()
        {
            _symbols = "f" + SensorsDataRepository.FloorNumber;
            // await SendBlueToothCommand(symbols);
            _needToSendData = true;
        }

        /// <summary>
        /// Присылаем команду BlueTooth модулю
        /// </summary>
        /// <param name="symbols"></param>
        async Task<bool> SendBlueToothCommand(string symbols)
        {
            try
            {
                SystemLogMessage = await BlueToothService.SendMode(_currentConnection, symbols);
                return true;

            }
            catch (Exception ex)
            {
                SystemLogMessage = ex.Message;
                return false;
            }
            finally
            {
                _needToSendData = false;
            }
        }
        #endregion

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            _isRecievingData = false;
            _pageIsActive = false;
            _currentConnection.Dispose();
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            _currentDevice = parameters.GetValue<BluetoothDeviceModel>("CurrentDevice");
            _isRecievingData = true;
            _needToSendData = false;
            await RecieveData(true);
            _pageIsActive = true;

        }


        /// <summary>
        /// Получаем данные с BlueTooth
        /// </summary>
        /// <param name="canChangeMode"></param>
        private async Task RecieveData(bool canChangeMode)
        {
            SystemLogMessage = string.Empty;
            while (_isRecievingData)
            {
                try
                {
                    using (_currentConnection = BlueToothService.CreateConnection(_currentDevice))
                    {
                        if (await _currentConnection.RetryConnectAsync(retriesCount: 3))
                        {
                            (_currentParameters, SystemLogMessage) = await Task.Run(() => BlueToothService.RecieveSensorsData(_currentConnection));
                            //Дополнительная проверка на полученные значения, т.к. проверка на соединение не всегда работает
                            double dataSumm = _currentParameters[0] + _currentParameters[1] + _currentParameters[2] + _currentParameters[3] + _currentParameters[4];
                            if (dataSumm != 0)
                            {
                                SensorsDataRepository.CurrentTemperature = _currentParameters[0];
                                SensorsDataRepository.CurrentPressure = _currentParameters[1];
                                SensorsDataRepository.CurrentPower = _currentParameters[2];
                                int modeIndex = Convert.ToInt32(_currentParameters[3]);
                                SensorsDataRepository.Mode = SensorsDataRepository.Modes[modeIndex];
                                int floorNum = Convert.ToInt32(_currentParameters[6]);
                                SensorsDataRepository.CurrentFloorNumber = floorNum;
                                //Для отображения начального режима
                                if (SensorsDataRepository.SelectedModeIndex == -1)
                                {
                                    SensorsDataRepository.SelectedModeIndex = modeIndex;
                                }

                                if (SensorsDataRepository.FloorNumber == -1)
                                {
                                    SensorsDataRepository.FloorNumber = floorNum;
                                }

                                _ = _currentParameters[4] == 1 ? SensorsDataRepository.TermoreleColor = Color.Green : SensorsDataRepository.TermoreleColor = Color.Red;
                                _ = _currentParameters[5] == 1 ? SensorsDataRepository.FireAlertColor = Color.Green : SensorsDataRepository.FireAlertColor = Color.Red;
                                _ = SensorsDataRepository.Mode == "Ручной" ? SensorsDataRepository.NumsOn = true : SensorsDataRepository.NumsOn = false;
                            }
                            //Шлем данные, если нужно
                            if (_needToSendData)
                            {
                                bool result = await SendBlueToothCommand(_symbols);
                            }
                        }
                    }
                }
                catch
                {
                    SystemLogMessage = "Ошибка передачи";
                }
                await Task.Delay(400);
            }
        }
    }

}




