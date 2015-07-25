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
using System.Xml;
using System.Data.SqlClient;
using System.Data;

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
        // The connection string, found in the webConfig file
        private static string _ConnectionString;
            //Properties.Settings.Default.BotDatabaseConnectionString;

        // Default constructor
        public Database()
        {
            _ConnectionString = "Data Source=(LocalDB)\\v11.0;AttachDbFilename=|DataDirectory|\\IRC\\BotDatabase.mdf;Integrated Security=True";
            //_ConnectionString = "Data Source=(LocalDB)\v11.0;AttachDbFilename='C:\\Users\\Adamst\\Documents\\Visual Studio 2012\\Projects\\ForlornscTriviaBot\\ForlornscTriviaBot\\IRC\\BotDatabase.mdf';Integrated Security=True";
  
                //Properties.Settings.Default.BotDatabaseConnectionString;

            //Console.WriteLine(_ConnectionString);

            /*
            String _ConnectionStringTest = "Data Source=(LocalDB)\v11.0;AttachDbFilename='C:\\Users\\Adamst\\Documents\\Visual Studio 2012\\Projects\\ForlornscTriviaBot\\ForlornscTriviaBot\\IRC\\BotDatabase.mdf';Integrated Security=True";

            using (SqlConnection conn = new SqlConnection(_ConnectionStringTest))
            {
                try
                {
                    conn.Open(); // throws if invalid

                    Console.WriteLine("connects");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Doesn't connect." + ex);
                }
            }
            */
        }

        /// <summary>
        /// Get based method for database operations; retrieving one or more values. 
        /// </summary>
        /// <param name="sqlQuery">Supplied text for database query. For example "select * from so and so"</param>
        /// <param name="type">The type of object to return as a list. This will be one of the following: channel, 
        /// botdata, command, score or triviaquestion. </param>
        /// <returns></returns>
        public dynamic GetDatabaseResults(String sqlQuery, String type)
        {
            // Create the connection to the database and the command
            SqlConnection con = new SqlConnection(_ConnectionString);

            SqlCommand cmd = new SqlCommand(sqlQuery, con);

            // Open the connection to the database
            using (con)
            {
                try
                {
                    // Attempt to open the connection 
                    con.Open();

                    // Execute the command using the "cmd" object.
                    SqlDataReader reader = cmd.ExecuteReader();

                    switch (type.ToLower())
                    {
                        case "channel":

                            // Collect a list of all channels that a bot is connected to. 
                            List<Channel> channels = new List<Channel>();

                            while (reader.Read())
                            {
                                Channel c = new Channel((int)reader["ChannelID"], (int)reader["BotID"], (String)reader["ChannelName"]);
                                channels.Add(c);
                            }

                            return channels;

                        case "botdata":

                            List<BotData> bots = new List<BotData>();

                            while(reader.Read())
                            {
                                BotData b = new BotData((int)reader["BotID"], (String)reader["BotName"]);
                                bots.Add(b);
                            }

                            return bots;

                        case "command":

                            // Collect a list of all commands for a channel, that is connected to a specific bot (gets confusing)
                            List<Command> commands = new List<Command>();

                            while (reader.Read())
                            {
                                Command c = new Command((int)reader["CommandID"], (int)reader["ChannelID"], (String)reader["CommandName"], (String)reader["CommandBody"]);
                                commands.Add(c);
                            }

                            return commands;

                        case "score":

                            return null;

                        case "triviaquestion":

                            return null;

                        default:
                            return null;
                    }
                }
                catch (Exception ex)
                {
                    // In the case of an error
                    string LastError = ex.Message;
                    Console.WriteLine(LastError);

                    // return nothing
                    return null;
                }
            }
        }

        public bool InsertRowQuery(String sqlQuery, String type)
        {
            // Create the connection to the database and the command
            SqlConnection con = new SqlConnection(_ConnectionString);

            SqlCommand cmd = new SqlCommand(sqlQuery, con);

            // Open the connection to the database
            using (con)
            {
                try
                {
                    // Attempt to open the connection 
                    con.Open();

                    // Execute the command using the "cmd" object.
                    Int32 reader = (Int32)cmd.ExecuteScalar();

                    try
                    {
                        /*
                        BotDatabaseDataSet ds = new BotDatabaseDataSet();
                        BotDatabaseDataSetTableAdapters.BotTableAdapter ba =
                            new BotDatabaseDataSetTableAdapters.BotTableAdapter();

                        ba.Fill(ds.Bot);

                        Console.WriteLine(ds.Bot[0].BotName);

                        ds.Bot[0].BotName = "awd";

                        ba.Insert("fsef");
                        ba.Update(ds);

                        BotDatabaseDataSetTableAdapters.ChannelTableAdapter ca =
                            new BotDatabaseDataSetTableAdapters.ChannelTableAdapter();

                        ca.Fill(ds.Channel);

                        ds.Channel[0].ChannelName = "tesdwd";
                        ca.Insert(1, "awdawd");
                        ca.Update(ds);

                        Console.WriteLine(ds.Bot[0].BotName);
                        */



                    }
                    catch(System.Exception ex)
                    {
                        Console.WriteLine("Update failed: " + ex);
                    }

                    switch (type.ToLower())
                    {
                        case "channel":

                            return false;

                        case "botdata":

                            return false;

                        case "bot":

                            return true;

                        case "command":

                            return false;

                        case "score":

                            return false;

                        case "triviaquestion":

                            return false;

                        default:
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    // In the case of an error
                    string LastError = ex.Message;
                    Console.WriteLine(LastError);

                    return false;
                }
            }
        }

    }
}
