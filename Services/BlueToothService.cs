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
    public class BlueToothService : IBlueToothService
    {
        public BlueToothService()
        {
        }

        public async Task<(double[], string)> RecieveSensorsData(BluetoothDeviceModel selectedDevice)
        {
            double[] currentParameters = new double[3];
            string message = "";
            if (selectedDevice != null)
            {
                byte[] buffer = new byte[12];
                IBluetoothAdapter adapter = DependencyService.Resolve<IBluetoothAdapter>();
                using (IBluetoothConnection connection = adapter.CreateConnection(selectedDevice))
                {

                    if (await connection.RetryConnectAsync(retriesCount: 3))
                    {

                        if (!(await connection.RetryReciveAsync(buffer)).Succeeded)
                        {
                            message = "Can not send data";
                        }
                        else
                        {
                            if (buffer.Length > 0)
                            {
                                string bufString = Encoding.UTF8.GetString(buffer);
                                string[] stringArray = bufString.Split('d');
                                currentParameters[0] = double.Parse(stringArray[0], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"));
                                currentParameters[1] = double.Parse(stringArray[1], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"));
                                currentParameters[2] = double.Parse(stringArray[1], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"));
                            }

                        }
                    }
                    else
                    {
                        message = "Can not connect";
                    }
                }
            }
            return await Task.Run(() => (currentParameters, message));
         //   return await Task.Run(() => message);
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
            return await Task.Run(() => message);
        }
    }
}
