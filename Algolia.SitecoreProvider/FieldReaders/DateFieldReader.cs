﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.FieldReaders;
using Sitecore.Data.Fields;

namespace Algolia.SitecoreProvider.FieldReaders
{
    public class DateFieldReader : FieldReader
    {
        public override object GetFieldValue(IIndexableDataField indexableField)
        {
            Field field = indexableField as SitecoreItemDataField;
            DateField dateField = FieldTypeManager.GetField(field) as DateField;
            if (dateField != null)
            {
                var date = dateField.DateTime;

                if (date == DateTime.MinValue)
                    return null;

                return (Int32)(date.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }
            return null;
        }
    }
}
