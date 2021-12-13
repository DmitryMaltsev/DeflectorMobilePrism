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
        private string _bluetoothMessage;
        private double[] _currentParameters;
        private bool isRecievingData = true;
        private bool _pageIsActive;
        public ISensorsDataRepository SensorsDataRepository { get; }
        public IBlueToothService BlueToothService { get; }
        BluetoothDeviceModel _selectedDevice { get; set; }
        IBluetoothConnection _currentConnection { get; set; }

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

        void ExecuteAcceptPowerCommand()
        {
            string symbols = "p" + SensorsDataRepository.DecimalNum.ToString() + SensorsDataRepository.UnitNum.ToString();
            SendPowerCommand(symbols);
        }



        void ExecuteChangeModeCommand()
        {
            string symbols = "m" + SensorsDataRepository.SelectedMode;
            SendPowerCommand(symbols);
        }

        async void SendPowerCommand(string symbols)
        {
            isRecievingData = false;
            using (_currentConnection = BlueToothService.CreateConnection(_selectedDevice))
            {
                if (await _currentConnection.RetryConnectAsync(retriesCount: 3))
                {
                    _bluetoothMessage = await Task.Run(() => BlueToothService.SendMode(_currentConnection, symbols));
                }
                else
                {
                    _bluetoothMessage = "Нет подключения при отправке";
                }
            }
            isRecievingData = true;
            RecieveData();
        }
        #endregion

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            isRecievingData = false;
            _selectedDevice = null;
            _pageIsActive = false;
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            _selectedDevice = parameters.GetValue<BluetoothDeviceModel>("SelectedDevice");
            RecieveData();
            _pageIsActive = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(10), TimerTickCallBack);   
        }

        private bool TimerTickCallBack()
        {
            try
            {

                SensorsDataRepository.CurrentTemperature = _currentParameters[0];
                SensorsDataRepository.CurrentPressure = _currentParameters[1];
                SensorsDataRepository.CurrentPower = _currentParameters[2];
                int index = Convert.ToInt32(_currentParameters[3]);
                SensorsDataRepository.Mode = SensorsDataRepository.Modes[index];
                _ = _currentParameters[4] == 1 ? SystemLogMessage = "Контактор замкнут" : SystemLogMessage = "Контактор разомкнут";
                BliuetoothLogMessage = _bluetoothMessage;
                NumsButtonsIsActive();
                if (_pageIsActive)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                SystemLogMessage = ex.Message;
                return true;
            }
            
        }

        private void NumsButtonsIsActive()
        {
            _ = SensorsDataRepository.Mode == "Ручной" ? SensorsDataRepository.NumsOn = true : SensorsDataRepository.NumsOn = false;
        }

        private async void RecieveData()
        {
            using (_currentConnection = BlueToothService.CreateConnection(_selectedDevice))
            {
                if (await _currentConnection.RetryConnectAsync(retriesCount: 3))
                {
                    while (isRecievingData)
                    {
                        (_currentParameters, _bluetoothMessage) = await Task.Run(() => BlueToothService.RecieveSensorsData(_currentConnection));
                        Thread.Sleep(100);
                    }
                }
                else
                {
                    _bluetoothMessage = "Данные не приняты";
                }
            }
        }
    }
}
