using Plugin.BluetoothClassic.Abstractions;

using System.Threading.Tasks;

namespace IServices
{
    public interface IBlueToothService
    {
        IBluetoothConnection CreateConnection(BluetoothDeviceModel selectedDevice);
        Task<(double[], string)> RecieveSensorsData(IBluetoothConnection connection);
         Task<string> SendMode(IBluetoothConnection connection, string sendingParameters);
    }
}