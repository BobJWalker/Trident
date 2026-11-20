using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Trident.Web.Core.Models;

namespace Trident.Web.BusinessLogic.Factories
{
    public interface IArticleListFactory
    {
        List<ArticleModel> MakeArticleModelListFromWebContent(FeedModel feedModel, string webContent);
        List<ArticleModel> MakeArticleModelListFromWebContent(FeedModel feedModel, List<ArticleModel> articleList);
    }

    public class ArticleListFactory(ILogger<ArticleListFactory> logger) : IArticleListFactory
    {
        public List<ArticleModel> MakeArticleModelListFromWebContent(FeedModel feedModel, string webContent)
        {            
            var doc = new HtmlDocument();
            doc.LoadHtml(webContent);

            var anchorList = doc.DocumentNode.SelectNodes("//a[@href]");
            if (anchorList?.Count == 0)
            {
                logger.LogInformation($"Unable to find any anchor tags for ${feedModel.Name}.  Returning empty list");
                return new List<ArticleModel>();
            }

            var tmpArticleList = new Dictionary<string, ArticleModel>();
            foreach (var anchor in anchorList)
            {
                var href = HtmlEntity.DeEntitize(anchor.GetAttributeValue("href", ""));

                if (MatchesFeedFilter(feedModel, href) == false)
                {
                    logger.LogInformation($"${href} does not match any of the filters, moving onto the next anchor tag");
                    continue;
                }

                var articleModel = MakeArticleModel(anchor, href, feedModel);

                if (articleModel == null)
                {
                    continue;
                }

                if(tmpArticleList.ContainsKey(href.ToLowerInvariant()))
                {
                    logger.LogInformation($"tmpArticleList already contains ${href}, updating existing item");
                    tmpArticleList[href].Thumbnail = string.IsNullOrWhiteSpace(tmpArticleList[href].Thumbnail) ? articleModel.Thumbnail : tmpArticleList[href].Thumbnail;
                    tmpArticleList[href].Title = string.IsNullOrWhiteSpace(tmpArticleList[href].Title) ? articleModel.Title : tmpArticleList[href].Title;
                }
                else
                {
                    logger.LogInformation($"tmpArticleList does not ${href}, adding to list");
                    tmpArticleList.Add(href.ToLowerInvariant(), articleModel);
                }
            }

            return tmpArticleList.Values.ToList();
        }

        public List<ArticleModel> MakeArticleModelListFromWebContent(FeedModel feedModel, List<ArticleModel> articleList)
        {
            var returnList = new List<ArticleModel>();

            foreach (var article in articleList)
            {
                if (MatchesFeedFilter(feedModel, article.Url) == false)
                {
                    logger.LogInformation($"{article.Url} does not match the filters for the feed, excluding it");
                    continue;
                }

                article.FeedName = feedModel.Name;
                returnList.Add(article);
            }

            return returnList;
        }

        private bool MatchesFeedFilter(FeedModel feedModel, string href)
        {
            if (string.IsNullOrWhiteSpace(href))
            {
                return false;
            }

            foreach (var filter in feedModel.ArticleBaseUrls)
            {
                if (href.ToLowerInvariant().StartsWith(filter.ToLowerInvariant(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private ArticleModel MakeArticleModel(HtmlNode anchor, string href, FeedModel feedModel)
        {
            var articleModel = new ArticleModel
            {
                Url = href,  
                FeedName = feedModel.Name
            };

            var img = anchor.SelectSingleNode(".//img[@src]");

            if (img != null)
            {
                var src = HtmlEntity.DeEntitize(img.GetAttributeValue("src", ""));
                logger.LogInformation($"Found the image url title in the anchor tag ${anchor.InnerText} as ${src}");

                articleModel.Thumbnail = src.IndexOf("?") > 0 ? src.Substring(0, src.IndexOf("?")) : src;

                return articleModel;
            }

            var headerOne = GetHeaderTitle(anchor, "h1");
            
            if (headerOne != null)
            {
                articleModel.Title = headerOne;
                return articleModel;
            }

            var headerTwo = GetHeaderTitle(anchor, "h2");

            if (headerTwo != null)
            {
                articleModel.Title = headerTwo;
                return articleModel;
            }

            var title = Regex.Replace(HtmlEntity.DeEntitize(anchor.InnerText), @"\s+", " ").Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                logger.LogInformation($"Unable to find the title in the anchor tag ${anchor.InnerText}");
                return null;
            }

            articleModel.Title = title;
            return articleModel;
        }

        private string GetHeaderTitle(HtmlNode anchor, string headerHtml)
        {
            var header = anchor.SelectSingleNode($".//{headerHtml}");

            if (header == null)
            {
                return null;
            }
                        
            return Regex.Replace(HtmlEntity.DeEntitize(header.InnerText), @"\s+", " ").Trim();                            
        }
    }
}
