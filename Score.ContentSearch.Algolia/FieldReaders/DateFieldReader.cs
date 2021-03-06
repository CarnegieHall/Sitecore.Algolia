﻿using System;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.FieldReaders;
using Sitecore.Data.Fields;

namespace Score.ContentSearch.Algolia.FieldReaders
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
