using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
namespace RESSWATCH
{
    public class ItemFactory<T> where T : new()
    {
        public static void SetItemFromRow(T item, DataRow row)
        {
            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                PropertyInfo p = item.GetType().GetProperty(c.ColumnName);
                // if exists, set the value
                if (p != null && row[c] != DBNull.Value)
                    p.SetValue(item, row[c], null); // TODO Change to default(_) if this is not a reference type /);
            }
        }

        public static T CreateItemFromRow(DataRow row)
        {
            // create a new object
            T item = new T();
            // set the item
            SetItemFromRow(item, row);
            // return 
            return item;
        }
    }
}