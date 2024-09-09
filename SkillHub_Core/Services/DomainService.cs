using LMSCore.Areas.Models;
using LMSCore.Models;
using MimeKit.Encodings;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace LMSCore.Services
{
    public class DomainService
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();
        protected lmsDbContext dbContext;
        public DomainService(lmsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<List<T>> DapperQuery<T>(string sqlQuery)
        {
            using (var connection = new SqlConnection(configuration.GetSection("ConnectionStrings:DbContext").Value.ToString()))
            {
                await connection.OpenAsync();
                var data = await connection.QueryAsync<T>(sqlQuery);
                await connection.CloseAsync();
                return data.ToList();
            }
        }
    }
}