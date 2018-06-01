// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Collections.Generic;

namespace DataTablesCoreMVCDapper.Models
{
    public class DataTablesResponse<T>
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public IEnumerable<T> Data { get; set; }
        public string Error { get; set; }

        public DataTablesResponse()
        {
            Data = new List<T>();
        }
    }
}
