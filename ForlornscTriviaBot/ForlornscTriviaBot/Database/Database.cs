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

        // Default constructor
        public Database()
        {
            _ConnectionString = "Data Source=(LocalDB)\\v11.0;AttachDbFilename=|DataDirectory|\\IRC\\BotDatabase.mdf;Integrated Security=True";
                                //Properties.Settings.Default.BotDatabaseConnectionString; // Use if above fails. 
        }

        /// <summary>
        /// Get based method for database operations; retrieving one or more values. 
        /// </summary>
        /// <param name="sqlQuery">Supplied text for database query. For example "select * from so where so = 'so'"</param>
        /// <param name="type">The type of object to return as a list. This will be one of the following: channel, 
        /// botdata, command, score or triviaquestion. </param>
        /// <returns>A list of objects</returns>
        public dynamic GetResultsQuery(String sqlQuery, String type)
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
                                Command c = new Command((int)reader["CommandID"], (int)reader["ChannelID"], (String)reader["CommandName"], (String)reader["CommandBody"],
                                    (bool)reader["CommandRepeat"], (int)reader["CommandRepeatCount"], (String)reader["CommandCreatedBy"]);
                                commands.Add(c);
                            }

                            return commands;

                        case "score":

                            // Collect a list of scorers for a channel. 
                            List<Scorer> scorers = new List<Scorer>();

                            while(reader.Read())
                            {
                                Scorer s = new Scorer((int)reader["ScorerID"], (int)reader["ChannelID"], (int)reader["ScorerValue"], (String)reader["ScorerUsername"]);
                                scorers.Add(s);
                            }

                            return scorers;

                        case "triviaquestion":

                            // Collect a list of trivia questions.
                            List<TriviaQuestion> triviaQuestions = new List<TriviaQuestion>();

                            while(reader.Read())
                            {
                                TriviaQuestion t = new TriviaQuestion((int)reader["TriviaQuestionID"], (int)reader["BotID"], (String)reader["QuestionText"], (String)reader["QuestionAnswer"]);
                                triviaQuestions.Add(t);
                            }

                            return triviaQuestions;

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

        //
        // Performs a SQLQuery for an update, delete or insert operation. 
        //
        public bool InsertUpdateDeleteRowQuery(String sqlQuery)
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
                    cmd.ExecuteScalar();

                    // If no exception is raised, we can assume the insert query performed correctly. 
                    return true;
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

        public bool DoesRowExist(String sqlQuery)
        {
            // Create the connection to the database and the command
            SqlConnection con = new SqlConnection(Properties.Settings.Default.BotDatabaseConnectionString);

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

                    // If the bot exists
                    if (reader.HasRows)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    // In the case of an error
                    string LastError = ex.Message;
                    Console.WriteLine(LastError);

                    // return error/not found
                    return false;
                }
            }
        }

    }
}
