﻿using System.Collections.Generic;
using System.Xml;
using Moq;
using Score.ContentSearch.Algolia.Abstract;
using Score.ContentSearch.Algolia.ComputedFields;
using Score.ContentSearch.Algolia.FieldsConfiguration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.FieldReaders;
using Sitecore.ContentSearch.Maintenance;

namespace Score.ContentSearch.Algolia.Tests.Builders
{
    internal class IndexBuilder
    {
        private readonly ISearchIndex _index;
        private readonly AlgoliaIndexConfiguration _configuration;

        public IndexBuilder()
        {
            var algoliaRepository = new Mock<IAlgoliaRepository>();
            _index = new AlgoliaBaseIndex("index_name", algoliaRepository.Object);
            _configuration = new AlgoliaIndexConfiguration
            {
                DocumentOptions = new DocumentBuilderOptions(),
                FieldMap = new FieldMap(),
                FieldReaders = new FieldReaderMap(),
            };
            _index.Configuration = _configuration;
        }

        public IndexBuilder WithSimpleFieldTypeMap(string typeKey)
        {
            var fieldConfig = new SimpleFieldsConfiguration(string.Empty, string.Empty, typeKey,
                new Dictionary<string, string>(), new XmlDocument());
            var fieldMap = _index.Configuration.FieldMap as FieldMap;

            fieldMap.Add(fieldConfig);
            return this;
        }

        public IndexBuilder WithParentsComputedField(string fieldName)
        {
            var field = new ParentIdsField {FieldName = fieldName};
            _index.Configuration.DocumentOptions.ComputedIndexFields.Add(field);
            return this;
        }

        public IndexBuilder WithMaxFieldLength(int maxfieldLength)
        {
            (_index.Configuration as IIndexCustomOptions).MaxFieldLength = maxfieldLength;
            return this;
        }

        public IndexBuilder WithIncludeTemplateId()
        {
            (_index.Configuration as IIndexCustomOptions).IncludeTemplateId = true;
            return this;
        }

        public IndexBuilder WithIncludedTemplate(string templateId)
        {
            _index.Configuration.IncludeTemplate(templateId);
            return this;
        }

        public IndexBuilder WithTagsBuilderForId()
        {
            _configuration.TagsProcessor = new AlgoliaTagsProcessor(new List<AlgoliaTagConfig>
            {
                new AlgoliaTagConfig
                {
                    FieldName = "_id"
                }
            });
            return this;
        }

        public IndexBuilder WithIndexAllFields()
        {
            _index.Configuration.IndexAllFields = true;
            return this;
        }

        public IndexBuilder WithDefaultFieldReader(string fieldTypeName)
        {
            AddStandardFieldReader(fieldTypeName, "DefaultFieldReader");
            return this;
        }

        public IndexBuilder WithNumericFieldReader(string fieldTypeName)
        {
            AddCustomFieldReader(fieldTypeName, "NumberFieldReader");
            return this;
        }

        public IndexBuilder WithDateFieldReader(string fieldTypeName)
        {
            AddCustomFieldReader(fieldTypeName, "DateFieldReader");
            return this;
        }

        public ISearchIndex Build()
        {
            return _index;
        }

        private void AddStandardFieldReader(string fieldTypeName, string fieldReaderType)
        {
            var fieldTypes = fieldTypeName.Split('|');
            string readerType = $"Sitecore.ContentSearch.FieldReaders.{fieldReaderType}, Sitecore.ContentSearch";
            _index.Configuration.FieldReaders.AddFieldReaderByFieldTypeName(readerType, fieldTypes);
        }

        private void AddCustomFieldReader(string fieldTypeName, string fieldReaderType)
        {
            var fieldTypes = fieldTypeName.Split('|');
            string readerType =
                $"Score.ContentSearch.Algolia.FieldReaders.{fieldReaderType}, Score.ContentSearch.Algolia";
            _index.Configuration.FieldReaders.AddFieldReaderByFieldTypeName(readerType, fieldTypes);
        }
    }
}
