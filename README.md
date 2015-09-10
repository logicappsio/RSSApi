# RSS Connector API
[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

## Deploying ##
Click the "Deploy to Azure" button above.  You can create new resources or reference existing ones (resource group, gateway, service plan, etc.)  **Site Name and Gateway must be unique URL hostnames.**  The deployment script will deploy the following:
 * Resource Group (optional)
 * Service Plan (if you don't reference exisiting one)
 * Gateway (if you don't reference existing one)
 * API App (RSSConnector)
 * API App Host (this is the site behind the api app that this github code deploys to)

## API Documentation ##
The API app has one action - GetFeedItem - which returns an Item containing the oldest feed item since the date provided. The idea is that you can call second and subsequent times with the date/time of the previously returned item to ensure you can retrieve every item you want since the original date/time specified.

Note that RSS does not provide a way to specify to the feed provider a date from which you would like historical feed items. Therefore, the Uri for the RSS you want to consume can arbitrarily return any number of items of any age and therefore you must check regularly enough that you can pick up all items should you need to. The RSS connector API takes care of feeds where the items are not returned in chronological order thereby ensuring that you will receive the items in the publication date order.

The action has three input parameters:

| Input | Description |
| ----- | ----- |
| FeedUri | The location of the RSS feed to consume |
| FromDate *(optional)* | The date/time from which a feed item is required |
| Keywords *(optional)* | A comma-seperated list of words or phrases to match against the feed item required |

###Trigger###
You can use the RSS Connector API as a trigger.  It takes FeedUri and Keywords as parameters and will trigger the logic app (and pass result) whenever a new item is found.  You set the frequency in which the polling on the specified Uri occurs. THe trigger will continue to fire until all new items have been processed.

## Example ##
| Step   | Info |
|----|----|
| Action | GetFeedItem |
| FeedUri | `https://social.msdn.microsoft.com/Forums/en-US/azurelogicapps/threads?outputAs=rss` |
| FromDate | `09/10/2015 17:22:00` |
| Keywords | `connectors` |

#### Result ####
```javascript
{
  "Id": "https://social.msdn.microsoft.com/Forums/en-US/a7e08f52-67f4-450b-b331-469d2ef782c5/issues-in-creating-orders-in-salesforce-using-logic-app?forum=azurelogicapps",
  "Link": "https://social.msdn.microsoft.com/Forums/en-US/a7e08f52-67f4-450b-b331-469d2ef782c5/issues-in-creating-orders-in-salesforce-using-logic-app?forum=azurelogicapps",
  "AuthorName": "Pooja Jagtap",
  "AuthorUri": "https://social.msdn.microsoft.com:443/profile/pooja%20jagtap/?type=forum",
  "Title": "issues in creating orders in salesforce using logic app",
  "Description": "<p>Hello All,</p>\n<p>I need to create order in salesforce by getting data from Dynamics CRM.</p>\n<p>For that i have created SQL connector which will pick newly created order in dynamics CRM database.</p>\n<p>But when i am picking salesforce connector, i am getting option for all other entities to create,or update or delete like account,contacts etc.</p>\n<p>But i am not getting option in actions list of salesforce to create Order.</p>\n<p>Please suggest what i need to do so that i will get option of creating order in Salesforce connector actions list.</p>\n<p>Thnaks in advance.</p>\n<p></p>\n<p></p>\n<p></p>\n<hr>\n<p>Pooja Jagtap Software Engineer KPIT Cummins</p>",
  "PubDate": "2015-08-17T11:04:41+00:00",
  "LastUpdateDate": "2015-08-18T11:28:16+00:00"
}
```


