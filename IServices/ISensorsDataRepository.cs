namespace IServices
{
    public interface ISensorsDataRepository
    {
        double CurrentPower { get; set; }
        double CurrentPressure { get; set; }
        double CurrentTemperature { get; set; }
        string Mode { get; set; }
        string[] Modes { get; set; }
    }
}