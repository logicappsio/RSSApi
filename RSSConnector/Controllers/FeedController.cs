using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using TRex.Metadata;
using System.Net.Http;
using Microsoft.Azure.AppService.ApiApps.Service;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Web;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Web.Http.Description;

namespace RSSConnector.Controllers
{
    public class FeedController : ApiController
    {
        [HttpGet]
        [Metadata(friendlyName: "Check Feed")]
        public Item GetFeedItem(string FeedUri, string FromDate = "", string Keywords = "")
        {
            return GetNextItem(FeedUri, Keywords, String.IsNullOrEmpty(FromDate) ? DateTime.Now : DateTime.Parse(FromDate));
        }

        /// <summary>
        /// Poll feed Url for changes since last poll
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [HttpGet]
        [Metadata(friendlyName: "Feed Item Trigger", description: "When a new feed item is found, the Logic App will fire")]
        [Trigger(TriggerType.Poll)]
        [ResponseType(typeof(Item))]
        public HttpResponseMessage NewFeedItemTrigger(string triggerState, string FeedUri, string Keywords = "")
        {
            DateTime sinceDate;

            try {
                // if there is a triggerState, we will use that date to check for a new feed item
                if (string.IsNullOrEmpty(triggerState))
                {
                    sinceDate = DateTime.Now;
                }
                else
                {
                    sinceDate = DateTime.Parse(triggerState);
                }

                var item = GetNextItem(FeedUri, Keywords, sinceDate);
               
                if (null == item)
                    return Request.EventWaitPoll(null, Convert.ToString(sinceDate));
                else
                    return Request.EventTriggered(item, Convert.ToString(item.PubDate), pollAgain: TimeSpan.Zero);
                }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// get the next item after the since date provided
        /// </summary>
        /// <param name="SinceDate"></param>
        /// <returns></returns>
        Item GetNextItem(string feedUri, string keywords, DateTime SinceDate)
        {
            Item item = null;
            var request = WebRequest.Create(feedUri);
            var response = request.GetResponse();
            DateTime lastDate = DateTime.Now;
            var s = response.GetResponseStream();

            // spin over feed items
            XmlReader rdr = XmlReader.Create(s);
            while (rdr.Read())
            {
                if (rdr.IsStartElement())
                {
                    if (rdr.LocalName == "item")
                    {
                        /// spin over contents of each item
                        string itemId = null;
                        string link = null;
                        string authorName = null;
                        string authorUri = null;
                        string title = null;
                        string description = null;
                        string pubDate = null;
                        string lastUpdate = null;

                        rdr.Read();
                        while (1 == 1)
                        {
                            #region item
                            if (rdr.IsStartElement())
                            {
                                switch (rdr.LocalName)
                                {
                                    case "guid":
                                        itemId = rdr.ReadElementContentAsString();
                                        break;
                                    case "link":
                                        link = rdr.ReadElementContentAsString();
                                        break;
                                    case "author":
                                        // read the author name
                                        // read the author uri
                                        rdr.Read(); // read the next element
                                        while (rdr.LocalName != "author")
                                        {
                                            if (rdr.IsStartElement())
                                            {
                                                switch (rdr.LocalName)
                                                {
                                                    case "name":
                                                        authorName = rdr.ReadElementContentAsString();
                                                        break;
                                                    case "uri":
                                                        authorUri = rdr.ReadElementContentAsString();
                                                        break;
                                                    default:
                                                        rdr.ReadElementContentAsString();
                                                        break;
                                                }
                                            }
                                        }
                                        rdr.Read();
                                        break;
                                    case "title":
                                        title = rdr.ReadElementContentAsString();
                                        break;
                                    case "description":
                                        description = rdr.ReadElementContentAsString();
                                        break;
                                    case "pubDate":
                                        pubDate = rdr.ReadElementContentAsString();
                                        break;
                                    case "updated":
                                        lastUpdate = rdr.ReadElementContentAsString();
                                        break;
                                    default:
                                        rdr.ReadElementContentAsString();
                                        break;
                                }
                            }
                            #endregion
                            else
                            {
                                if (rdr.LocalName == "item") // item end element
                                {
                                    // return the first new item we find
                                    DateTime origDate;
                                    DateTime.TryParse(pubDate, out origDate);

                                    DateTime updateDate;
                                    DateTime.TryParse(lastUpdate, out updateDate);

                                    // is it newer than last poll date, and is it the oldest in this poll set?
                                    if (null != origDate && origDate > SinceDate && origDate <= lastDate)
                                    {
                                        if (!String.IsNullOrEmpty(keywords))
                                        {
                                            // check for keywords
                                            if (description.IndexOf(keywords) < 0 && title.IndexOf(keywords) < 0)
                                            {
                                                // drop item as no keyword match and keep going
                                                break;
                                            }
                                        }
                                        item = new Item()
                                        {
                                            Id = itemId,
                                            Link = link,
                                            AuthorName = authorName,
                                            Description = description,
                                            AuthorUri = authorUri,
                                            LastUpdateDate = updateDate,
                                            PubDate = origDate,
                                            Title = title
                                        };
                                        lastDate = origDate;
                                        Console.WriteLine(lastDate.ToShortDateString() + " " + lastDate.ToShortTimeString());
                                        break;
                                    }
                                    else
                                    {
                                        // keep going
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // return item
            return item;
        }
    }

    public class Item
    {
        public string Id;
        public string Link;
        public string AuthorName;
        public string AuthorUri;
        public string Title;
        public string Description;
        public DateTime PubDate;
        public DateTime LastUpdateDate;
    }
}
