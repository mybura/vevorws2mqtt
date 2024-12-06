using Serilog;
using vevorws2mqtt.Services.Consumed.Mqtt;
using vevorwsmqtt.Drivers.YT60234;
using System.Globalization;

namespace vevorws2mqtt.Drivers.YT60234
{
    public class Driver : IDriver
    {
        private MqttConnection mqttConnection;
        private Configuration.Configuration configuration;

        public void UseMqttClient(MqttConnection mqttConnection)
        {
            this.mqttConnection = mqttConnection;
        }

        public void UseConfiguration(Configuration.Configuration configuration)
        {
            this.configuration = configuration;
        }

        public void PublishNewHADevices(IDevice device)
        {
            PublishNewTempSensorDeviceForHA((Device)device);
            PublishNewDewPointTempSensorDeviceForHA((Device)device);
            PublishNewHumiditySensorDeviceForHA((Device)device);
            PublishNewPressureSensorDeviceForHA((Device)device);
            PublishNewLightSensorDeviceForHA((Device)device);
            PublishNewWindSpeedSensorDeviceForHA((Device)device);
            PublishNewWindDirectionSensorDeviceForHA((Device)device);
            PublishNewRainfallDailySensorDeviceForHA((Device)device);
            PublishNewRainfallHourlySensorDeviceForHA((Device)device);
            PublishNewUVIndexSensorDeviceForHA((Device)device);
            PublishNewDateSensorDeviceForHA((Device)device);
            PublishNewWindSpeedGustSensorDeviceForHA((Device)device);
            PublishNewSolarRadiationSensorDeviceForHA((Device)device);

            RunRefreshLoop(device.DeviceID, CancellationToken.None);
        }

        public void ClearHADeviceRegistrations(IDevice device)
        {
            PublishClearDeviceForHA(ETopic.HAHumiditySensorConfig, device.DeviceID);
            PublishClearDeviceForHA(ETopic.HAPresureSensorConfig, device.DeviceID);
            PublishClearDeviceForHA(ETopic.HATempSensorConfig, device.DeviceID);
            PublishClearDeviceForHA(ETopic.HADewPointTempSensorConfig, device.DeviceID);
            PublishClearDeviceForHA(ETopic.HALightLevelSensorConfig, device.DeviceID);
            PublishClearDeviceForHA(ETopic.HAWindSpeedSensorConfig, device.DeviceID);
            PublishClearDeviceForHA(ETopic.HAWindSpeedGustSensorConfig, device.DeviceID);
            PublishClearDeviceForHA(ETopic.HAWindDirectionSensorConfig, device.DeviceID);
            PublishClearDeviceForHA(ETopic.HARainFallDailySensorConfig, device.DeviceID);
            PublishClearDeviceForHA(ETopic.HARainFallHourlySensorConfig, device.DeviceID);
            PublishClearDeviceForHA(ETopic.HAUVIndexSensorConfig, device.DeviceID);
            PublishClearDeviceForHA(ETopic.HALastUpdateSensorConfig, device.DeviceID);
            PublishClearDeviceForHA(ETopic.HASolarRadiationSensorConfig, device.DeviceID);            
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
            SendPayload(ID, ETopic.GetHumidity, $@"{humidity}");
            SendPayload(ID, ETopic.GetTemperature, $@"{(tempf - 32.0) * (5.0 / 9.0)}");
            SendPayload(ID, ETopic.GetAtmosphericPressure, $@"{baromin * 3.386}");
            SendPayload(ID, ETopic.GetLightLevel, $@"{solarRadiation / 0.0083}");
            SendPayload(ID, ETopic.GetSolarRadiation, $@"{solarRadiation}");
            SendPayload(ID, ETopic.GetWindSpeed, $@"{windspeedmph * 1.609}");
            SendPayload(ID, ETopic.GetWindGust, $@"{windgustmph * 1.609}");
            SendPayload(ID, ETopic.GetUVIndex, $@"{UV}");
            SendPayload(ID, ETopic.GetDewPoint, $@"{(dewptf - 32.0) * (5.0 / 9.0)}");
            SendPayload(ID, ETopic.GetWindDirection, $@"{winddir}");
            SendPayload(ID, ETopic.GetHourlyRainfall, $@"{rainin * 25.4}");
            SendPayload(ID, ETopic.GetDailyRainfall, $@"{dailyrainin * 25.4}");
            SendPayload(ID, ETopic.GetLastUpdateDate, $@"{dateutc.ToString("o", CultureInfo.InvariantCulture)}");
        }

        public void DispatchMqttMessage(IDevice device, string topic, string newValue)
        {
            // Don't expect any requests from mqtt, so this is not needed
        }

        public IDevice RegisterDevice(string stDeviceId)
        {
            var newDevice = new Device();
            newDevice.DeviceID = stDeviceId;
            newDevice.LastUpdateDateUtc = DateTime.MinValue.ToUniversalTime();
            newDevice.Driver = this;

            return newDevice;
        }

