using Prism.Ioc;
using Prism.Modularity;
using OperationsModule.Views;
using OperationsModule.ViewModels;

namespace OperationsModule
{
    public class ModuleName : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ChangeModes, ChangeModesViewModel>();
        }
    }
}
