using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ForlornscTriviaBot.Entities;
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
    class DatabaseParser
    {
        public DatabaseParser()
        {
            // Default constructor
        }

        /// <summary>
        /// This method creates and returns an instance of a 'Results' object based on a 
        /// deserialised dictionary of strings and objects. 
        /// </summary>
        /// <param name="dictionary">The deserialised dictionary that is returned from 
        /// the "GetWebResponseDictionary" method. </param>
        /// <returns></returns>
        public List<Channel> ConvertDictionaryResults(Dictionary<String, object> dictionary)
        {
            // Create a blank results object
            List<Channel> channels = new List<Channel>();

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

                    if (obj is ArrayList)
                    {
                        // We have the ArrayList of events
                        ArrayList channelsArrayList = (ArrayList)obj;

                        // In the scope of the results collected from the 
                        // distributed API. The events are split up into
                        // individual dictionaries of <string, string>
                        for (int i = 0; i < channelsArrayList.Count; i++)
                        {
                            // Get the specific dictionary with the event information inside of it
                            Dictionary<String, object> channelDictionary = (Dictionary<String, object>)channelsArrayList[i];

                            // Create a blank channel object
                            Channel c = new Channel("Unknown", "Unknown", "Unknown", "Unknown");

                            // For each key we have for this dictionary, retrieve the key name and the corresponding value
                            foreach (String channelKey in channelDictionary.Keys)
                            {
                                // Store the corresponding value for the key
                                String strKeyValue = (String)channelDictionary[channelKey];

                                switch (channelKey)
                                {
                                    case "channelName": c.channelName = strKeyValue; break;
                                    case "createdAt": c.createdAt = strKeyValue; break;
                                    case "objectId": c.objectId = strKeyValue; break;
                                    case "updatedAt": c.updatedAt = strKeyValue; break;

                                    default: break;
                                }

                            }

                            // Add the new event to the list
                            channels.Add(c);
                        }
                    }
                    else if (obj is String)
                    {
                        // We have either the success or message string
                        String strKeyValue = (String)obj;

                        switch (startKey)
                        {
                            case "message": Console.WriteLine("Retrieve channels message:" + strKeyValue); break;

                            case "status": Console.WriteLine("Retrieve channels status:" + strKeyValue); break;

                            // Do nothing
                            default: break;
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

            return channels;
        }
    }
}
