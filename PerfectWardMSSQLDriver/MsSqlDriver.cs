using PerfectWardAPI;
using PerfectWardAPI.Api;
using PerfectWardAPI.Data;
using PerfectWardAPI.Model.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace PerfectWardMsSqlDriver
{
    public class MsSqlDriver : IDbDriver
    {
        private readonly string _connectionString;
        private const int RETRY_LIMIT = 10;
        private const int RETRY_TIME = 10;   //Time in seconds.

        public MsSqlDriver(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool TestConnection()
        {
            Debug.Log("Testing connection...");
            string msg;
            try
            {
                using (var conn = OpenConnection())
                {
                    conn.Close();
                }
                Debug.Log("Connection succeeded.");
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
            }

            Debug.Log("Connection failed.");
            Debug.Log(msg);
            return false;
        }

        private bool TableExists()
        {
            bool exists = false;
            using (var conn = OpenConnection())
            {
                using (var cmd = new SqlCommand("select OBJECT_ID('dbo.reports', 'U')", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    exists = cmd.ExecuteScalar() != DBNull.Value;
                }
                conn.Close();
            }
            Debug.Log($"Checking if table 'reports' exists: {exists}");
            return exists;
        }

        //Drops the reports and answers tables if they exist. Useful when writing queries.
        public void DropTables()
        {
            Debug.Log("Dropping tables...");
            using (var conn = OpenConnection())
            {
                using (var tx = conn.BeginTransaction())
                {
                    Debug.Log("Dropping reports table...");
                    using (var cmd = new SqlCommand("IF OBJECT_ID('dbo.reports', 'U') IS NOT NULL DROP TABLE dbo.reports;", conn, tx))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                    Debug.Log("Dropping answers table...");
                    using (var cmd = new SqlCommand("IF OBJECT_ID('dbo.answers', 'U') IS NOT NULL DROP TABLE dbo.answers;", conn, tx))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
                conn.Close();
            }
            Debug.Log("Dropped tables");
        }

        public void CreateTables()
        {
            if (!TableExists())
            {
                Debug.Log($"Creating table 'reports'...");
                using (var conn = OpenConnection())
                {
                    using (var tx = conn.BeginTransaction())
                    {
                        var createTableReports = "create table reports (" +
                            string.Join(", ", new[]
                            {
                                "id bigint not null primary key",
                                "score float not null",
                                "comment text",
                                "started_at datetime not null",
                                "ended_at datetime not null",

                                "inspection_type_id bigint",
                                "inspection_type_name text",

                                "area_id bigint",
                                "area_name text",
                                "area_division_id bigint",
                                "area_division_name text",

                                "inspector_id bigint",
                                "inspector_name text",
                                "inspector_email text",
                                "inspector_first_name text",
                                "inspector_last_name text",
                                "inspector_job_title text",
                                "inspector_role_id bigint",
                                "inspector_role_name text",

                                "survey_id bigint",
                                "survey_name text",

                                "final_reflections text",
                            }) + ");";

                        using (var cmd = new SqlCommand(createTableReports, conn, tx))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.ExecuteNonQuery();
                        }

                        var createTableAnswers = "create table answers (" +
                            string.Join(", ", new[]
                            {
                                "id bigint not null primary key",
                                "report_id bigint not null",
                                "score float not null",
                                "it_scores bit not null",
                                "is_multiple bit not null",
                                "answer_text text null",
                                "answer_choice_id int null",
                                "question_id bigint not null",
                                "question_text text null",
                                "category_name text null",
                                "note text null",
                                "parent_id bigint null",
                            }) + ");";

                        using (var cmd = new SqlCommand(createTableAnswers, conn, tx))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                    conn.Close();
                }
                Debug.Log($"Created table 'reports'");
            }
        }

        public int[] ListReportIDs()
        {
            Debug.Log("Getting all report IDs...");
            using (var conn = OpenConnection())
            {
                var ids = new List<int>();
                using (var cmd = new SqlCommand("select id from dbo.reports", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        ids.Add((int)(long)reader[0]);
                    }
                }
                conn.Close();
                return ids.ToArray();
            }
        }

        public void UploadReport(ref SqlConnection conn, DetailedReportResponse reportsResponse)
        {
            var retryCount = 0;
            SqlTransaction tx;

            while (true)
            {
                try
                {
                    if (conn == null)
                    {
                        Debug.Log($"Opening a fresh database connection...");
                        conn = OpenConnection();
                        Debug.Log($"Successfully opened a fresh database connection.");
                    }
                    tx = conn.BeginTransaction();
                    break;
                }
                catch (Exception ex)
                {
                    conn?.Close();
                    conn?.Dispose();
                    conn = null;
                    Debug.Log($"Error beginning a new transaction. Error:\n{ex}");
                    if (++retryCount < RETRY_LIMIT)
                    {
                        Debug.Log($"Sleeping for {RETRY_TIME} seconds...");
                        Thread.Sleep(1000 * RETRY_TIME);    //Sleep for a minute.
                    }
                    else
                    {
                        Debug.Log($"Beginning a new transaction failed {RETRY_LIMIT} times consecutively. Terminating...");
                        Environment.Exit(0);
                    }
                }
            }

            var r = reportsResponse.Report;
            Debug.Log($"Uploading report {r.Id}...");
            using (var cmd = new SqlCommand("INSERT INTO reports VALUES(@id,@score,@comment,@started_at,@ended_at,@inspection_type_id,@inspection_type_name,@area_id,@area_name,@area_division_id,@area_division_name,@inspector_id,@inspector_name,@inspector_email,@inspector_first_name,@inspector_last_name,@inspector_job_title,@inspector_role_id,@inspector_role_name,@survey_id,@survey_name,@final_reflections)", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", r.Id);
                cmd.Parameters.AddWithValue("@score", r.Score);
                cmd.Parameters.AddWithValue("@comment", (object)(r.Comment) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@started_at", r.StartedAtDate);
                cmd.Parameters.AddWithValue("@ended_at", r.EndedAtDate);

                cmd.Parameters.AddWithValue("@inspection_type_id", (object)(r.InspectionType?.Id) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@inspection_type_name", (object)(r.InspectionType?.Name) ?? DBNull.Value);

                var area = r.Area;
                cmd.Parameters.AddWithValue("@area_id", (object)(area?.Id) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@area_name", (object)(area?.Name) ?? DBNull.Value);

                var division = area?.Division;
                cmd.Parameters.AddWithValue("@area_division_id", (object)(division?.Id) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@area_division_name", (object)(division?.Name) ?? DBNull.Value);

                var inspector = r.Inspector;
                cmd.Parameters.AddWithValue("@inspector_id", (object)(inspector?.Id) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@inspector_name", (object)(inspector?.Name) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@inspector_email", (object)(inspector?.Email) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@inspector_first_name", (object)(inspector?.FirstName) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@inspector_last_name", (object)(inspector?.LastName) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@inspector_job_title", (object)(inspector?.JobTitle) ?? DBNull.Value);

                var role = inspector?.Role;
                cmd.Parameters.AddWithValue("@inspector_role_id", (object)(role?.Id) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@inspector_role_name", (object)(role?.Name) ?? DBNull.Value);

                var survey = r.Survey;
                cmd.Parameters.AddWithValue("@survey_id", (object)(survey?.Id) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@survey_name", (object)(survey?.Name) ?? DBNull.Value);

                var fr = JSON.GetString(r.FinalReflections);
                cmd.Parameters.AddWithValue("@final_reflections", (object)(fr) ?? DBNull.Value);

                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }

            foreach (var answer in r.Answers) AddAnswer(answer, r.Id, conn, tx);

            Debug.Log($"Uploaded.");
            tx.Commit();

            tx.Dispose();



        }

        public void UploadReports(ref DetailedReportResponse[] reportsResponse)
        {
            if (!reportsResponse.GetEnumerator().MoveNext())
            {
                Debug.Log($"No reports available for upload.");
                return;
            }
            var count = 0;
            var conn = OpenConnection();
            using (var tx = conn.BeginTransaction())
            {
                foreach (var drr in reportsResponse)
                {
                    count++;
                    var r = drr.Report;
                    Debug.Log($"Upload progress {count}/{reportsResponse.Length}");
                    UploadReport(ref conn, drr);
                }
                tx.Commit();
            }
            conn.Close();
            conn.Dispose();
            Debug.Log($"Finished uploading reports.");
        }

        private void AddAnswer(Answer a, int reportId, SqlConnection conn, SqlTransaction tx, int? parent = null)
        {
            using (var cmd = new SqlCommand("INSERT INTO answers VALUES(@id,@report_id,@score,@it_scores,@is_multiple,@answer_text,@answer_choice_id,@question_id,@question_text,@category_name,@note,@parent_id)", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", a.Id);
                cmd.Parameters.AddWithValue("@report_id", reportId);
                cmd.Parameters.AddWithValue("@score", a.Score);
                cmd.Parameters.AddWithValue("@it_scores", a.ItScores);
                cmd.Parameters.AddWithValue("@is_multiple", a.IsMultiple);
                cmd.Parameters.AddWithValue("@answer_text", (object)(a.AnswerText) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@answer_choice_id", (object)(a.AnswerChoiceId) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@question_id", a.QuestionId);
                cmd.Parameters.AddWithValue("@question_text", (object)(a.QuestionText) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@category_name", (object)(a.CategoryName) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@note", (object)(a.Note) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@parent_id", parent.HasValue ? (object)parent.Value : DBNull.Value);

                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();

                if (a.SubAnswers != null)
                {
                    foreach (var sa in a.SubAnswers)
                    {
                        AddAnswer(sa, reportId, conn, tx, a.Id);
                    }
                }
            }
        }

        public DateTime GetLatestStartDate()
        {
            Debug.Log("Getting latest start date...");
            using (var conn = OpenConnection())
            {
                DateTime ret;
                using (var cmd = new SqlCommand("select max(started_at) from dbo.reports", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    var endedAt = cmd.ExecuteScalar();

                    if (endedAt == DBNull.Value)
                    {
                        ret = DateTime.MinValue;
                        Debug.Log("No entries found, returning DateTime.MinValue");
                    }
                    else
                    {
                        ret = (DateTime)endedAt;
                        Debug.Log($"Latest end date: {ret}");
                    }
                }
                conn.Close();
                return ret;
            }
        }

        public SqlConnection OpenConnection()
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
