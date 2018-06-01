// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using Dapper;
using DataTablesCoreMVCDapper.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DataTablesCoreMVCDapper.Data
{
    /// <summary>
    ///  A DataTablesContext instance represents an AJAX session between the database and
    ///  the jQuery DataTables Plugin and is used to process the reqwuest and return
    ///  sorted, filtered and paged data. DataTablesContext is a combination of the
    ///  root SQL query returning records of type TSource and the DataTables parameters
    ///  defining sorting, filtering and paging criteria.
    /// </summary>
    public class DataTablesContext
    {
        public DataTablesContext(string query, DataTablesRequest request)
        {
            Query = query;
            Request = request;
        }

        public static string SearchToken { get; } = "SearchValue";
        public string Query { get; protected internal set; }
        public DataTablesRequest Request { get; protected set; }

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
        /// Runs the specified data query and returns an IEnumerable of one or more TSource records.
        /// </summary>
        public static async Task<IEnumerable<TSource>> QueryAsync<TSource>(string query, object param)
        {
            using (var db = GetOpenConnection())
            {
                return await db.QueryAsync<TSource>(query, param);
            }
        }

        /// <summary>
        /// Runs the specified T query and return the single T result.
        /// </summary>
        public static async Task<T> QuerySingleAsync<T>(string query, object param = null)
        {
            using (var db = GetOpenConnection())
            {
                return await db.QuerySingleAsync<T>(query, param);
            }
        }
    }

    /// <summary>
    /// Extensions to format and execute generic SQL queries from DataTablesContext.Request settings
    /// </summary>
    /// <remarks>
    /// These extensions link the DataTablesContext with data returned by Dapper ORM.  The SQL query
    /// string formatters prepare the SQL query (DataTablesContext.Query) for sorting, filtering and
    /// paging using criteria defined by the DataTables AJAX request (DataTablesContext.Request.)
    /// The SQL query executors pass one or more SQL queries through mehtods in DataTablesContext
    /// down to Dapper ORM returning data needed to fulfill the DataTables AJAX request.
    /// </remarks>
    public static class DataTablesContextExtensions
    {
        #region SQL Query Formatters

        // methods to format fragments of a SQL query in the form:
        // SELECT * FROM (BASE-QUERY) WHERE-CLAUSE ORDER-BY OFFSET-FETCH
        // where:
        //  BASE-QUERY is the SQL query to populate a list of data-model-class objects.
        //      the base query is a complete SQL query without ORDER-BY or GROUP-BY declaritives,
        //      and may include one or more JOIN to populate lists of data-model-class objects,
        //      example: SELECT * FROM [table]
        //  WHERE-CLAUSE is the SQL query fragment used to filter results by search string.
        //      example: WHERE [column0] LIKE '%@SearchValue%' OR [column1] LIKE '%@SearchValue%' OR ...
        //  ORDER-BY orders results by one or more columns. example: ORDER BY [column1] DESC
        //  OFFSET-FETCH limits result set returns for a given page. example: OFFSET 20 ROWS
        //      FETCH NEXT 10 ROWS ONLY
        // fragments are used to build three different queries returning:
        //  count of all displayable rows in table
        //  count of filtered (using search item) rows in table
        //  list of (optionally) filtered and ordered data rows for one page in DataTables UI table.

        public static DataTablesContext OrderBy(this DataTablesContext context)
        {
            try
            {
                context.Query = string.Concat(context.Query, context.Request.Order.Count == 0
                    // when no orderby specified, provide generic ORDER BY
                    // because ORDER BY is required when using OFFSET-FETCH.
                    ? @" ORDER BY 1"
                    : @" ORDER BY " + string.Join(",", context.Request.Order
                        .Select(o => $@"[{context.Request.Columns[o.Column].Name}]"
                            + (o.Descending ? " DESC" : "")
                        )
                    ));
            }
            catch (Exception ex)
            {
                context.Request.Error += string.Format("{0}: {1}\n{2}",
                    ex.GetType().Name,
                    ex.Message.TrimEnd('.'),
                    ex.StackTrace);
            }
            return context;
        }

        public static DataTablesContext SkipTake(this DataTablesContext context)
        {
            try
            {
                // OFFSET-FETCH works beautifully to give us a specific page of rows
                // from the full result set, but requires the use of ORDER BY.
                var offset = Math.Max(0, context.Request.Start);
                var fetch = Math.Max(0, context.Request.Length);
                context.Query = string.Concat(context.Query, $@" OFFSET {offset} ROWS" +
                    (fetch > 0 ? $" FETCH NEXT {fetch} ROWS ONLY" : string.Empty));
            }
            catch (Exception ex)
            {
                context.Request.Error += string.Format("{0}: {1}\n{2}",
                    ex.GetType().Name,
                    ex.Message.TrimEnd('.'),
                    ex.StackTrace);
            }
            return context;
        }

        public static DataTablesContext Where(this DataTablesContext context)
        {
            try
            {
                // Note: the use of percent wilcard around search value.
                context.Query = string.Concat(context.Query, (string.IsNullOrWhiteSpace(context.Request.Search?.Value))
                    ? " WHERE 1=1"
                    : " WHERE " + string.Join(" OR ", context.Request.Columns
                        .Where(c => c.Searchable)
                        .Select(c => $@"[{c.Name}] LIKE '%'+@{DataTablesContext.SearchToken}+'%'")));
            }
            catch (Exception ex)
            {
                context.Request.Error += string.Format("{0}: {1}\n{2}",
                    ex.GetType().Name,
                    ex.Message.TrimEnd('.'),
                    ex.StackTrace);
            }
            return context;
        }

        #endregion

        #region SQL Query Executors
        // methods to execute generic SQL queries using Dapper

        public static DynamicParameters GetParam(this DataTablesContext context)
        {
            var param = new DynamicParameters();
            param.Add(DataTablesContext.SearchToken, context.Request.Search?.Value);
            return param;
        }

        /// <summary>
        /// Runs the context COUNT query and return the integer result.
        /// </summary>
        public static async Task<int> CountAsync(this DataTablesContext context)
        {
            var query = $@"SELECT COUNT(*) FROM ( {context.Query} ) [COUNT_DERIVED];";
            try
            {
                return await DataTablesContext.QuerySingleAsync<int>(query, context.GetParam());
            }
            catch (Exception ex)
            {
                context.Request.Error += string.Format("{0}: {1}\nSqlQuery: {2} using: {3}\n{4}",
                    ex.GetType().Name,
                    ex.Message.TrimEnd('.'),
                    query,
                    Newtonsoft.Json.JsonConvert.SerializeObject(context.GetParam()),
                    ex.StackTrace);
                return 0;
            }
        }

        /// <summary>
        /// Runs the context data query and returns an IEnumerable of one or more TSource records.
        /// </summary>
        public static async Task<IEnumerable<TSource>> DataAsync<TSource>(this DataTablesContext context)
        {
            try
            {
                return await DataTablesContext.QueryAsync<TSource>(context.Query, context.GetParam());
            }
            catch (Exception ex)
            {
                context.Request.Error += string.Format("{0}: {1}\nSqlQuery: {2} using: {3}\n{4}",
                    ex.GetType().Name,
                    ex.Message.TrimEnd('.'),
                    context.Query,
                    Newtonsoft.Json.JsonConvert.SerializeObject(context.GetParam()),
                    ex.StackTrace);
                return new List<TSource>();
            }
        }

        #endregion
    }
}
