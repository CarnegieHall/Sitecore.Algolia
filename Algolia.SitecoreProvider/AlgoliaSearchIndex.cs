﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Algolia.SitecoreProvider.Abstract;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Abstractions;
using Sitecore.ContentSearch.Events;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.Maintenance.Strategies;
using Sitecore.ContentSearch.Security;
using Sitecore.Eventing;
using Sitecore.Events;

namespace Algolia.SitecoreProvider
{
    public class AlgoliaSearchIndex : ISearchIndex
    {
        private readonly AlgoliaConfig _config;
        private readonly IAlgoliaRepository _repository;


        public AlgoliaSearchIndex(string name, string applicationId, string fullApiKey, IIndexPropertyStore propertyStore)
        {
            if (propertyStore == null) throw new ArgumentNullException("propertyStore");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentOutOfRangeException("name");
            if (string.IsNullOrWhiteSpace(applicationId)) throw new ArgumentOutOfRangeException("applicationId");
            if (string.IsNullOrWhiteSpace(fullApiKey)) throw new ArgumentOutOfRangeException("fullApiKey");

            Name = name;
            PropertyStore = propertyStore;
            
            var config = new AlgoliaConfig
            {
                ApplicationId = applicationId,
                FullApiKey = fullApiKey,
                IndexName = name
            };

            _config = config;
            _repository = new AlgoliaRepository(_config);

            this.Crawlers = new List<IProviderCrawler>();
            this.Strategies = new List<IIndexUpdateStrategy>();
        }

        public List<IIndexUpdateStrategy> Strategies { get; private set; }

        #region ISearchIndex

        public void AddCrawler(IProviderCrawler crawler)
        {
            crawler.Initialize(this);
            this.Crawlers.Add(crawler);
        }

        public void AddStrategy(IIndexUpdateStrategy strategy)
        {
            strategy.Initialize(this);
            this.Strategies.Add(strategy);
        }

        public void Rebuild()
        {
            Rebuild(IndexingOptions.Default);
        }

        public void Rebuild(IndexingOptions indexingOptions)
        {
            Event.RaiseEvent("indexing:start", new object[] { this.Name, true });
            var event2 = new IndexingStartedEvent
            {
                IndexName = this.Name,
                FullRebuild = true
            };
            EventManager.QueueEvent<IndexingStartedEvent>(event2);
            this.Reset();
            this.DoRebuild(indexingOptions);
            Event.RaiseEvent("indexing:end", new object[] { this.Name, true });
            var event3 = new IndexingFinishedEvent
            {
                IndexName = this.Name,
                FullRebuild = true
            };
            EventManager.QueueEvent<IndexingFinishedEvent>(event3);
        }

        public Task RebuildAsync(IndexingOptions indexingOptions, CancellationToken cancellationToken)
        {
            return Task.Run(() => Rebuild(indexingOptions), cancellationToken);
        }

        public void Refresh(IIndexable indexableStartingPoint)
        {
            Refresh(indexableStartingPoint, IndexingOptions.Default);
        }

        public void Refresh(IIndexable indexableStartingPoint, IndexingOptions indexingOptions)
        {
            using (var context = this.CreateUpdateContext())
            {
                foreach (var crawler in this.Crawlers)
                {
                    crawler.RefreshFromRoot(context, indexableStartingPoint, indexingOptions);
                }
                context.Optimize();
                context.Commit();
            }
        }

        public Task RefreshAsync(IIndexable indexableStartingPoint, IndexingOptions indexingOptions,
            CancellationToken cancellationToken)
        {
            return Task.Run(() => Console.WriteLine(""));
        }

        public void Update(IIndexableUniqueId indexableUniqueId)
        {
           Update(indexableUniqueId, IndexingOptions.Default);
        }

        public void Update(IIndexableUniqueId indexableUniqueId, IndexingOptions indexingOptions)
        {
            Update(new List<IIndexableUniqueId> {indexableUniqueId}, IndexingOptions.Default);
        }

        public void Update(IEnumerable<IIndexableUniqueId> indexableUniqueIds)
        {
            Update(indexableUniqueIds, IndexingOptions.Default);
        }

        public void Update(IEnumerable<IIndexableUniqueId> indexableUniqueIds, IndexingOptions indexingOptions)
        {
            using (var context = this.CreateUpdateContext())
            {
                foreach (var crawler in this.Crawlers)
                {
                    foreach (var indexableUniqueId in indexableUniqueIds)
                    {
                        crawler.Update(context, indexableUniqueId, indexingOptions);
                    }
                }
                context.Commit();
            }
        }

        public void Update(IEnumerable<IndexableInfo> indexableInfo)
        {
            Update(indexableInfo.Select(t => t.IndexableUniqueId));
        }

        public void Delete(IIndexableId indexableId)
        {
            Delete(indexableId, IndexingOptions.Default);
        }

        public void Delete(IIndexableId indexableId, IndexingOptions indexingOptions)
        {
            using (var context = this.CreateUpdateContext())
            {
                foreach (var crawler in this.Crawlers)
                {
                    crawler.Delete(context, indexableId, indexingOptions);
                }
                context.Commit();
            }
        }

        public void Delete(IIndexableUniqueId indexableUniqueId)
        {
            Delete(indexableUniqueId, IndexingOptions.Default);
        }

        public void Delete(IIndexableUniqueId indexableUniqueId, IndexingOptions indexingOptions)
        {
            using (var context = this.CreateUpdateContext())
            {
                foreach (var crawler in this.Crawlers)
                {
                    crawler.Delete(context, indexableUniqueId, indexingOptions);
                }
                context.Commit();
            }
        }

        public void Reset()
        {
            _repository.ClearIndexAsync().Wait();
        }

        public void Initialize()
        {

        }

        public IProviderUpdateContext CreateUpdateContext()
        {
            return new AlgoliaUpdateContext(this, _repository);
        }

        public IProviderDeleteContext CreateDeleteContext()
        {
            return null;
        }

        public IProviderSearchContext CreateSearchContext(
            SearchSecurityOptions options = SearchSecurityOptions.EnableSecurityCheck)
        {
            return null;
        }

        public void StopIndexing()
        {

        }

        public void PauseIndexing()
        {

        }

        public void ResumeIndexing()
        {

        }

        public string Name { get; private set; }
        public ISearchIndexSummary Summary { get; private set; }
        public ISearchIndexSchema Schema { get; private set; }
        public IIndexPropertyStore PropertyStore { get; set; }
        public AbstractFieldNameTranslator FieldNameTranslator { get; set; }
        public ProviderIndexConfiguration Configuration { get; set; }
        public IIndexOperations Operations { get; private set; }
        public IndexingState IndexingState { get; private set; }
        public IList<IProviderCrawler> Crawlers { get; private set; }
        public IObjectLocator Locator { get; private set; }

        #endregion

        protected virtual void DoRebuild(IndexingOptions indexingOptions)
        {
            using (var context = this.CreateUpdateContext())
            {
                foreach (var crawler in this.Crawlers)
                {
                    crawler.RebuildFromRoot(context, indexingOptions);
                }
                context.Optimize();
                context.Commit();
            }
        }

    }
}
