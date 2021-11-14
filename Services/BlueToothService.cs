using IServices;

using Plugin.BluetoothClassic.Abstractions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Services
{
    public class BlueToothService
    {
        public BlueToothService(ISensorsDataRepository sensorsDataRepository)
        {
            SensorsDataRepository = sensorsDataRepository;
        }

        public ISensorsDataRepository SensorsDataRepository { get; }

        public async Task<string> RecieveSensorsData(BluetoothDeviceModel selectedDevice)
        {
            string message = "";
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
                                message = "Can not send data";
                                break;
                            }
                            else
                            {
                                if (buffer.Length > 0)
                                {
                                    string bufString = Encoding.UTF8.GetString(buffer);
                                    string[] stringArray = bufString.Split('d');
                                    SensorsDataRepository.CurrentTemperature = double.Parse(stringArray[0], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"));
                                    SensorsDataRepository.CurrentPressure = double.Parse(stringArray[1], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"));
                                    SensorsDataRepository.CurrentPower = double.Parse(stringArray[1], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"));
                                }

                            }
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

        public async Task<string> SendMode(BluetoothDeviceModel selectedDevice)
        {
            string message = "";
            if (selectedDevice != null)
            {
                IBluetoothAdapter bluetoothAdapter = DependencyService.Resolve<IBluetoothAdapter>();
                using (IBluetoothConnection connection = bluetoothAdapter.CreateConnection(selectedDevice))
                {
                    if (await connection.RetryConnectAsync(retriesCount: 3))
                    {
                        if (!SensorsDataRepository.Mode.Contains("\n"))
                            SensorsDataRepository.Mode += '\n';
                        char[] byteBuffer = SensorsDataRepository.Mode.ToCharArray();
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
