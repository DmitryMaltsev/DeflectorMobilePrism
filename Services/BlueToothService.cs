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

        public IBluetoothConnection CreateConnection(BluetoothDeviceModel selectedDevice)
        {

            IBluetoothAdapter blueToothAdapter = DependencyService.Resolve<IBluetoothAdapter>();
            using (IBluetoothConnection connection = blueToothAdapter.CreateConnection(selectedDevice))
                return connection;
        }

        public async Task<(double[], string)> RecieveSensorsData(IBluetoothConnection connection)
        {
            double[] currentParameters = new double[6];
            string message = "";
            byte[] buffer = new byte[25];
            if (!(await connection.RetryReciveAsync(buffer, 0, buffer.Length)).Succeeded)
            {
                message = "Ошибка соединения(получ)";
            }
            else
            {
                string bufString = Encoding.UTF8.GetString(buffer);
                string[] stringArray = bufString.Split('d');
                currentParameters[0] = double.Parse(stringArray[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                currentParameters[1] = double.Parse(stringArray[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                currentParameters[2] = double.Parse(stringArray[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                currentParameters[3] = double.Parse(stringArray[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                currentParameters[4] = double.Parse(stringArray[4], NumberStyles.Any, CultureInfo.InvariantCulture);
                currentParameters[5] = double.Parse(stringArray[5], NumberStyles.Any, CultureInfo.InvariantCulture);
                message = "Работа в норме(получ)";
            }
            return (currentParameters, message);
        }

        public async Task<string> SendMode(IBluetoothConnection connection, string sendingParameters)
        {
            string message = "";
            using (connection)
            {

                if (!sendingParameters.Contains("\n"))
                    sendingParameters += '\n';
                char[] byteBuffer = sendingParameters.ToCharArray();
                Encoding utf8 = Encoding.UTF8;
                byte[] buffer = utf8.GetBytes(byteBuffer);
                await connection.RetryTransmitAsync(buffer, 0, buffer.Length);
            }
            return message;
        }
    }
}
