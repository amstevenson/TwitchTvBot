﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using ForlornscTriviaBot.Entities;

namespace ForlornscTriviaBot.JsonParser
{
    class JsonParser
    {
        private string _webResponseString = ""; // Used for unedited response (json) 

        public JsonParser()
        {
            // Default constructor
        }

        /// <summary>
        /// This method connects with an API and collects the response inside of a 
        /// dictionary of objects thare are identified by a string tag. This dictionary
        /// can then be used to construct objects of a specific type, or to simply 
        /// extract certain values.
        /// </summary>
        /// <param name="url"> The url to the API. The parameters must be added to the end of this url string 
        ///  in a restful manner if the method is anything but GET. </param>
        /// <param name="method"> The method: GET, POST, PUT or DELETE </param>
        /// <returns></returns>
        public Dictionary<string, object> GetWebResponseDictionary(string url, string method)
        {
            // Make sure the method is in upper case
            method = method.ToUpper();

            // Create the webclient
            var webClient = new System.Net.WebClient();

            // Create a blank dictionary
            Dictionary<string, object> returnDictionary;

            // Create the serialiser
            JavaScriptSerializer serialiser = new JavaScriptSerializer();

            // Create the string that will collect the response from the distributed api
            string returnString = "";

            // There are four selections available for the list of verbs. The GET
            // verb requires a different method of retrieving the results compared to the other three.
            try
            {
                if (method.ToUpper() == "GET")
                {
                    returnString = webClient.DownloadString(url);

                }
                else
                {
                    // The url in this context must have the query parameters added. 
                    // For example: protocol//example.com/create?name=example
                    returnString = webClient.UploadString(url, method, "");
                }
            }
            catch (WebException ex)
            {
                // One of the common ways in which this exception is triggered, is when
                // the status code for the server that (in this case the distributed API) changes to
                // a 4XX result (replace XX with numbers). 
                // For example: 405 (method not found) would not return any dictionary for the request
                // but would instead trigger this exception.
                Console.WriteLine("Web exception encountered: " + ex.Message);
            }
            catch (SystemException ex)
            {
                Console.WriteLine("System exception: " + ex.Message);
            }

            // initialise...and deserialise the return dictionary
            returnDictionary = serialiser.Deserialize<Dictionary<string, object>>(returnString);

            // Parametise the web response string that is used in the application to output
            // the pure json response for the web request
            SetWebResponseString(returnString);

            return returnDictionary;
        }

        /// <summary>
        /// This method returns a list of the chat's moderators. This is essentially treated as the means
        /// of ensuring that a user has a certain elevation level.
        /// </summary>
        /// <param name="dictionary">The deserialised dictionary that is returned from 
        /// the "GetWebResponseDictionary" method. </param>
        /// <returns></returns>
        public Chat ConvertDictionaryToResults(Dictionary<string, object> dictionary)
        {
            // Create a blank Chat object.
            Chat chat = new Chat();

            // Create a blank event array
            List<User> userList;

            // Create a blank object
            object obj;

            // For each key we have, determine what to do with the values
            // based on the type of object for each instance.
            try
            {
                foreach (String startKey in dictionary.Keys)
                {
                    // The key of the current dictionary
                    obj = dictionary[startKey];

                    if(obj is Dictionary<String, Object>)
                    {
                        // Find the chatters; we are not interested in the links or the amount of users.
                        Dictionary<String, Object> chatterObject = (Dictionary<String, Object>)obj;

                        foreach(String chatterKey in chatterObject.Keys)
                        {
                            obj = chatterObject[chatterKey];

                            // If we have found the moderators arraylist, extract the details of each
                            // and then return it as a chat object.
                            if(chatterKey.Equals("moderators"))
                            {
                                if(obj.GetType() == typeof(ArrayList))
                                {
                                    ArrayList moderatorArrayList = (ArrayList)obj;
                                    userList = new List<User>(moderatorArrayList.Count);

                                    
                                    for(int i = 0; i < moderatorArrayList.Count; i++)
                                    {
                                        String moderatorName = (String)moderatorArrayList[i];

                                        if (!moderatorName.Equals(""))
                                        {
                                            User u = new User(moderatorName);

                                            // Add the user object to the list
                                            userList.Add(u);
                                        }

                                    }

                                    // if the list is not empty, assign it to the chat object.
                                    if (userList.Count > 0)
                                    {
                                        chat.channelModerators = userList.ToArray();
                                        chat.numModerators = userList.Count;
                                    }
                                    else
                                    {
                                        chat.numModerators = 0;
                                    }

                                    return chat;
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine("Invalid cast exception: " + ex.Message);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Null reference exception: " + ex.Message);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception triggered: " + ex.Message);
            }

            return chat;
        }

        /// <summary>
        /// Setter for the json response of a download/upload string method
        /// for a WebClient request. 
        /// </summary>
        /// <param name="webResponse">The bare response for the API call</param>
        private void SetWebResponseString(string webResponse)
        {
            this._webResponseString = webResponse;
        }

        /// <summary>
        /// Getter for the json response of a download/upload string method 
        /// for a WebClient request. 
        /// </summary>
        /// <returns>The bare response for the API call</returns>
        public string GetWebResponseString()
        {
            return _webResponseString;
        }
    }
}
