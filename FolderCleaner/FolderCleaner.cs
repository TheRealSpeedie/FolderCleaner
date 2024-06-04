using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
using FolderCleaner.Properties;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Aspose.Imaging.FileFormats.OpenDocument.Objects.Graphic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace FolderCleaner
{
    public partial class FolderCleaner : ServiceBase
    {
        private bool isRunning;
        private string result;

        private FileProcessor _fileprocessor;

        CancellationTokenSource cancellationTokenSource;
        CancellationToken cancellationToken;
        TimeSpan interval;
        TimeSpan intervalTimeController;
        AutoResetEvent resetInterval;
        public FolderCleaner()
        {
            InitializeComponent();
            eventLog = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("Folder Cleaner"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "Folder_Cleaner", "FolderCleanerLOG");
            }
            eventLog.Source = "Folder_Cleaner";
            eventLog.Log = "FolderCleanerLOG";
        }
        public void startService(string[] args)
        {
            OnStart(args);
        }
        protected override void OnStart(string[] args)
        {
            isRunning = true;
            interval = new TimeSpan();
            intervalTimeController = new TimeSpan(0, 0, 10);
            resetInterval = new AutoResetEvent(false);
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            _fileprocessor = new FileProcessor(eventLog);

            eventLog.WriteEntry("Successfully Started");
            Task task = Task.Run(() => ScanFolder(cancellationToken), cancellationToken);
        }
        protected override void OnStop()
        {
            isRunning = false;
            cancellationTokenSource.Cancel();
            eventLog.WriteEntry("In OnStop.");
        }
        protected override void OnContinue()
        {
            eventLog.WriteEntry("In OnContinue.");
        }

        public void ScanFolder(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Task was canceled before it started.");
                return;
            }
            while (isRunning)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Task was canceled during execution.");
                    return;
                }
                if (interval < TimeSpan.Zero)
                {
                    interval = TimeSpan.FromSeconds(1);
                }
                if (resetInterval.WaitOne(interval))
                    continue;
                Stopwatch stopwatch = Stopwatch.StartNew();
                _fileprocessor.ProcessFiles();
                stopwatch.Stop();
                interval = intervalTimeController.Subtract(TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds));
            }
        }
    }
       
}

