using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.ServiceBus.Messaging;

namespace WindConsole
{
    internal static class Program
    {
        private static RegistryManager _registryManager;
        private const string ConnectionString = "HostName=WindHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=Qim461LUqPRvfkSsdpRlq1CRNjII3cg++lViTnDVqTk=";

        private static string _iotHubD2CEndpoint = "messages/events";
        private static EventHubClient _eventHubClient;

        static void Main()
        {
            _registryManager = RegistryManager.CreateFromConnectionString(ConnectionString);

            Console.WriteLine("Adding device to IoT hub...");
            AddDeviceAsync().Wait();

            Console.WriteLine("Receive messages:\n");
            _eventHubClient = EventHubClient.CreateFromConnectionString(ConnectionString, _iotHubD2CEndpoint);

            var d2CPartitions = _eventHubClient.GetRuntimeInformation().PartitionIds;
            foreach (var partition in d2CPartitions)
            {
                ReceiveMessagesFromDeviceAsync(partition);
            }

            Console.WriteLine("\nPress Enter to continue.");
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
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
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