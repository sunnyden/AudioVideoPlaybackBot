﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SkypeTeamsMeetingBot
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            //While debugging this section is used.
            var myService = new SkypeTeamsMeetingBotService();
            myService.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);

#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SkypeTeamsMeetingBotService()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
