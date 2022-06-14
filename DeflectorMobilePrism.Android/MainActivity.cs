using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;

using AndroidX.Core.App;
using AndroidX.Core.Content;

using DeflectoreMobilePrism.Core;

using Prism;
using Prism.Ioc;

namespace DeflectorMobilePrism.Droid
{
    [Activity(Theme = "@style/MainTheme",
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);
            IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);
            IntentFilter pairedFilter = new IntentFilter(BluetoothDevice.ActionPairingRequest);
            DeviceDiscoveredReceiver receiver = new DeviceDiscoveredReceiver(this);
            RegisterReceiver(receiver, filter);
            RegisterReceiver(receiver, pairedFilter);
            #region Проверка на соответствующие разрашения  для  Bluetooth поисковика
            Permission coarseLocationPermissionGranted =
            ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation);
            // check if the app has permission to access fine location
            Permission fineLocationPermissionGranted =
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation);
            string[] locationPermissions = new[]
            {
                Manifest.Permission.AccessCoarseLocation,
                Manifest.Permission.AccessFineLocation
            };
            int locationPermissionsRequestCode = 1_000;
            if (coarseLocationPermissionGranted == Permission.Denied ||
               fineLocationPermissionGranted == Permission.Denied)
            {
                ActivityCompat.RequestPermissions(this, locationPermissions, locationPermissionsRequestCode);
                //ActivityCompat.RequestPermissions(this, locationPermissions, locationPermissionsRequestCode);
            }
            #endregion

            #region Создание Xamarin forms
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App(new AndroidInitializer()));
            #endregion
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    public class DeviceDiscoveredReceiver : BroadcastReceiver
    {
        Activity chatActivity;

        public DeviceDiscoveredReceiver(Activity chat)
        {
            this.chatActivity = chat;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;
            // When discovery finds a device
            if (action == BluetoothDevice.ActionFound)
            {
                // Get the BluetoothDevice object from the Intent
                BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                // If it's already paired, skip it, because it's been listed already

                if (device.BondState != Bond.Bonded && !string.IsNullOrEmpty(device.Name))
                {
                    MainActivityModel.BluetoothDevices.Add(device);
                }
                // When discovery is finished, change the Activity title
            }
            else if (action == BluetoothAdapter.ActionDiscoveryFinished)
            {
                //  chatActivity.SetProgressBarIndeterminateVisibility(false);
                //   chatActivity.SetTitle(Resource.String.select_device);
                MainActivityModel.BluetoothDevices.Clear();
            }
            else
            if (action == BluetoothDevice.ActionPairingRequest)
            {

            }
        }
    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register any platform specific implementations
        }
    }
}

