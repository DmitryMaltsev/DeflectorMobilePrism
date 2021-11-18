using Plugin.BluetoothClassic.Abstractions;
using System.Threading.Tasks;

namespace IServices
{
    public interface IBlueToothService
    {
        (string, double[]) ResultRecieveSensorData(BluetoothDeviceModel selectedDevice);
        Task<string> SendMode(BluetoothDeviceModel selectedDevice, string sendingParameters);
    }
}