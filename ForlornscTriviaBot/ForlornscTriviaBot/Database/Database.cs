using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using ForlornscTriviaBot.Entities;
using System.Threading.Tasks;
using System.Collections;

// *******************************************************************//
//                                                                    //
// This code and the class structures were written by me (Adam        //
// Stevenson)(addstevenson@hotmail.com).   You may use this code      //
// for any educational or non-profit making purpose, so long as       //
// you leave my name on it.  And please cirrext some of the typinf    //
// and spellig errirs.                                                //
//                                                                    //
// Adam Stevenson <AddStevenson@hotmail.com>                          //
//                                                                    //
//********************************************************************//

namespace ForlornscTriviaBot.Database
{
    class Database
    {
        // Web request for retrieving values from Parse Database
        private HttpWebRequest httpWebRequest;
        private String botName;
        private DatabaseParser dbParser;

        // Default constructor
        public Database()
        {
            // Initialise Parser
            dbParser = new DatabaseParser();
        }

        //
        // Return the results for a bot. The bot name will be used
        // to bring back rows from the database that relate to it. 
        //
        public BotData GetBotResults(String botName)
        {
            this.botName = botName; 

            //
            // Bot results
            //
            BotData results = new BotData();
            results.channels = GetBotChannels().ToArray();

            return results;
        }

        private List<Channel> GetBotChannels()
        {
            // Blank list of channels
            List<Channel> channelList = new List<Channel>();

            // Parametise query for: get all channels that bot is connected to.
            Dictionary<String, String> channelQueryHeaders = new Dictionary<String, String>();
            //channelQueryHeaders.Add("channelName", "Forlornsc");

            String channelUrl = "https://api.parse.com/1/classes/Channel";

            String channelMethod = "post";

            // Set the web client for this query
            //SetDatabaseQuery(channelMethod, channelUrl, channelQueryHeaders, "{channelName=Forlornsc}"); // POST
            SetDatabaseQuery(channelMethod, channelUrl, channelQueryHeaders, "");

            try
            {
                // Get the queries response
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var streamReader = new StreamReader(httpResponse.GetResponseStream());

                // Retrieve all of the channels that a bot is currently connected to.
                String databaseResponse = streamReader.ReadToEnd();
                JavaScriptSerializer serialiser = new JavaScriptSerializer();
                Dictionary<String, object> jsonDictionary = serialiser.Deserialize<Dictionary<String, object>>(databaseResponse);

                channelList = dbParser.ConvertDictionaryResults(jsonDictionary);

                //
                // How many results
                //
                for (int i = 0; i < channelList.Count; i++)
                    Console.WriteLine("Channel: " + channelList[i].channelName);
            }
            catch (WebException err)
            {
                Console.WriteLine("Web exception: " + err.Message);
            }

            return channelList;
        }

        //
        // This method populates the http web request with specific values and headers.
        // Param: method  = GET, POST, DELETE or PUT.
        // Param: url     = The url of the Parse API query. 
        // Param: headers = All of the headers required for the Parse Rest API call.  
        //
        private void SetDatabaseQuery(String method, String url, Dictionary<String, String> headers, String jsonData)
        {
            //
            // Setting up the HttpWebRequest (The default stuff)
            //
            httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Headers.Clear();
            httpWebRequest.Method = method.ToUpper();
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ContentType = "application/json";
            
            if(method.ToUpper() != "GET")
            {
                if (jsonData.Equals(""))
                {
                    Console.WriteLine("No json data supplied for " + method.ToUpper() + " query.");
                    return;
                }

                // POST, PUT or DELETE requires -d, -g etc for CURL, which is annoying.
                //
                // In order to make a CURL work, this needs to be constructed for the -d section. 
                // Basically: The structure of a curl json body needs to be written to the request stream. 
                //
                byte[] buffer = Encoding.GetEncoding("UTF-8").GetBytes("{ \"channelName\": \" " + "Forlornsc" + "\" }");
                string result = System.Convert.ToBase64String(buffer);
                Stream reqstr = httpWebRequest.GetRequestStream();
                reqstr.Write(buffer, 0, buffer.Length);
                reqstr.Close();
            }

            //
            // Add headers (first two are required for each call made to Parse database)
            //
            httpWebRequest.Headers.Add("X-Parse-Application-Id", "");
            httpWebRequest.Headers.Add("X-Parse-REST-API-Key", "");

            if (headers.Count > 0)
                //
                // Add additional non key headers, if any are specified. 
                // High chance there wouldn't be however because Parse uses CURL.
                //
                foreach (String startKey in headers.Keys)
                {
                    String stringValue = headers[startKey];

                    // Add as a HTTPWebRequest header
                    httpWebRequest.Headers.Add(startKey, stringValue);

                    // Debugging
                    Console.WriteLine("Key: " + startKey);
                    Console.WriteLine("Value: " + stringValue);
                }
        }

    }
}
