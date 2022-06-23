
using Android.Bluetooth;

using System;
using System.Collections.Generic;

namespace DeflectoreMobilePrism.Core
{
    public static class MainActivityModel
    {
        public static BluetoothAdapter BlutoothAdapter = BluetoothAdapter.DefaultAdapter;
        public static List<BluetoothDevice> BluetoothDevices = new List<BluetoothDevice>();
    }
}
