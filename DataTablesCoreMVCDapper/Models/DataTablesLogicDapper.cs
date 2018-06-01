// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using DataTablesCoreMVCDapper.Data;
using System.Threading.Tasks;

namespace DataTablesCoreMVCDapper.Models
{
    /// <summary>
    /// The DataTablesLogicDapper links DataTables AJAX requests from MVC Controllers to non-generic
    /// response handlers fulfilled by Dapper ORM.  The non-generic methods define the root SQL query
    /// and pass requests through the DataTablesRequestAsync generic method for request fulfillment.
    /// </summary>
    public class DataTablesLogicDapper
    {
        /// <summary>
        /// Handles generic DataTablesRequest for TSource data using Dapper ORM
        /// </summary>
        /// <param name="request">DataTables Ajax Request</param>
        /// <param name="baseSQLQuery">The base SQL query for TSource</param>
        /// <returns>DataTablesResponse for TSource data</returns>
        public static async Task<DataTablesResponse<TSource>> DataTablesRequestAsync<TSource>(DataTablesRequest request, string baseSQLQuery)
        {
            // prepare the data and count SQL queries
            var totalQuery = new DataTablesContext(baseSQLQuery, request);
            var filterQuery = totalQuery.Where();
            var dataQuery = filterQuery.OrderBy().SkipTake();

            // run the queries and return the response
            return new DataTablesResponse<TSource>()
            {
                Draw = request.Draw,
                RecordsTotal = await totalQuery.CountAsync(),
                RecordsFiltered = await filterQuery.CountAsync(),
                Data = await dataQuery.DataAsync<TSource>(),
                Error = request.Error,
            };
        }

        /// <summary>
        /// Handles the DataTablesRequest for the Customer Database
        /// </summary>
        /// <param name="request">DataTables Ajax Request</param>
        /// <returns>DataTablesResponse for Customer table</returns>
        /// <remarks>
        /// When intergration with projects without Data.Repository classes, the 
        /// base SQL query may be hard-coded here when making the call to the generic
        /// DataTablesRequestAsync() method.
        /// </remarks>
        public static async Task<DataTablesResponse<Customer>> DataTablesCustomerRequestAsync(DataTablesRequest request)
        {
            return await DataTablesRequestAsync<Customer>(request, CustomerRepository.BaseQuery());
        }
    }
}
