using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIMA.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Infrastructure.Repositories
{
    public class DataBaseServices
    {
        private static IConfiguration _config;

        public DataBaseServices(IConfiguration config) => _config = config;

        /// <summary>
        /// Test Latency Database 
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static int PingSqlServer(IConfiguration config)
        {
            _config = config;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    using var command = new SqlCommand("SELECT 1", connection);
                    stopwatch = Stopwatch.StartNew();
                    connection.Open();
                    command.ExecuteScalar();
                }

            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return -1;
            }

            stopwatch.Stop();
            return (int)stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Test Latency Database 
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static async Task<int> PingSqlServerAsync(IConfiguration config)
        {
            _config = config;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await Task.Delay(500);
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    using var command = new SqlCommand("SELECT 1", connection);
                    stopwatch = Stopwatch.StartNew();
                    connection.Open();
                    command.ExecuteScalar();
                }

            }
            catch (Exception ex)
            {
                stopwatch.Stop(); 
                return -1;
            }
   
            stopwatch.Stop();
            return (int)stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Unit Test SqlException
        /// </summary>
        /// <returns></returns>
        public static SqlException ThrowSqlException()
        {
            try
            {
                using (var conn = new SqlConnection(@"Data Source=FAIL;Initial Catalog=FAIL;Connection Timeout=1"))
                {
                    conn.Open();
                }
            }
            catch (SqlException ex)
            {
                return ex;
            }
            return null;

        }

    }

}