        public string GetTopic(string deviceId, ETopic topic)
        {
            switch (topic)
            {
                case ETopic.HATempSensorConfig:
                    return $"{configuration.HomeAssistant.HaDiscoveryTopicPrefix}/sensor/{deviceId}_temperature/config";
                case ETopic.HADewPointTempSensorConfig:
                    return $"{configuration.HomeAssistant.HaDiscoveryTopicPrefix}/sensor/{deviceId}_temperature_dewpoint/config";
                case ETopic.HAPresureSensorConfig:
                    return $"{configuration.HomeAssistant.HaDiscoveryTopicPrefix}/sensor/{deviceId}_atmospheric_pressure/config";
                case ETopic.HALastUpdateSensorConfig:
                    return $"{configuration.HomeAssistant.HaDiscoveryTopicPrefix}/sensor/{deviceId}_last_update/config";
                case ETopic.HAHumiditySensorConfig:
                    return $"{configuration.HomeAssistant.HaDiscoveryTopicPrefix}/sensor/{deviceId}_humidity/config";
                case ETopic.HAWindSpeedSensorConfig:
                    return $"{configuration.HomeAssistant.HaDiscoveryTopicPrefix}/sensor/{deviceId}_windspeed/config";
                case ETopic.HAWindSpeedGustSensorConfig:
                    return $"{configuration.HomeAssistant.HaDiscoveryTopicPrefix}/sensor/{deviceId}_windspeed_gust/config";
                case ETopic.HALightLevelSensorConfig:
                    return $"{configuration.HomeAssistant.HaDiscoveryTopicPrefix}/sensor/{deviceId}_lightlevel/config";
                case ETopic.HAWindDirectionSensorConfig:
                    return $"{configuration.HomeAssistant.HaDiscoveryTopicPrefix}/sensor/{deviceId}_winddirection/config";
                case ETopic.HARainFallDailySensorConfig:
                    return $"{configuration.HomeAssistant.HaDiscoveryTopicPrefix}/sensor/{deviceId}_rainfall_daily/config";
                case ETopic.HASolarRadiationSensorConfig:
                    return $"{configuration.HomeAssistant.HaDiscoveryTopicPrefix}/sensor/{deviceId}_solarradiation/config";
                case ETopic.HARainFallHourlySensorConfig:
                    return $"{configuration.HomeAssistant.HaDiscoveryTopicPrefix}/sensor/{deviceId}_rainfall_hourly/config";
                case ETopic.HAUVIndexSensorConfig:
                    return $"{configuration.HomeAssistant.HaDiscoveryTopicPrefix}/sensor/{deviceId}_uv_index/config";
                case ETopic.GetState:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/mode";
                case ETopic.GetTemperature:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/temperature";
                case ETopic.GetAtmosphericPressure:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/atmospheric_pressure";
                case ETopic.GetHumidity:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/humidity";
                case ETopic.GetLightLevel:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/light_level";
                case ETopic.GetSolarRadiation:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/solarradiation";
                case ETopic.GetWindSpeed:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/wind_speed";
                case ETopic.GetWindGust:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/wind_gust";
                case ETopic.GetUVIndex:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/UV_index";
                case ETopic.GetDewPoint:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/dewpoint";
                case ETopic.GetWindDirection:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/wind_direction";
                case ETopic.GetHourlyRainfall:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/rainfall_hourly";
                case ETopic.GetDailyRainfall:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/rainfall_daily";
                case ETopic.GetLastUpdateDate:
                    return $"{configuration.ThisAppName}/weatherstation/{deviceId}/last_updated";
                default:
                    throw new NotSupportedException($"Topic not supported: '{topic}'");
            }
        }

        void PublishClearDeviceForHA(ETopic topic, string deviceId)
        {
            var pauloadTopic = GetTopic(deviceId, topic);

            var configPayload = "{}";

            mqttConnection.SendMessage(pauloadTopic, configPayload, true);
        }

