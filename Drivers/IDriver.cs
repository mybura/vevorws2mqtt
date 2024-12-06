using Newtonsoft.Json.Linq;
using vevorws2mqtt.Services.Consumed.Mqtt;

namespace vevorws2mqtt.Drivers
{
    public interface IDriver
    {
        void UseMqttClient(MqttConnection mqttConnection);
        void UseConfiguration(Configuration.Configuration configuration);
        IDevice RegisterDevice(string stDeviceId);
        void PublishNewHADevices(IDevice device);
        void ClearHADeviceRegistrations(IDevice device);
        void DispatchMqttMessage(IDevice device, string topic, string newValue);
    }
}
