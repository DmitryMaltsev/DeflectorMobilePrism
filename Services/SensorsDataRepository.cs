using IServices;

using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class SensorsDataRepository : BindableBase, ISensorsDataRepository
    {
        public SensorsDataRepository()
        {
            Modes = new List<string> { "По температуре", "По давлению", "Ручной" };
            Mode = Modes[0];
        }

        private int _floorNumber=-1;
        public int FloorNumber
        {
            get { return _floorNumber; }
            set { SetProperty(ref _floorNumber, value); }
        }   

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

        private List<string> _modes;
        public List<string> Modes
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


        /// <summary>
        /// Какой индекс режима выбран
        /// </summary>
        private int _selectedModeIndex=-1;
        public int SelectedModeIndex
        {
            get { return _selectedModeIndex; }
            set { SetProperty(ref _selectedModeIndex, value); }
        }
        private int _decimalNum;
        public int DecimalNum
        {
            get { return _decimalNum; }
            set { SetProperty(ref _decimalNum, value); }
        }

        private int _unitNum;
        public int UnitNum
        {
            get { return _unitNum; }
            set { SetProperty(ref _unitNum, value); }
        }

        private bool _numsOn=false;
        public bool NumsOn
        {
            get { return _numsOn; }
            set { SetProperty(ref _numsOn, value); }
        }
    }
}
