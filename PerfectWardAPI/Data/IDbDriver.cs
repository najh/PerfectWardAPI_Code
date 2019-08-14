using PerfectWardAPI.Model.Reports;
using System;
using System.Data.SqlClient;

namespace PerfectWardAPI.Data
{
    public interface IDbDriver
    {
        bool TestConnection();
        DateTime GetLatestStartDate();
        SqlConnection OpenConnection();
        void DropTables();
        void CreateTables();
        void UploadReport(ref SqlConnection conn, DetailedReportResponse reportsResponse);
        void UploadReports(ref DetailedReportResponse[] reportsResponse);
        int[] ListReportIDs();
    }
}
