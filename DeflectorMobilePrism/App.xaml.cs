using DeflectorMobilePrism.ViewModels;
using DeflectorMobilePrism.Views;

using IServices;

using OperationsModule.ViewModels;
using OperationsModule.Views;

using Prism;
using Prism.Ioc;

using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace DeflectorMobilePrism
{
    public partial class App
    {
        public App(IPlatformInitializer initializer)
            : base(initializer)
        {
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            await NavigationService.NavigateAsync("MainPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();
            containerRegistry.RegisterSingleton<IBlueToothService, IBlueToothService>();
            containerRegistry.RegisterSingleton<ISensorsDataRepository, ISensorsDataRepository>();
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();
            containerRegistry.RegisterForNavigation<ChangeModes, ChangeModesViewModel>();
        }
    }
}
