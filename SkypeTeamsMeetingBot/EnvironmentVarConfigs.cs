using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkypeTeamsMeetingBot
{
    public class EnvironmentVarConfigs
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
        /// Gets or sets the instance internal port.
        /// </summary>
        /// <value>The instance internal port.</value>
        public int MediaStreamingPort { get; set; }
        public int BotSignalingPort { get; set; }

        public string H2641280X72030FpsFile { get; set; }
        public string H264640X36030FpsFile { get; set; }
        public string H264320X18015FpsFile { get; set; }
        public string H2641920X1080VBSS15FpsFile { get; set; }
        public string AudioFileLocationFile { get; set; }
        public int AudioVideoFileLengthInSec { get; set; }

    }
}
