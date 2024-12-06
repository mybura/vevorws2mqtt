using Serilog;
using vevorws2mqtt.Drivers;
using vevorws2mqtt.Services.Consumed.Mqtt;

namespace vevorws2mqtt
{ 
    public class Program
    {
        private static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            SetupLogging(builder);
            GetSettings(args, builder);
            SetupDI(builder);

            var host = builder.Build();

            host?.Services?.GetService<MqttConnection>()?.Setup();
            host?.Services?.GetService<DriverManager>()?.Setup();
            host?.Services?.GetService<DeviceManager>()?.Setup();

            ListenForWeatherUndergroundUpdates(host);

            // Loop, allowing event handlers to process messages received via mqtt and regular timers
            host?.Run();
        }

        private static void ListenForWeatherUndergroundUpdates(WebApplication? host)
        {
            var manager = host?.Services?.GetService<DeviceManager>();

            host?.MapGet("/weatherstation/updateweatherstation.php",
                        (string ID,
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
                         double solarRadiation) =>
                        {
                            manager.DispatchWeatherUndergroundUpdate(ID, dateutc, baromin, tempf, humidity, dewptf, rainin, dailyrainin, winddir, windspeedmph, windgustmph, UV, solarRadiation);
                        });
        }

        private static void SetupDI(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<MqttConnection>();
        }

        private static void SetupLogging(WebApplicationBuilder builder)
        {
            var logger = new LoggerConfiguration()
                                .WriteTo.Console()
                                .CreateLogger();

            Log.Logger = logger;

            builder.Services.AddSingleton(logger);
        }

        private static void GetSettings(string[] args, WebApplicationBuilder builder)
        {
            // Configure where configuration is retrieved from
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("settings.json", optional: true, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();

            if (args is { Length: > 0 })
            {
                builder.Configuration.AddCommandLine(args);
            }

            // Register the configuration with DI
            var configuration = builder.Configuration.Get<Configuration.Configuration>();
            builder.Services.AddSingleton(configuration);
                builder.Services.AddSingleton<DriverManager>();
                builder.Services.AddSingleton<DeviceManager>();
            }
        }
}
