using IServices;

using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class SensorsDataRepository : BindableBase, ISensorsDataRepository
    {
        private double _currentTemperature;
        public double CurrentTemperature
        {
            get { return _currentTemperature; }
            set { SetProperty(ref _currentTemperature, value); }
        }

        private double _currentPressure;
        public double CurrentPressure
        {
            get { return _currentPressure; }
            set { SetProperty(ref _currentPressure, value); }
        }

        private double _currentPower;
        public double CurrentPower
        {
            get { return _currentPower; }
            set { SetProperty(ref _currentPower, value); }
        }

        private string[] _modes = { "По температуре", "По давлению", "Ручной" };
        public string[] Modes
        {
            get { return _modes; }
            set { SetProperty(ref _modes, value); }
        }

        private string _mode;
        public string Mode
        {
            get { return _mode; }
            set { SetProperty(ref _mode, value); }
        }
    }
}
