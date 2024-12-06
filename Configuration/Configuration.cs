using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vevorws2mqtt.Configuration
{
    public class Configuration
    {
        public string MqttServer { get; set; }
        public HomeAssistantConfiguration HomeAssistant { get; set; } = new HomeAssistantConfiguration();
        public IntervalConfiguration Intervals { get; set; } = new IntervalConfiguration();

        public List<string> DeviceIds { get; set; } = new List<string>();

        public string ThisAppName { get; set; } = "vevorws2mqtt";
        public string ThisVersion { get; set; } = "1.0.0";

        public MqttConfiguration Mqtt { get; set; } = new MqttConfiguration();
    }
}