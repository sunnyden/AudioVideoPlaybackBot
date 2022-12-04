using Microsoft.Skype.Bots.Media;
using SkypeTeamsMeetingBot.AudioVideoPlaybackBot.FrontEnd;
using SkypeTeamsMeetingBot.AudioVideoPlaybackBot.FrontEnd.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SkypeTeamsMeetingBot
{
    /// <summary>
    /// Class AzureSettings.
    /// Implements the <see cref="EchoBot.Services.Contract.IAzureSettings" />
    /// </summary>
    /// <seealso cref="EchoBot.Services.Contract.IAzureSettings" />
    public class WindowsServiceConfiguration : IConfiguration
    {

        /// <summary>
        /// Gets or sets the name of the bot.
        /// </summary>
        /// <value>The name of the bot.</value>
        public string BotName { get; set; }

        /// <summary>
        /// Gets or sets the name of the service DNS.
        /// </summary>
        /// <value>The name of the service DNS.</value>
        public string ServiceDnsName { get; set; }

        /// <summary>
        /// Gets or sets the service cname.
        /// </summary>
        /// <value>The service cname.</value>
        public string ServiceCname { get; set; }

        /// <summary>
        /// Gets or sets the certificate thumbprint.
        /// </summary>
        /// <value>The certificate thumbprint.</value>
        public string CertificateThumbprint { get; set; }

        /// <summary>
        /// Gets or sets the call control listening urls.
        /// </summary>
        /// <value>The call control listening urls.</value>
        public IEnumerable<string> CallControlListeningUrls { get; set; }

        /// <inheritdoc/>
        public Uri PlaceCallEndpointUrl { get; private set; }

        /// <summary>
        /// Gets or sets the call control base URL.
        /// </summary>
        /// <value>The call control base URL.</value>
        public Uri CallControlBaseUrl { get; set; }

        /// <summary>
        /// Gets the media platform settings.
        /// </summary>
        /// <value>The media platform settings.</value>
        public MediaPlatformSettings MediaPlatformSettings { get; private set; }

        /// <summary>
        /// Gets or sets the aad application identifier.
        /// </summary>
        /// <value>The aad application identifier.</value>
        public string AadAppId { get; set; }

        /// <summary>
        /// Gets or sets the aad application secret.
        /// </summary>
        /// <value>The aad application secret.</value>
        public string AadAppSecret { get; set; }

        /// <summary>
        /// Gets the h264 1280 x 720 file location.
        /// </summary>
        public string H2641280X72030FpsFile { get; private set; }

        /// <summary>
        /// Gets the h264 640 x 360 file location.
        /// </summary>
        public string H264640X36030FpsFile { get; private set; }

        /// <summary>
        /// Gets the h264 320 x 180 file location.
        /// </summary>
        public string H264320X18015FpsFile { get; private set; }


        /// <inheritdoc/>
        public Dictionary<string, VideoFormat> H264FileLocations { get; private set; }

        /// <inheritdoc/>
        public string AudioFileLocation { get; private set; }

        /// <inheritdoc/>
        public int AudioVideoFileLengthInSec { get; private set; }

        /// <summary>
        /// Gets the h264 1920 x 1080 vbss file location.
        /// </summary>
        public string H2641920X108015VBSSFpsFile { get; private set; }

        public string BotInternalHostingProtocol = "https";

        /// <summary>
        /// Gets or sets the instance listening port.
        /// </summary>
        /// <value>The instance listening port.</value>
        public int MediaStreamingPort { get; set; }
        public int BotSignalingPort { get; set; }

        public string MediaDnsName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureConfiguration"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public WindowsServiceConfiguration(EnvironmentVarConfigs envConfigs)
        {
            if (!System.Diagnostics.EventLog.SourceExists(SampleConstants.EventLogSource))
            {
                EventLog.CreateEventSource(SampleConstants.EventLogSource, SampleConstants.EventLogType);
            }
            EventLog.WriteEntry(SampleConstants.EventLogSource, $"Initializing {nameof(WindowsServiceConfiguration)}", EventLogEntryType.Warning);

            this.MapEnvironmentVars(envConfigs);
            this.Initialize();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            this.MediaDnsName = ServiceDnsName;

            // Enivonment Var Validations 
            if (string.IsNullOrEmpty(ServiceDnsName)) throw new ArgumentNullException(nameof(ServiceDnsName));
            if (string.IsNullOrEmpty(CertificateThumbprint)) throw new ArgumentNullException(nameof(CertificateThumbprint));
            if (string.IsNullOrEmpty(AadAppId)) throw new ArgumentNullException(nameof(AadAppId));
            if (string.IsNullOrEmpty(AadAppSecret)) throw new ArgumentNullException(nameof(AadAppSecret));
            if (BotSignalingPort == 0) throw new ArgumentOutOfRangeException(nameof(BotSignalingPort));
            if (MediaStreamingPort == 0) throw new ArgumentOutOfRangeException(nameof(MediaStreamingPort));

            EventLog.WriteEntry(SampleConstants.EventLogSource, $"WindowsServiceConfiguration Cname {this.ServiceCname} DNS {this.ServiceDnsName}", EventLogEntryType.Warning);
            if (string.IsNullOrEmpty(this.ServiceCname))
            {
                this.ServiceCname = this.ServiceDnsName;
            }

            this.PlaceCallEndpointUrl = new Uri("https://graph.microsoft.com/v1.0");

            X509Certificate2 defaultCertificate = this.GetCertificateFromStore();

            // localhost
            var baseDomain = "localhost";

            // externall URLs always are https
            var botCallingUrl = $"https://*:{BotSignalingPort}/joinCall";
            var botInstanceUrl = $"https://{ServiceCname}:{BotSignalingPort}/{HttpRouteConstants.CallSignalingRoutePrefix}/{HttpRouteConstants.OnNotificationRequestRoute} (Existing calls notifications/updates)";

            // Create structured config objects for service.
            this.CallControlBaseUrl = new Uri($"https://{this.ServiceCname}:{BotSignalingPort}/{HttpRouteConstants.CallSignalingRoutePrefix}");
            EventLog.WriteEntry(SampleConstants.EventLogSource, $"WindowsServiceConfiguration CallControlBaseUrl {this.CallControlBaseUrl}", EventLogEntryType.Warning);

            // http for local development or where certificate is not installed
            // https for running on VM
            var controlListenUris = new HashSet<string>();
            //Add DSN CName for external listening
            controlListenUris.Add($"https://*:{BotSignalingPort}/");
            controlListenUris.Add($"http://*:{BotSignalingPort + 1}/");
            EventLog.WriteEntry(SampleConstants.EventLogSource, $"WindowsServiceConfiguration controlListenUrl 1 {$"https://*:{BotSignalingPort}/"}", EventLogEntryType.Warning);
            this.CallControlListeningUrls = controlListenUris;

            this.MediaPlatformSettings = new MediaPlatformSettings()
            {
                MediaPlatformInstanceSettings = new MediaPlatformInstanceSettings()
                {
                    CertificateThumbprint = defaultCertificate.Thumbprint,
                    InstanceInternalPort = MediaStreamingPort,
                    InstancePublicIPAddress = IPAddress.Any,
                    InstancePublicPort = MediaStreamingPort,
                    ServiceFqdn = MediaDnsName
                },
                ApplicationId = this.AadAppId,
            };

            if (string.IsNullOrEmpty(this.H2641280X72030FpsFile) ||
                string.IsNullOrEmpty(this.H264320X18015FpsFile) ||
                string.IsNullOrEmpty(this.H264640X36030FpsFile) ||
                string.IsNullOrEmpty(this.H2641920X108015VBSSFpsFile))
            {
                throw new ArgumentNullException("H264Files", "Update app.config in WorkerRole with all the h264 files with the specified resolutions");
            }

            this.H264FileLocations = new Dictionary<string, VideoFormat>();
            this.H264FileLocations.Add(this.H2641280X72030FpsFile, VideoFormat.H264_1280x720_30Fps);
            this.H264FileLocations.Add(this.H264320X18015FpsFile, VideoFormat.H264_320x180_15Fps);
            this.H264FileLocations.Add(this.H264640X36030FpsFile, VideoFormat.H264_640x360_30Fps);
            this.H264FileLocations.Add(this.H2641920X108015VBSSFpsFile, VideoFormat.H264_1920x1080_15Fps);

            if (string.IsNullOrEmpty(this.AudioFileLocation))
            {
                throw new ArgumentNullException("AudioFileLocation", "Update app.config in WorkerRole with the audio file location");
            }


            EventLog.WriteEntry(SampleConstants.EventLogSource, $"Listening on: {botCallingUrl} (New Incoming calls)", EventLogEntryType.Information);
            EventLog.WriteEntry(SampleConstants.EventLogSource, $"Listening on: {botInstanceUrl} (Existing calls notifications/updates)", EventLogEntryType.Information);

            EventLog.WriteEntry(SampleConstants.EventLogSource, $"Listening on: net.tcp//{MediaDnsName}:{MediaStreamingPort} (Media connection)", EventLogEntryType.Information);
        }

        /// <summary>
        /// Helper to search the certificate store by its thumbprint.
        /// </summary>
        /// <returns>Certificate if found.</returns>
        /// <exception cref="Exception">No certificate with thumbprint {CertificateThumbprint} was found in the machine store.</exception>
        private X509Certificate2 GetCertificateFromStore()
        {

            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByThumbprint, CertificateThumbprint, validOnly: false);

                if (certs.Count != 1)
                {
                    throw new Exception($"No certificate with thumbprint {CertificateThumbprint} was found in the machine store.");
                }

                return certs[0];
            }
            finally
            {
                store.Close();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private void MapEnvironmentVars(EnvironmentVarConfigs envs)
        {
            if (envs == null)
            {
                throw new ArgumentNullException(nameof(envs));
            }
            this.AadAppId = envs.AadAppId;
            this.AadAppSecret = envs.AadAppSecret;
            this.BotName = envs.BotName;
            this.BotSignalingPort = envs.BotSignalingPort;
            this.ServiceCname = envs.ServiceCname;
            this.ServiceDnsName = envs.ServiceDnsName;
            this.CertificateThumbprint = envs.CertificateThumbprint;
            this.MediaStreamingPort = envs.MediaStreamingPort;
            this.H2641280X72030FpsFile = envs.H2641280X72030FpsFile;
            this.H2641920X108015VBSSFpsFile = envs.H2641920X1080VBSS15FpsFile;
            this.H264320X18015FpsFile = envs.H264320X18015FpsFile;
            this.H264640X36030FpsFile = envs.H264640X36030FpsFile;
            this.AudioFileLocation = envs.AudioFileLocationFile;
            this.AudioVideoFileLengthInSec = envs.AudioVideoFileLengthInSec;
        }
    }
}
