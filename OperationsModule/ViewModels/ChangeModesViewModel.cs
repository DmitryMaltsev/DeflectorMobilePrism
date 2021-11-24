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

        private BluetoothDeviceModel _selectedDevice { get; set; }
        private string _message = "";
        private double[] _currentParameters;
        public ISensorsDataRepository SensorsDataRepository { get; }
        public IBlueToothService BlueToothService { get; }
        IBluetoothConnection connection;
        #region Delegates
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
            _currentParameters = new double[3];
        }

        #region ExecuteMethods

        void ExecuteUnitOnCommand()
        {

            if (SensorsDataRepository.UnitNum < 9)
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
            //  string symbols = SensorsDataRepository.DecimalNum.ToString() + SensorsDataRepository.UnitNum.ToString();
            string symbols = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            SendCommand(symbols);
        }

        async void SendCommand(string symbols)
        {

            _message = await Task.Run(() => BlueToothService.SendMode(_selectedDevice, symbols));
        }
        void ExecuteChangeModeCommand()
        {
            NumsButtonsIsActive();
        }
        #endregion


        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            _selectedDevice = parameters.GetValue<BluetoothDeviceModel>("SelectedDevice");
            //  RecieveSensorsData(_selectedDevice);

            RecieveData();
            Device.StartTimer(TimeSpan.FromMilliseconds(10), TimerTickCallBack);
            //  bluetoothRecieveTask.Start();
            NumsButtonsIsActive();
        }

        private async void RecieveData()
        {

            //while (true)
            //{
            //    (_currentParameters, _message) = await Task.Run(() => BlueToothService.RecieveSensorsData(connection));
            //    Thread.Sleep(100);
            //}

        }


        private void NumsButtonsIsActive()
        {
            _ = SensorsDataRepository.Mode == "Ручной" ? SensorsDataRepository.NumsOn : SensorsDataRepository.NumsOn == false;
        }

        private bool TimerTickCallBack()
        {
            SensorsDataRepository.CurrentTemperature = _currentParameters[0];
            SensorsDataRepository.CurrentPressure = _currentParameters[1];
            return true;
        }
    }
}
