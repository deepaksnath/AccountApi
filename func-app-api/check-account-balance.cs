using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using func_app_api.Models;
using System.Data.SqlClient;

namespace func_app_api
{
    public static class Function1
    {
        [FunctionName("check-account-balance")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var transaction = JsonConvert.DeserializeObject<Transaction>(requestBody);
            Account account = null;
            
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    if (!String.IsNullOrEmpty(transaction.Account.ToString()))
                    {
                        var query = $"SELECT Id, AccountId, Name, Balance, " +
                            $"CASE WHEN Balance > {transaction.Amount} THEN CAST(1 as bit) ELSE CAST(0 as bit) END As IsGreater " +
                            $"FROM Wallet WHERE AccountId = {transaction.Account} ";
                        SqlCommand command = new SqlCommand(query, connection);
                        var reader = await command.ExecuteReaderAsync();
                        while (reader.Read())
                        {
                            account = new Account()
                            {
                                Id = (int)reader["Id"],
                                AccountId = (int)reader["AccountId"],
                                Name = reader["Name"].ToString(),
                                Balance = (double)reader["Balance"],
                                IsGreater = (bool)reader["isGreater"]
                            };                      
                        }
                    }
                }

                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    var accname = account != null ? account.Name : string.Empty;
                    var accbalance = account != null ? account.Balance : 0;
                    var tranSuccess = account != null ? "TRUE" : "FALSE";
                    var query = $"INSERT INTO [Transaction] VALUES" +
                        $"('Account balance check.'," +
                        $"'{DateTime.UtcNow}'," +
                        $"'{transaction.Account}'," +
                        $"'{accname}'," +
                        $"'{accbalance}'," +
                        $"'{transaction.Direction}'," +
                        $"'{tranSuccess}'" +
                        $")";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.ExecuteNonQuery();
                }
                if (account != null)
                {
                    if (account.IsGreater)
                    {
                        return new OkObjectResult($"Congratulations {account.Name}, You have sufficient balance in your account(>{transaction.Amount}). Your Balance is {account.Balance}");
                    }
                    else
                    {
                        return new OkObjectResult($"Sorry {account.Name}. You don't have sufficient balance in your account(<{transaction.Amount}). Your Balance is {account.Balance}");
                    }
                }
                else
                {
                    return new OkObjectResult($"There is no account with id {transaction.Account}");
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                return new BadRequestResult();
            }
        }
    }
}
