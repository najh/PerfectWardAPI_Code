using PerfectWardApi.Api;
using PerfectWardAPI;
using PerfectWardAPI.Api;
using PerfectWardAPI.Data;
using PerfectWardAPI.Model.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ApiTest
{
    class Program
    {
        private const int CLOSE_TIME = 30;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);

            Debug.Log("Background task started.");
            Debug.Log($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");

            var mtx = new Mutex(true, "PW_TASK");
            if (!mtx.WaitOne(1000))
            {
                Debug.Log("Existing task in progress, quitting...");
                return;
            }

            var connStr = EnvironmentVariables.Get(EnvironmentVariables.SQLConnectionString);
            //Debug.Log($"{nameof(connStr)}: {connStr}");
            if (string.IsNullOrEmpty(connStr))
            {
                Debug.Log($"{nameof(connStr)} is null, quitting...");
                return;
            }

            var pwEmail = EnvironmentVariables.Get(EnvironmentVariables.PerfectWardEmail);
            //Debug.Log($"{nameof(pwEmail)}: {pwEmail}");
            if (string.IsNullOrEmpty(pwEmail))
            {
                Debug.Log($"{nameof(pwEmail)} is null, quitting...");
                return;
            }

            var pwToken = EnvironmentVariables.Get(EnvironmentVariables.PerfectWardToken);
            //Debug.Log($"{nameof(pwToken)}: {pwToken}");
            if (string.IsNullOrEmpty(pwToken))
            {
                Debug.Log($"{nameof(pwToken)} is null, quitting...");
                return;
            }

            var driver = DbDriver.Create(connStr);

            if (driver == null)
            {
                Debug.Log("No DB driver found, quitting.");
                return;
            }

            Debug.Log($"DB driver found: {driver.GetType()}");

            if (driver.TestConnection())
            {
                var creds = new StringCredentials(pwEmail, pwToken);
                var pwc = new PerfectWardClient(creds);
                Task.Run(async () =>
                {
                    if (await pwc.TestApi())
                    {
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
                                reportsResponse = await pwc.ListReportsSince(maxTimestamp, ++currentPage);
                                reports.AddRange(reportsResponse.Reports);
                            }
                        }
                        reports = reports.Distinct(new ReportEqualityComparer()).Where(x => !existingIDs.Contains(x.Id)).OrderBy(x => x.Id).ToList();
                        Debug.Log($"Query returned reports: {reports.Count}");

                        var conn = driver.OpenConnection();

                        var reportsStack = new Queue<Report>(reports);
                        while(reportsStack.Any())
                        {
                            var report = reportsStack.Peek();
                            var drr = pwc.ReportDetails(report.Id).Result;
                            driver.UploadReport(ref conn, drr);
                            reportsStack.Dequeue();
                        }
                        //foreach(var report in reports)
                        //{
                        //    var drr = pwc.ReportDetails(report.Id).Result;
                        //    driver.UploadReport(conn, drr);
                        //};

                        Debug.Log($"Finished uploading reports, terminating...");

                        conn.Close();
                        conn.Dispose();
                    }
                }).Wait();
            }
        }

        static void UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var ex = (Exception)args.ExceptionObject;
            try
            {
                Debug.Log($"Unhandled exception occurred:\n{ex}");
            }
            catch (Exception) { }
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(CLOSE_TIME * 1000);
                Debug.Log($"Terminating...");
                Environment.Exit(0);
            });
            MessageBox.Show($"An error occurred, terminating in {CLOSE_TIME} seconds:\n{ex}");
        }
    }
}