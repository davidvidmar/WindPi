namespace WindPi.Helpers
{
    /// <summary>
    /// Class AppSettings for specific settings for application
    /// </summary>
    public static class AppSettings
    {
        /// <summary>
        /// The iot hub URI
        /// </summary>
        /// <remarks>
        ///     name of your Azure IOT Hub, when you create it
        ///     [yourname].azure-devices.net
        /// </remarks>
        public const string IotHubUri = "WindHub.azure-devices.net";
        /// <summary>
        /// The device identifier
        /// </summary>
        /// <remarks>
        ///     contains your own name, which will be set, when <c>DeviceClient</c> class will call
        ///     Azure IOT Hub Api to register your device
        /// </remarks>
        public const string DeviceId = "RPi-Wind-1";
        /// <summary>
        /// The device key
        /// </summary>
        /// <remarks>
        ///     reference to the Azure IOT hub developer guide - 
        ///     https://azure.microsoft.com/en-us/documentation/articles/iot-hub-devguide/#security
        /// </remarks>
        public const string DeviceKey = "aaRE0MhoR5XQBZTecv3VO5mfPymgOJtmbYn4ZusGlzU=";
    }
}