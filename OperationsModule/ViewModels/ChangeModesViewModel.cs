using IServices;

using Plugin.BluetoothClassic.Abstractions;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

using Xamarin.Forms;

namespace OperationsModule.ViewModels
{
    public class ChangeModesViewModel : BindableBase, INavigationAware
    {
        private BluetoothDeviceModel _selectedDevice { get; set; }
        private string _message = "";
        private double[] _doubleBuffer;
        public ChangeModesViewModel(ISensorsDataRepository sensorsDataRepository, IBlueToothService blueToothService)
        {
            SensorsDataRepository = sensorsDataRepository;
            BlueToothService = blueToothService;
        }

        public ISensorsDataRepository SensorsDataRepository { get; }
        public IBlueToothService BlueToothService { get; }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            _selectedDevice = parameters.GetValue<BluetoothDeviceModel>("ChangeModes");
            Device.StartTimer(TimeSpan.FromMilliseconds(10), TimerTickCallBack);
           // Task<double[]> blueToothRecieveTask = new Task<double[]>(() => BlueToothService.RecieveSensorsData(_selectedDevice));
            //(_message,_doubleBuffer)=BlueToothService.RecieveSensorsData(_selectedDevice);
            //blueToothRecieveTask.Start();
        }

        private bool TimerTickCallBack()
        {

            SensorsDataRepository.CurrentTemperature = _doubleBuffer[0];
            SensorsDataRepository.CurrentPressure = _doubleBuffer[1];
            return true;
        }

    }
}
