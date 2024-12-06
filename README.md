# vevor2mqtt

Bridge Vevor brand devices to MQTT. [Home Assistant](https://www.home-assistant.io) [MQTT integration](https://www.home-assistant.io/integrations/mqtt) automatically recognises the devices.

Using Home Assistant UI, one can monitor the sensors exposed by the Vevor devices.

Currently only supports YT60234 models.

## Usage

To be run as a docker instance (tested on Linux host, Windows should work if you build a Windows image using this repo)

### Required:

1. Network traffic from YT60234 device forwarded to the docker machine.
2. Server address of an MQTT broker (e.g. [mosquitto](https://mosquitto.org/)) requiring no credentials
3. [Working Docker host](https://www.tutorialspoint.com/docker/docker_installation.htm)

### Example:

- MQTT server IP = 10.10.10.11
- Desired docker instance name = vevor2mqtt

```bash
docker run --detach --env MqttServer=10.10.10.11 --env DeviceIds__0=Local --name vevor2mqtt mybura/vevor2mqtt:latest
```
## Notes

- The code can be run as an executable. Just download this repo and build/run it from Visual Studio 2022.
- Additional settings are available, see the Configuration class.
- Supports Settings.json instead of environment variable configuration.
- Other devices can be added by creating extra drivers. An example is in the Drivers/YT60234 folder. Some exploration of supported SmartThings attributes and behaviours are needed. The framework automatically detects new drivers by checking for classes that implement IDriver.
 
## License

The MIT License (MIT)