using Sitecore.ContentSearch;
using Sitecore.Data.Items;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaCrawler : SitecoreItemCrawler
    {
        public string ShowInSearchResultsFieldName { get; set; }
        public string HideFromSearchResultsFieldName { get; set; }

        protected override bool IsExcludedFromIndex(SitecoreIndexableItem indexable, bool checkLocation = false)
        {
            var result = base.IsExcludedFromIndex(indexable, checkLocation);

            if (result)
                return true;

            var obj = (Item)indexable;

            if (!string.IsNullOrWhiteSpace(ShowInSearchResultsFieldName))
            {
                var showInSearchResultsField = obj.Fields[ShowInSearchResultsFieldName];
                if (showInSearchResultsField != null)
                {
                    result = string.IsNullOrWhiteSpace(showInSearchResultsField.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(HideFromSearchResultsFieldName))
            {
                var hideFromSearchResultsField = obj.Fields[HideFromSearchResultsFieldName];
                if (hideFromSearchResultsField != null)
                {
                    if (bool.TryParse(hideFromSearchResultsField.Value, out result))
                    {
                        // If the value is a bool return that value
                        return result;

                    }
                    else if (hideFromSearchResultsField.Value == "0")
                    {
                        // If the value is a zero, it's probably a bool and return false
                        return false;
                    }
                    else
                    {
                        result = !string.IsNullOrWhiteSpace(hideFromSearchResultsField.Value);
                    }
                }
            }

            return result;
        }
    }
}