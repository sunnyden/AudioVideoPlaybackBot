using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkypeTeamsMeetingBot.AudioVideoPlaybackBot.FrontEnd;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace SkypeTeamsMeetingBot
{
    public partial class SkypeTeamsMeetingBotService : ServiceBase
    {
        /// <summary>
        /// The cancellation token source.
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// The run complete event.
        /// </summary>
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public SkypeTeamsMeetingBotService()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists(SampleConstants.EventLogSource))
            {
                EventLog.CreateEventSource(SampleConstants.EventLogSource, SampleConstants.EventLogType);
            }
            EventLog.WriteEntry(SampleConstants.EventLogSource, "Initializing SkypeTeamsMeetingBot Service", EventLogEntryType.Warning);
        }

        public void OnDebug()
        {
            this.OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry(SampleConstants.EventLogSource, "Starting SkypeTeamsMeetingBotService", EventLogEntryType.Warning);
            try
            {
                ServicePointManager.DefaultConnectionLimit = 12;
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.AddEnvironmentVariables();
                JsonSerializer serializer = new JsonSerializer();
                var configPath = Environment.GetEnvironmentVariable("MeetingBotConfig");
                if (configPath == null)
                {
                    configPath = "config.json";
                }
                var configFile = JObject.Parse(File.ReadAllText(configPath));
                var configs = serializer.Deserialize<EnvironmentVarConfigs>(configFile.CreateReader());

                // ECS backend service enforced TLS 1.2 access.
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Create and start the environment-independent service.
                Service.Instance.Initialize(new WindowsServiceConfiguration(configs));
                Service.Instance.Start();
                EventLog.WriteEntry(SampleConstants.EventLogSource, "SkypeTeamsMeetingBotService Service Started", EventLogEntryType.Warning);
                base.OnStart(args);

            }
            catch (Exception e)
            {
                EventLog.WriteEntry(SampleConstants.EventLogSource, $"SkypeTeamsMeetingBotService Exception caught {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry(SampleConstants.EventLogSource, "SkypeTeamsMeetingBotService Service Stopping", EventLogEntryType.Warning);
            Service.Instance.Stop();
            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();
            EventLog.WriteEntry(SampleConstants.EventLogSource, "SkypeTeamsMeetingBotService Service Stopped", EventLogEntryType.Warning);
            base.OnStop();
        }
    }
}
