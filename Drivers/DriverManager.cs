using vevorws2mqtt.Services.Consumed.Mqtt;
using System;
using System.Collections.Generic;
using System.Linq;

namespace vevorws2mqtt.Drivers
{
    public class DriverManager
    {
        private Configuration.Configuration configuration { get; }
        private MqttConnection mqttConnection { get; }
        private List<IDriver> drivers { get; set; }

        public DriverManager(Configuration.Configuration configuration, 
                             MqttConnection mqttConnection)
        {
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
            this.mqttConnection = mqttConnection ?? throw new ArgumentNullException(nameof(mqttConnection));
        }

        public void Setup()
        {
            // Get a list of drivers in this project
            var driverType = typeof(IDriver);
            var driverTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => driverType.IsAssignableFrom(p) &&
                            !p.IsInterface)
                .ToList();
            
            // Create instances of identified drivers
            drivers = driverTypes.Select(t => Activator.CreateInstance(t)).Cast<IDriver>().ToList();

            // Hook the drivers up to the mqtt connection
            drivers.ForEach(x => 
            {
                x.UseConfiguration(configuration);
                x.UseMqttClient(mqttConnection);               
            });
        }

        public List<IDriver> SupportedDrivers()
        {
            var result = drivers.ToList();

            return result;
        }
    }
}
