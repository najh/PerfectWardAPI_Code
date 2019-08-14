﻿using PerfectWardApi.Api;
using PerfectWardAPI.Api;
using PerfectWardAPI.Data;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Installer
{
    public partial class MainWindow : Window
    {
        private bool SuccessAPI = false;
        private bool SuccessSQL = false;

        private SolidColorBrush bruPwGreen = new SolidColorBrush(Color.FromArgb(255, 0, 175, 80));
        private SolidColorBrush bruError = new SolidColorBrush(Color.FromArgb(255, 174, 0, 42));

        private const string TaskName = "PW API";

        public MainWindow()
        {
            PerfectWardAPI.Debug.Log("Connector started.");
            PerfectWardAPI.Debug.Log($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");

            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                if (args[1] == "-uninstall")
                {
                    PerfectWardAPI.Debug.Log("Uninstall flag received.");
                    PerfectWardAPI.Debug.Close();
                    try
                    {
                        SetEnvironmentVariables(null, null, null, null, null);
                        foreach (var pi in Process.GetProcessesByName("PwTask")) pi.Kill();
                        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PerfectWardAPI";
                        foreach (var f in new DirectoryInfo(appDataFolder).GetFiles("*_debug.log")) f.Delete();
                        var ts = Microsoft.Win32.TaskScheduler.TaskService.Instance;
                        ts.RootFolder.DeleteTask(TaskName, false);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    Environment.Exit(0);
                }
            }

            InitializeComponent();

            TextChangedEventHandler pwTestCondition = (s, e) =>
            {
                btnInstall.IsEnabled = false;
                btnTestPw.IsEnabled = txtEmail.Text.Length > 0 && txtAPIToken.Text.Length > 0;
            };
            txtEmail.TextChanged += pwTestCondition;
            txtAPIToken.TextChanged += pwTestCondition;

            txtConnStr.TextChanged += (s, e) =>
            {
                btnInstall.IsEnabled = false;
                btnTestSql.IsEnabled = txtConnStr.Text.Length > 0;
            };

            RoutedEventHandler cc = (s, e) =>
            {
                grpProxy.IsEnabled = chkPxyEnabled.IsChecked.Value;
            };
            chkPxyEnabled.Checked += cc;
            chkPxyEnabled.Unchecked += cc;

            btnTestPw.Click += (s, e) =>
            {
                var credentials = new StringCredentials(txtEmail.Text, txtAPIToken.Text);

                PerfectWardClient pwc;

                if(chkPxyEnabled.IsChecked.Value)
                {
                    pwc = new PerfectWardClient(credentials, new System.Net.NetworkCredential(txtPxyUser.Text, txtPxyPass.Text));
                }
                else
                {
                    pwc = new PerfectWardClient(credentials);
                }
                
                grpPw.IsEnabled = false;
                txtPwStatus.Content = "Testing...";
                txtPwStatus.Foreground = Brushes.Black;

                Task.Factory.StartNew(async () =>
                {
                    PerfectWardAPI.Debug.Log("Test API clicked.");
                    try
                    {
                        SuccessAPI = await pwc.TestApi();
                        PerfectWardAPI.Debug.Log($"API test: {SuccessAPI}");
                        Dispatcher.Invoke(() =>
                        {
                            if (SuccessAPI)
                            {
                                txtPwStatus.Content = "Success!";
                                txtPwStatus.Foreground = bruPwGreen;

                            }
                            else
                            {
                                grpPw.IsEnabled = true;
                                txtPwStatus.Content = "Authentication failed";
                                txtPwStatus.Foreground = bruError;
                            }

                            btnInstall.IsEnabled = SuccessAPI && SuccessSQL;
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        PerfectWardAPI.Debug.Log("Error in test API:\n" + ex);
                        Environment.Exit(0);
                    }
                });

            };

            btnTestSql.Click += (s, e) =>
            {
                PerfectWardAPI.Debug.Log("Test SQL clicked.");
                try
                {
                    var driver = DbDriver.Create(txtConnStr.Text);
                    grpSql.IsEnabled = false;
                    txtSqlStatus.Content = "Testing...";
                    txtSqlStatus.Foreground = Brushes.Black;

                    Task.Factory.StartNew(() =>
                    {
                        SuccessSQL = driver.TestConnection();
                        PerfectWardAPI.Debug.Log($"SQL test: {SuccessSQL}");
                        Dispatcher.Invoke(() =>
                        {
                            if (SuccessSQL)
                            {
                                txtSqlStatus.Content = "Success!";
                                txtSqlStatus.Foreground = bruPwGreen;

                                //driver.DropTables();
                                driver.CreateTables();
                            }
                            else
                            {

                                grpSql.IsEnabled = true;
                                txtSqlStatus.Content = "Connection failed";
                                txtSqlStatus.Foreground = bruError;
                            }

                            btnInstall.IsEnabled = SuccessAPI && SuccessSQL;
                        });
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    PerfectWardAPI.Debug.Log("Error in test SQL:\n" + ex);
                    Environment.Exit(0);
                }
            };

            btnInstall.Click += (s, e) =>
            {
                PerfectWardAPI.Debug.Log("Install clicked.");
                try
                {
                    PerfectWardAPI.Debug.Log("Setting environment variables...");
                    SetEnvironmentVariables(txtEmail.Text, txtAPIToken.Text, txtPxyUser.Text, txtPxyPass.Text, txtConnStr.Text);
                    var subfolder = new DirectoryInfo(Environment.CurrentDirectory);
                    var exePath = $"{subfolder.FullName}\\PwTask.exe";

                    PerfectWardAPI.Debug.Log("Scheduling task...");
                    var ts = Microsoft.Win32.TaskScheduler.TaskService.Instance;
                    ts.RootFolder.DeleteTask(TaskName, false);
                    var t = ts.AddTask(
                        TaskName,
                        new Microsoft.Win32.TaskScheduler.DailyTrigger(),
                        new Microsoft.Win32.TaskScheduler.ExecAction(exePath, null, subfolder.FullName)
                        );

                    MessageBox.Show("The API was connected.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    PerfectWardAPI.Debug.Log($"Starting task...");

                    Process.Start(new ProcessStartInfo("PwTask.exe"));
                    //t.Run();

                    MessageBox.Show("The reporting task has begun and will run in the background.", string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);

                    PerfectWardAPI.Debug.Log($"Quitting...");
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    PerfectWardAPI.Debug.Log("Error in install:\n" + ex);
                    Environment.Exit(0);
                }
            };

            Closed += (s, e) =>
            {
                PerfectWardAPI.Debug.Log("Closing connector.");
            };
        }

        private void SetEnvironmentVariables(string email, string token, string pxyUser, string pxyPass, string connStr)
        {
            EnvironmentVariables.Set(EnvironmentVariables.PerfectWardEmail, email);
            EnvironmentVariables.Set(EnvironmentVariables.PerfectWardToken, token);
            if (string.IsNullOrEmpty(pxyUser)) pxyUser = null;
            if (string.IsNullOrEmpty(pxyPass)) pxyPass = null;
            EnvironmentVariables.Set(EnvironmentVariables.ProxyUsername, pxyUser);
            EnvironmentVariables.Set(EnvironmentVariables.ProxyPassword, pxyPass);
            EnvironmentVariables.Set(EnvironmentVariables.SQLConnectionString, connStr);
        }
    }
}
