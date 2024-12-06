using Serilog;
using vevorws2mqtt.Services.Consumed.Mqtt;

namespace vevorws2mqtt.Drivers
{
    public class DeviceManager
    {
        private Configuration.Configuration configuration { get; }
        private DriverManager driverManager { get; }
        private MqttConnection mqttConnection { get; }
        private List<IDevice> devices { get; set; }

        public DeviceManager(Configuration.Configuration configuration, DriverManager driverManager, MqttConnection mqttConnection)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.driverManager = driverManager ?? throw new ArgumentNullException(nameof(driverManager));
            this.mqttConnection = mqttConnection ?? throw new ArgumentNullException(nameof(mqttConnection));
        }

        public void DispatchWeatherUndergroundUpdate(string ID,
                         DateTimeOffset dateutc,
                         double baromin,
                         double tempf,
                         double humidity,
                         double dewptf,
                         double rainin,
                         double dailyrainin,
                         double winddir,
                         double windspeedmph,
                         double windgustmph,
                         double UV,
                         double solarRadiation)
        {
            var relevantDevices = devices.Where(x => x.DeviceID == ID && x.Driver is Drivers.YT60234.Driver).ToList();

            relevantDevices.ForEach(device => 
            {
                var driver = device.Driver as YT60234.Driver;
                driver?.DispatchWeatherUndergroundUpdate(ID, dateutc, baromin, tempf, humidity, dewptf, rainin, dailyrainin, winddir, windspeedmph,windgustmph, UV, solarRadiation);
            });
        }

        public void Setup()
        {
            var deviceIds = configuration.DeviceIds;

            var deviceStatuses = deviceIds.Select(deviceId => new
            {
                DeviceId = deviceId
            })
                                          .ToList();
            var supportedDevices = deviceStatuses.Select(entry => new
            {
                Drivers = (List<IDriver>)driverManager.SupportedDrivers(),
                Status = entry
            }).ToList();

            devices = supportedDevices.SelectMany(entry => entry.Drivers.Select(d => (IDevice)d.RegisterDevice(entry.Status.DeviceId))).ToList();

            deviceIds.ForEach(deviceId =>
            {
                var device = devices.Single(x => x.DeviceID == deviceId);

                Log.Information($"Configuring device '{deviceId}'.");

                device.Driver.ClearHADeviceRegistrations(device);
                device.Driver.PublishNewHADevices(device);

                mqttConnection.SubscribeToMessageReceived((a) =>
                {
                    var newValue = System.Text.Encoding.UTF8.GetString(a.ApplicationMessage.PayloadSegment.Array);
                    var topic = a.ApplicationMessage.Topic;

                    device.Driver.DispatchMqttMessage(device, topic, newValue);
                });
            });
        }
    }
}
