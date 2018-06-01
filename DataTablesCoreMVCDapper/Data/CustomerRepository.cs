using Dapper;
using DataTablesCoreMVCDapper.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DataTablesCoreMVCDapper.Data
{
    public class CustomerRepository
    {
        protected static ILogger Logger => Startup.LoggerFactory.CreateLogger(nameof(CustomerRepository));

        /// <summary>
        /// Dapper ORM SqlConnection Factory
        /// </summary>
        /// <returns>A new instance of DbConnection with the database for use by Dapper ORM.</returns>
        public static DbConnection GetOpenConnection()
        {
            var connection = new SqlConnection(ConfigurationExtensions.GetConnectionString(Startup.Configuration, "DefaultConnection"));
            connection.Open();

            return connection;
        }

        /// <summary>
        /// Returns the SQL Query selecting all data to hydrate the Customer class data model.
        /// </summary>
        /// <remarks>
        /// This may be a complex SQL query with JOINs as needed.  But the query must not
        /// contain ORDER BY, OFFSET, or TOP declaritives.
        /// </remarks>
        public static string BaseQuery()
        {
            return $@"SELECT * FROM {Customer.Table}";
        }

        #region CRUD Operations

        /// <summary>
        /// Creates (adds) a record to Customers.
        /// </summary>
        /// <param name="customer">Customer value(s) to create.</param>
        /// <returns>Customer.Id of newly created record.</returns>
        public static async Task<int> CreateAsync(Customer customer)
        {
            using (var db = GetOpenConnection())
            {
                string query = $@"INSERT INTO {Customer.Table}
                    ([FirstName], [LastName], [CompanyName], [Address], [City], [County], [State], [Zip], [Phone1], [Phone2], [Email], [Web])
                    VALUES 
                    (@FirstName, @LastName, @CompanyName, @Address, @City, @County, @State, @Zip, @Phone1, @Phone2, @Email, @Web);
                    SELECT SCOPE_IDENTITY() AS [Id];";
                try
                {
                    return await db.QuerySingleAsync<int>(query, customer);
                }
                catch
                {
                    Logger.LogError($@"SqlQuery: {query} using: {Newtonsoft.Json.JsonConvert.SerializeObject(customer)}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes a record from Customers.
        /// </summary>
        /// <param name="Id">Customer.Id of target Customer to remove.</param>
        /// <returns>Number of records deleted.</returns>
        public static async Task<int> DeleteAsync(Customer customer)
        {
            using (var db = GetOpenConnection())
            {
                string query = $@"DELETE TOP(1) FROM {Customer.Table} WHERE [Id]=@Id;";
                try
                {
                    return await db.ExecuteAsync(query, customer);
                }
                catch
                {
                    Logger.LogError($@"SqlQuery: {query} using: {Newtonsoft.Json.JsonConvert.SerializeObject(customer)}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Reads a list of Customer records.
        /// </summary>
        /// <returns>IEnumerable of all Customer records.</returns>
        public static async Task<IEnumerable<Customer>> ListAsync()
        {
            using (var db = GetOpenConnection())
            {
                string query = BaseQuery();
                try
                {
                    return await db.QueryAsync<Customer>(query);
                }
                catch
                {
                    Logger.LogError($@"SqlQuery: {query}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Reads a record from Customers.
        /// </summary>
        /// <param name="Id">Customer.Id of target Customer to read.</param>
        /// <returns>The Customer record.</returns>
        public static async Task<Customer> ReadAsync(int Id)
        {
            using (var db = GetOpenConnection())
            {
                string query = $@"{BaseQuery()} WHERE [Id]=@Id;";
                object param = new { Id };
                try
                {
                    return await db.QuerySingleAsync<Customer>(query, param);
                }
                catch
                {
                    Logger.LogError($@"SqlQuery: {query} using: {Newtonsoft.Json.JsonConvert.SerializeObject(param)}");
                    throw;
                }
            }
        }
        public static Customer Read(int Id)
        {
            var task = Task.Run<Customer>(async () => await ReadAsync(Id));
            return task.Result;
        }

        /// <summary>
        /// Updates a record in Customers
        /// </summary>
        /// <param name="customer">Customer value(s) to update.</param>
        /// <returns>Number of records changed.</returns>
        public static async Task<int> UpdateAsync(Customer customer)
        {
            using (var db = GetOpenConnection())
            {
                string query = $@"UPDATE {Customer.Table}
                    SET [FirstName]=@FirstName, [LastName]=@LastName, [CompanyName]=@CompanyName, [Address]=@Address, [City]=@City, [County]=@County, [State]=@State, [Zip]=@Zip, [Phone1]=@Phone1, [Phone2]=@Phone2, [Email]=@Email, [Web]=@Web
                    WHERE [Id]=@Id;";
                try
                {
                    return await db.ExecuteAsync(query, customer);
                }
                catch
                {
                    Logger.LogError($@"SqlQuery: {query} using: {Newtonsoft.Json.JsonConvert.SerializeObject(customer)}");
                    throw;
                }
            }
        }
        
        #endregion
    }
}
