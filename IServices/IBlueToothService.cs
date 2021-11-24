using Plugin.BluetoothClassic.Abstractions;

using System.Threading.Tasks;

namespace IServices
{
    public interface IBlueToothService
    {
        Task<(double[], string)> RecieveSensorsData(BluetoothDeviceModel selectedDevice);
         Task<string> SendMode(BluetoothDeviceModel selectedDevice, string sendingParameters);
    }
}