        void PublishNewSensorDeviceForHA(Device device, ETopic deviceTopic, ETopic stateTopic, string sensorClass, string deviceClass, string uom, string name, int precission = 0)
        {
            var topic = GetTopic(device.DeviceID, deviceTopic);
            var deviceClassProperty = $@"""device_class"" : ""{deviceClass}"",";

            if (string.IsNullOrWhiteSpace(deviceClass))
                deviceClassProperty = "";

            var configPayload = $@"
    {{ 
        ""name"":""{name}"",
        ""unique_id"" : ""{device.DeviceID}_{deviceClass}_{name.Replace(" ", "_")}"",
        ""sw_version"" : ""{configuration.ThisVersion}"",
        {deviceClassProperty}
        ""suggested_display_precision"" : {precission},
        ""state_class"" : ""{sensorClass}"",
        ""unit_of_measurement"" : ""{uom}"",
        ""state_topic"" : ""{GetTopic(device.DeviceID, stateTopic)}"",
        ""device"" : 
        {{
            ""model"" : ""YT60234"",
            ""name"" : ""Weather Station"",
            ""manufacturer"" : ""Vevor"",
            ""suggested_area"" : ""Outside"",
            ""via_device"" : ""{configuration.ThisAppName}"",
            ""identifiers"" : [""{device.DeviceID}"", ""YT60234""]
        }}
    }}";

            mqttConnection.SendMessage(topic, configPayload, true);
        }

        public void PublishNewHumiditySensorDeviceForHA(Device device)
        {
            PublishNewSensorDeviceForHA(device, ETopic.HAHumiditySensorConfig, ETopic.GetHumidity, "measurement", "humidity", "%", "Humidity");
        }

        public void PublishNewDateSensorDeviceForHA(Device device)
        {
            PublishNewSensorDeviceForHA(device, ETopic.HALastUpdateSensorConfig, ETopic.GetLastUpdateDate, "measurement", "timestamp", "", "LastUpdated");
        }

        public void PublishNewPressureSensorDeviceForHA(Device device)
        {
            PublishNewSensorDeviceForHA(device, ETopic.HAPresureSensorConfig, ETopic.GetAtmosphericPressure, "measurement", "atmospheric_pressure", "kPa", "AtmosphericPressure");
        }
        
        public void PublishNewDewPointTempSensorDeviceForHA(Device device)
        {
            PublishNewSensorDeviceForHA(device, ETopic.HADewPointTempSensorConfig, ETopic.GetDewPoint, "measurement", "temperature", $"°C", "DewPointTemperature", 1);
        }

        public void PublishNewTempSensorDeviceForHA(Device device)
        {
            PublishNewSensorDeviceForHA(device, ETopic.HATempSensorConfig, ETopic.GetTemperature, "measurement", "temperature", $"°C", "Temperature", 1);
        }

        public void PublishNewLightSensorDeviceForHA(Device device)
        {
            PublishNewSensorDeviceForHA(device, ETopic.HALightLevelSensorConfig, ETopic.GetLightLevel, "measurement", "illuminance", $"lx", "LightLevel", 2);
        }

        public void PublishNewSolarRadiationSensorDeviceForHA(Device device)
        {
            PublishNewSensorDeviceForHA(device, ETopic.HASolarRadiationSensorConfig, ETopic.GetSolarRadiation, "measurement", null, $"w/m²", "SolarRadiation");
        }
        
        public void PublishNewWindSpeedSensorDeviceForHA(Device device)
        {
            PublishNewSensorDeviceForHA(device, ETopic.HAWindSpeedSensorConfig, ETopic.GetWindSpeed, "measurement", "wind_speed", $"km/h", "WindSpeed", 2);
        }

        public void PublishNewWindSpeedGustSensorDeviceForHA(Device device)
        {
            PublishNewSensorDeviceForHA(device, ETopic.HAWindSpeedGustSensorConfig, ETopic.GetWindGust, "measurement", "wind_speed", $"km/h", "GustWindSpeed", 2);
        }

        public void PublishNewWindDirectionSensorDeviceForHA(Device device)
        {
            // TODO Fix, none is not supported and the field must be missing in the config tpoic for it to be a generic measurement
            PublishNewSensorDeviceForHA(device, ETopic.HAWindDirectionSensorConfig, ETopic.GetWindDirection, "measurement", null, $"°", "WindDirection");
        }

        public void PublishNewRainfallDailySensorDeviceForHA(Device device)
        {
            PublishNewSensorDeviceForHA(device, ETopic.HARainFallDailySensorConfig, ETopic.GetDailyRainfall, "measurement", "precipitation_intensity", $"mm/d", "DailyRain");
        }

        public void PublishNewRainfallHourlySensorDeviceForHA(Device device)
        {
            PublishNewSensorDeviceForHA(device, ETopic.HARainFallHourlySensorConfig, ETopic.GetHourlyRainfall, "measurement", "precipitation_intensity", $"mm/h", "HourlyRain");
        }

        public void PublishNewUVIndexSensorDeviceForHA(Device device)
        {
            PublishNewSensorDeviceForHA(device, ETopic.HAUVIndexSensorConfig, ETopic.GetUVIndex, "measurement", null, $"", "UVIndex");
        }

        private void SendPayload(string deviceId, ETopic topic, string payload)
        {
            var stateTopic = GetTopic(deviceId, topic);

            mqttConnection.SendMessage(stateTopic, payload);
        }

        void RunRefreshLoop(string deviceId, CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        mqttConnection.EnsureConnected();   // Ecosystem just not stable enough, make sure mqtt connection is up before doing the refresh.
                        Task.Delay(configuration.Intervals.UpdateDelay, cancellationToken).Wait();
                        Task.Delay(configuration.Intervals.UpdateInterval, cancellationToken).Wait();
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(100);
                        Log.Error(ex, "Unexpected error while refreshing device state.");
                    }
                }
            });
        }
    }
}
