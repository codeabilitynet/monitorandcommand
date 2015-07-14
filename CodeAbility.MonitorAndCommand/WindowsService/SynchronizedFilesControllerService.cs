using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration; 
using System.Data;
using System.Diagnostics;
using System.IO; 
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using Lusis.Q2M.SynchronizedFilesController; 

namespace Lusis.Q2M.WindowsService
{
    public partial class SynchronizedFilesControllerService : ServiceBase
    {
        Facade facade; 

        public SynchronizedFilesControllerService()
        {
            InitializeComponent();

            ServiceName = "SynchronizedFilesControllerService";
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            try
            { 
                string rootFolderPath = args[0];

                NotifyFilters notifyFilters = NotifyFilters.CreationTime
                                            | NotifyFilters.LastAccess
                                            | NotifyFilters.LastWrite
                                            | NotifyFilters.Size
                                            | NotifyFilters.FileName
                                            | NotifyFilters.DirectoryName;

                string fileFilters = "*.zip";
                bool includeSubdirectories = true;
                     
                int checkingPoolTime = Int32.Parse(args[1]);

                facade = new Facade(rootFolderPath, notifyFilters, fileFilters, includeSubdirectories, checkingPoolTime);

                facade.Start();
            }
            catch (Exception)
            {
                Trace.WriteLine("SynchronizedFilesControllerService could not start, check command line parameters."); 
            }
        }

        protected override void OnStop()
        {
            facade.Stop();
        }
    }
}
