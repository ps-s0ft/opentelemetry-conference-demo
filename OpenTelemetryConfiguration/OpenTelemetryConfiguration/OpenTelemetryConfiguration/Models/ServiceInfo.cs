namespace OpenTelemetryConfiguration.Models
{
    /// <summary>
    /// Configuration model for service metadata.
    /// </summary>
    public sealed class ServiceInfo
    {
        /// <summary>
        /// Service name.
        /// </summary>
        public string Name { get; init; } = "UnknownService";

        /// <summary>
        /// Service version.
        /// </summary>
        public string Version { get; init; } = "1.0.0";

        /// <summary>
        /// Service environment.
        /// </summary>
        public string Environment { get; init; } = "Local";
    }
}
