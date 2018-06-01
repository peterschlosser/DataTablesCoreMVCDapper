// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DataTablesCoreMVCDapper.Models
{
    // The ModelBinder attribute routes/binds Controller input decode requests
    // to this DataTablesModelBinder:IModelBinder class.  It saves us from the
    // need to declare an IModelBinderProvider and registering it in 
    // Startup.ConfigureServices().
    [ModelBinder(BinderType = typeof(DataTablesModelBinder))]
    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public SearchInfo Search { get; set; }
        public List<SortInfo> Order { get; set; }
        public List<ColumnInfo> Columns { get; set; }
        public string Error { get; set; } = "";
    }

    public class SearchInfo
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }

    public class SortInfo
    {
        public int Column { get; set; }
        public bool Descending { get; set; }
    }

    public class ColumnInfo
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public SearchInfo Search { get; set; }
    }
}
