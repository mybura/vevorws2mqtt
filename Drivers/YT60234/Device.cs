using vevorws2mqtt.Drivers;

namespace vevorwsmqtt.Drivers.YT60234
{
    public class Device : IDevice
    {
        public string DeviceID { get; set; }
        public IDriver Driver { get; set; }
        public DateTimeOffset LastUpdateDateUtc { get; set; }
        public double Pressure_Inches { get; set; }
        public double Temperature_Farenheit { get; set; }
        public double Humidity { get; set; }
        public double DewPoint_Farenheit { get; set; }
        public double Rainfall_Inches { get; set; }
        public double DailyRainfall_Inches { get; set; }
        public double WindDirection_Degrees { get; set; }
        public double WindSpeed_Mph { get; set; }
        public double WindGust_Mph { get; set; }
        public double UVIndex { get; set; }
        public double SolarRadiation { get; set; }

    }
}
