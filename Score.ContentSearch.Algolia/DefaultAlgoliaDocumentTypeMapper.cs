﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;

namespace Score.ContentSearch.Algolia
{
    public class DefaultAlgoliaDocumentTypeMapper: DefaultDocumentMapper<JObject>
    {
        protected override IEnumerable<string> GetDocumentFieldNames(JObject document)
        {
            throw new NotImplementedException();
        }

        protected override IDictionary<string, object> ReadDocumentFields(JObject document, IEnumerable<string> fieldNames, IEnumerable<Sitecore.ContentSearch.Linq.Common.IFieldQueryTranslator> virtualFieldProcessors)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        protected override void ReadDocumentFields<TElement>(JObject document, IEnumerable<string> fieldNames, DocumentTypeMapInfo documentTypeMapInfo, IEnumerable<Sitecore.ContentSearch.Linq.Common.IFieldQueryTranslator> virtualFieldProcessors, TElement result)
        {
            throw new NotImplementedException();
        }
    }
}
