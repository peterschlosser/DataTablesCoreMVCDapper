// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataTablesCoreMVCDapper.Models
{
    /// <summary>
    /// Parses (binds) HTTP Input parameters from jQuery DataTable Ajax requests
    /// to the DataTablesRequest class model.
    /// </summary>
    /// <remarks>
    /// This model binder does not decode JSON input.  Content-type from DataTables
    /// must be application/x-www-form-urlencoded, which it is by default in v1.10.
    /// Tested with Methods "GET" and "POST", with and without [FromBody] binder.  
    /// Tested in ASP.NET Core v1.1 and Core v2.0.
    /// </remarks>
    public class DataTablesModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var Values = bindingContext.ValueProvider;

            if (Values.GetValue("draw") == ValueProviderResult.None)
            {
                throw new ArgumentNullException("draw", "Bad or missing DataTables argument.");
            }

            var draw = Convert.ToInt32(Values.GetValue("draw").ToString());
            var start = Convert.ToInt32(Values.GetValue("start").ToString());
            var length = Convert.ToInt32(Values.GetValue("length").ToString());

            // parses search[value]
            // parses search[regex]
            var search = new SearchInfo
            {
                Value = Values.GetValue("search[value]").ToString(),
                Regex = Convert.ToBoolean(Values.GetValue("search[regex]").ToString())
            };

            // parses columns[n][data]
            // parses columns[n][name]
            // parses columns[n][orderable]
            // parses columns[n][searchable]
            // parses columns[n][search][value]
            // parses columns[n][search][regex]
            var columns = new List<ColumnInfo>();
            for (var i = 0; true; i++)
            {
                var keyData = $@"columns[{i}][data]";
                var keyName = $@"columns[{i}][name]";
                var keyOrderable = $@"columns[{i}][orderable]";
                var keySearchable = $@"columns[{i}][searchable]";
                var keyValue = $@"columns[{i}][search][value]";
                var keyRegex = $@"columns[{i}][search][regex]";

                if (!Values.ContainsPrefix(keyData) ||
                    string.IsNullOrWhiteSpace(Values.GetValue(keyData).ToString()))
                    break;

                columns.Add(new ColumnInfo
                {
                    Data = Values.GetValue(keyData).ToString(),
                    Name = Values.GetValue(keyName).ToString(),
                    Orderable = Boolean.TryParse(Values.GetValue(keyOrderable).ToString(), out var orderable) ? orderable : false,
                    Searchable = Boolean.TryParse(Values.GetValue(keySearchable).ToString(), out var searchable) ? searchable : false,
                    Search = new SearchInfo
                    {
                        Value = Values.GetValue(keyValue).ToString(),
                        Regex = Boolean.TryParse(Values.GetValue(keyRegex).ToString(), out var regex) ? regex : false
                    }
                });
            }

            // parses order[n][column]
            // parses order[n][dir]
            var order = new List<SortInfo>();
            for (var i = 0; true; i++)
            {
                var keyColumn = $@"order[{i}][column]";
                var keyDir = $@"order[{i}][dir]";

                if (!Values.ContainsPrefix(keyColumn) ||
                    string.IsNullOrWhiteSpace(Values.GetValue(keyColumn).ToString()) ||
                    Int32.TryParse(Values.GetValue(keyColumn).ToString(), out int column) == false)
                {
                    break;
                }
                if (columns[column].Orderable)
                {
                    order.Add(new SortInfo
                    {
                        Column = column,
                        Descending = Values.GetValue(keyDir).ToString().ToLower() == "desc"
                    });
                }
            }

            // bind all the DataTablesRequest data together
            bindingContext.Result = ModelBindingResult.Success(new DataTablesRequest
            {
                Draw = draw,
                Start = start,
                Length = length,
                Search = search,
                Order = order,
                Columns = columns,
            });

            return Task.CompletedTask;

        }
    }
}
