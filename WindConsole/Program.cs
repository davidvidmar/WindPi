using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.ServiceBus.Messaging;
using WindConsole.Properties;

namespace WindConsole
{
    internal static class Program
    {
        private static RegistryManager _registryManager;

        private const string IotHubD2CEndpoint = "messages/events";
        private static EventHubClient _eventHubClient;

        private static void Main()
        {
            _registryManager = RegistryManager.CreateFromConnectionString(Settings.Default.IotHubConnectionString);

            try
            {
                Console.WriteLine("Adding device to IoT hub...\n");
                AddDeviceAsync().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to add device: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                Console.WriteLine();
            }

            Console.WriteLine("Receiving messages...\n");

            try
            {
                _eventHubClient = EventHubClient.CreateFromConnectionString(Settings.Default.IotHubConnectionString, IotHubD2CEndpoint);

                var d2CPartitions = _eventHubClient.GetRuntimeInformation().PartitionIds;
                foreach (var partition in d2CPartitions)
                {
                    ReceiveMessagesFromDeviceAsync(partition).Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure listening for messages: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                Console.WriteLine();
            }

            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }

        private static async Task AddDeviceAsync()
        {
            const string deviceId = "RPi-Wind-1";
            Device device;
            try
            {
                device = await _registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await _registryManager.GetDeviceAsync(deviceId);
            }
            Console.WriteLine("Generated device key: {0}\n", device.Authentication.SymmetricKey.PrimaryKey);
        }

        private static async Task ReceiveMessagesFromDeviceAsync(string partition)
        {
            var eventHubReceiver = _eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.Now);
            while (true)
            {
                var eventData = await eventHubReceiver.ReceiveAsync();
                if (eventData == null) continue;
                var data = Encoding.UTF8.GetString(eventData.GetBytes());

                Console.WriteLine("Message received. Partition: {0} Data: '{1}'", partition, data);
            }
        }

    }
}