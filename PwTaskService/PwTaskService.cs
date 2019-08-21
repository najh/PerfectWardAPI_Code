using PerfectWardApi.Api;
using PerfectWardAPI;
using PerfectWardAPI.Api;
using PerfectWardAPI.Data;
using PerfectWardAPI.Model.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace PwTaskService
{
    public partial class PwTaskService : ServiceBase
    {
        private CancellationTokenSource ServiceTokenSource;
        private ManualResetEvent TaskComplete;
        private bool OneShotCancelled = true;

        public PwTaskService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Debug.Log("Starting PwTask Service");

            TaskComplete?.Dispose();
            TaskComplete = new ManualResetEvent(false);
            ServiceTokenSource?.Dispose();
            ServiceTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(DoWork, ServiceTokenSource.Token);
        }

        protected override void OnStop()
        {
            Debug.Log("Stopping PwTask Service...");
            ServiceTokenSource.Cancel();
            TaskComplete.WaitOne();
            Debug.Log("Successfully terminated.");
        }

        private async void DoWork()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);

            Debug.Log("Background task started.");
            Debug.Log($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");

            var connStr = EnvironmentVariables.Get(EnvironmentVariables.SQLConnectionString);
            //Debug.Log($"{nameof(connStr)}: {connStr}");
            if (string.IsNullOrEmpty(connStr))
            {
                Debug.Log($"{nameof(connStr)} is null, quitting...");
                Environment.Exit(-1);
                return;
            }

            var pwEmail = EnvironmentVariables.Get(EnvironmentVariables.PerfectWardEmail);
            //Debug.Log($"{nameof(pwEmail)}: {pwEmail}");
            if (string.IsNullOrEmpty(pwEmail))
            {
                Debug.Log($"{nameof(pwEmail)} is null, quitting...");
                Environment.Exit(-1);
                return;
            }

            var pwToken = EnvironmentVariables.Get(EnvironmentVariables.PerfectWardToken);
            //Debug.Log($"{nameof(pwToken)}: {pwToken}");
            if (string.IsNullOrEmpty(pwToken))
            {
                Debug.Log($"{nameof(pwToken)} is null, quitting...");
                Environment.Exit(-1);
                return;
            }

            Environment.CurrentDirectory = new FileInfo(Assembly.GetExecutingAssembly().CodeBase.Substring(8)).DirectoryName;

            var driver = DbDriver.Create(connStr);

            if (driver == null)
            {
                Debug.Log("No DB driver found, quitting.");
                Environment.Exit(-1);
                return;
            }

            Debug.Log($"DB driver found: {driver.GetType()}");

            if (!driver.TestConnection())
            {
                Debug.Log("Failed to connect to the database.");
                Environment.Exit(-1);
                return;
            }

            var creds = new StringCredentials(pwEmail, pwToken);
            var pwc = new PerfectWardClient(creds);
            if (!await pwc.TestApi())
            {
                Debug.Log("Failed to connect to the API.");
                Environment.Exit(-1);
                return;
            }

            driver.CreateTables();
            var existingIDs = driver.ListReportIDs();
            var maxTimestamp = driver.GetLatestStartDate().ToTimeStamp();
            var reportsResponse = await pwc.ListReportsSince(maxTimestamp);
            var reports = reportsResponse.Reports;
            if (reportsResponse.Meta != null)
            {
                int currentPage = reportsResponse.Meta.CurrentPage;
                int totalPages = reportsResponse.Meta.TotalPages;
                while (currentPage < totalPages)
                {
                    if (AnnounceCancel()) break;
                    reportsResponse = await pwc.ListReportsSince(maxTimestamp, ++currentPage);
                    reports.AddRange(reportsResponse.Reports);
                }
            }
            reports = reports.Distinct(new ReportEqualityComparer()).Where(x => !existingIDs.Contains(x.Id)).OrderBy(x => x.Id).ToList();
            Debug.Log($"Query returned reports: {reports.Count}");

            var conn = driver.OpenConnection();

            var reportsStack = new Queue<Report>(reports);
            while (reportsStack.Any())
            {
                if (AnnounceCancel()) break;
                var report = reportsStack.Peek();
                var drr = pwc.ReportDetails(report.Id).Result;
                driver.UploadReport(ref conn, drr);
                reportsStack.Dequeue();
            }

            Debug.Log($"Finished uploading reports, terminating...");

            conn.Close();
            conn.Dispose();

            TaskComplete.Set();
            if (!AnnounceCancel()) Stop();
        }

        private bool AnnounceCancel()
        {
            if (ServiceTokenSource.IsCancellationRequested)
            {
                if (OneShotCancelled)
                {
                    OneShotCancelled = false;
                    Debug.Log($"Service stop requested, terminating...");
                }
                return true;
            }
            return false;
        }

        void UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var ex = (Exception)args.ExceptionObject;
            try
            {
                Debug.Log($"Unhandled exception occurred:\n{ex}");
            }
            catch (Exception) { }
            Debug.Log($"Terminating...");
            Environment.Exit(0);
        }
    }
}
