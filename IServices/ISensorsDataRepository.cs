﻿using System.Collections.Generic;

namespace IServices
{
    public interface ISensorsDataRepository
    {
        double CurrentPower { get; set; }
        double CurrentPressure { get; set; }
        double CurrentTemperature { get; set; }
        string Mode { get; set; }
        List<string> Modes { get; set; }
        int DecimalNum { get; set; }
        int UnitNum { get; set; }
        bool NumsOn { get; set; }
        string SelectedMode { get; set; }
        int FloorNumber { get; set; }
    }
}