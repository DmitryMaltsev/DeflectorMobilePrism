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
        public ChangeModesViewModel(ISensorsDataRepository sensorsDataRepository, IBlueToothService blueToothService)
        {
            SensorsDataRepository = sensorsDataRepository;
            BlueToothService = blueToothService;
            _currentParameters = new double[3];
        }

        public ISensorsDataRepository SensorsDataRepository { get; }
        public IBlueToothService BlueToothService { get; }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            _selectedDevice = parameters.GetValue<BluetoothDeviceModel>("ChangeModes");         
            Task bluetoothRecieveTask = new Task(() => RecieveSensorsData(_selectedDevice));
            Device.StartTimer(TimeSpan.FromMilliseconds(10), TimerTickCallBack);
        }

        private bool TimerTickCallBack()
        {
            SensorsDataRepository.CurrentTemperature = _currentParameters[0];
            SensorsDataRepository.CurrentPressure = _currentParameters[1];
            return true;
        }

        private async void RecieveSensorsData(BluetoothDeviceModel selectedDevice)
        {
            if (selectedDevice != null)
            {
                byte[] buffer = new byte[12];
                IBluetoothAdapter adapter = DependencyService.Resolve<IBluetoothAdapter>();
                using (IBluetoothConnection connection = adapter.CreateConnection(selectedDevice))
                {

                    if (await connection.RetryConnectAsync(retriesCount: 3))
                    {
                        while (true)
                        {
                            if (!(await connection.RetryReciveAsync(buffer)).Succeeded)
                            {
                                _message = "Can not send data";
                                break;
                            }
                            else
                            {
                                if (buffer.Length > 0)
                                {
                                    string bufString = Encoding.UTF8.GetString(buffer);
                                    string[] stringArray = bufString.Split('d');
                                    _currentParameters[0] = double.Parse(stringArray[0], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"));
                                    _currentParameters[1] = double.Parse(stringArray[1], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"));
                                    _currentParameters[2] = double.Parse(stringArray[1], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"));
                                }

                            }
                        }
                    }
                    else
                    {
                        _message = "Can not connect";
                    }
                }
            }
        }

        public async Task<string> SendMode(BluetoothDeviceModel selectedDevice, string sendingParameters)
        {
            string message = "";
            if (selectedDevice != null)
            {
                IBluetoothAdapter bluetoothAdapter = DependencyService.Resolve<IBluetoothAdapter>();
                using (IBluetoothConnection connection = bluetoothAdapter.CreateConnection(selectedDevice))
                {
                    if (await connection.RetryConnectAsync(retriesCount: 3))
                    {
                        if (!sendingParameters.Contains("\n"))
                            sendingParameters += '\n';
                        char[] byteBuffer = sendingParameters.ToCharArray();
                        Encoding utf8 = Encoding.UTF8;
                        byte[] buffer = utf8.GetBytes(byteBuffer);
                        if (!await connection.RetryTransmitAsync(buffer, 0, buffer.Length))
                        {
                            message = "Can not send data";
                        }
                    }
                    else
                    {
                        message = "Can not connect";
                    }
                }
            }
            return message;
        }

    }
}
