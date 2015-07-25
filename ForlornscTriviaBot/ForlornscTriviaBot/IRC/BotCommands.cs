using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ForlornscTriviaBot.Entities;
using ForlornscTriviaBot.Database;
using System.Data.SqlClient;

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

namespace ForlornscTriviaBot.IRC
{
    //
    // Used in conjunction with the database.
    // Each IRC object created will have one of these for the purpose
    // of CRUD operations. 
    //
    class BotCommands
    {
        // Class variables
        private String botName;

        // Database for retrieving values from Parse
        private Database.Database db;

        public BotCommands(String botName)
        {
            // Default constructor
            this.botName = botName;
            this.db = new Database.Database();
        }

        //
        // Keep a database record of a bots new entry
        // into another channel.
        //
        public void JoinChannel(String channelName)
        {

        }

        //
        // Remove the database record pertaining to a specific channel
        // that the bot is currently in.
        //
        public void LeaveChannel(String channelName)
        {

        }

        //
        // Get all channels attributed to a bot
        //
        public List<Channel> GetChannels(String botName)
        {
            List<Channel> channels = db.GetDatabaseResults("SELECT * FROM Channel " + 
                                                           "JOIN Bot ON Channel.BotID = Bot.BotID WHERE BotName = '" + botName + "';"
                                                            , "channel");
            
            return channels;
        }

        //
        // Add a new string command. 
        // Others will probably have different formats, such as shuffle etc
        // but giving people the ability to add string commands is not too bad for now.
        // Note to self however that this should only be given to moderators, else things
        // can get a little bit tricky or complicated if people decide to spam it. 
        //
        public void AddCommand(String channelName)
        {
            
        }

        //
        // Search for the commands for a channel related to a specific bot. 
        // This will normally be called when the user uses !cmds in the chat. 
        // I am not quite sure where else this would be used for now.
        //
        public List<Command> GetCommands(String channelName)
        {
            List<Command> commands = db.GetDatabaseResults("SELECT * FROM Command", "command");

            return commands;
        }

        //
        // Delete a string command for a channel. 
        //
        public void DeleteCommand(String channelName)
        {

        }

        //
        // Add a new trivia question for a channel.
        // Obviously, the answer will need to be set here too, alongside
        // the question body etc. There could additionally be a points value
        // attributed to it in the future. For example, harder questions awarded more
        // than easier ones. Not for now however...
        //
        public void AddTriviaQuestion(String channelName)
        {

        }

        //
        // Delete a trivia question belonging to a channel. 
        //
        public void DeleteTriviaQuestion(String channelname)
        {

        }

        //
        // Keeping track of scores. This method could probably be named better.
        // However I know what this is for, so that's all that matters for now.
        // Add a viewer to a channel if it is not already there, update score
        // values where necessary. 
        //
        public void AddChannelViewer(String channelName)
        {

        }

        //
        // Delete a naughty channel viewer from the viewers table (which is effectively the score I guess).
        //
        public void DeleteChannelViewer(String channelName)
        {

        }

        //
        // Delete all viewers from a channel
        //
        public void DeleteAllChannelViewers(String channelName)
        {

        }

        //
        // Get all results for the bot.
        // This will be called when the bot is created and references most of the above get methods. 
        //
        public BotData GetBotResults(String botName)
        {
            List<BotData> bd = db.GetDatabaseResults("SELECT * FROM Bot WHERE BotName = '" + botName + "';", "botdata");
            List<Channel> channels = db.GetDatabaseResults("SELECT * FROM Channel WHERE BotID = '" + bd[0].objectId + "';", "channel");
            bd[0].channels = channels.ToArray();

            // Print
            /*
            if (bd.Count > 0)
                for (int i = 0; i < bd.Count; i++)
                {
                    Console.WriteLine("Bot " + bd[i].objectId + ": Name = " + bd[i].BotName);
                }

            // For channels
            if (channels.Count > 0)
                for (int i = 0; i < channels.Count; i++)
                {
                    Console.WriteLine("Channel " + channels[i].objectId + ": BotID = " + channels[i].botID + " Name = " + channels[i].channelName);
                }

             */

            return bd[0];
        }

        //
        // Check to see if the bot exists, if it doesn't then we will add it to the database
        // afterwards to be used as a foreign key. 
        //
        public bool DoesBotExist(String botName)
        {
            // Create the connection to the database and the command
            SqlConnection con = new SqlConnection(Properties.Settings.Default.BotDatabaseConnectionString);

            SqlCommand cmd = new SqlCommand("SELECT * FROM Bot WHERE BotName = '" + botName + "';", con);

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

        //
        // If a new bot is used and does not exist in the database, this
        // method will be used to add it.
        //
        public bool AddNewBot(String botName)
        {
            String query = "INSERT INTO Bot (BotName) " +
                           "VALUES ('" + botName + "');" +
                           "SELECT CAST(Scope_Identity() AS int)";

            //Console.WriteLine(query);

            // Add new bot entry to the database
            if (db.InsertRowQuery(query, "bot"))
            {
                Console.WriteLine("Bot added = true");
                return true;
            }
            else
            {
                Console.WriteLine("Bot added = false");
                return false;
            }
        }

        //
        // Testing purposes only. Writing results to a file. 
        //
        public void PrintAllTestData()
        {
            List<BotData> bd = db.GetDatabaseResults("SELECT * FROM Bot", "botdata");
            List<Channel> channels = db.GetDatabaseResults("SELECT * FROM Channel", "channel");
            List<Command> commands = db.GetDatabaseResults("SELECT * FROM Command", "command");

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"DatabaseInformation.txt"))
            {
                // For bots
                if (bd.Count > 0)
                    for (int i = 0; i < bd.Count; i++)
                    {
                        file.WriteLine("Bot " + bd[i].objectId + ": Name = " + bd[i].BotName);
                    }

                file.WriteLine("");
                file.WriteLine("------------------------------------------------------------------------------------");
                file.WriteLine("");

                // For channels
                if (channels.Count > 0)
                    for (int i = 0; i < channels.Count; i++)
                    {
                        file.WriteLine("Channel " + channels[i].objectId + ": BotID = " + channels[i].botID + " Name = " + channels[i].channelName);
                    }

                file.WriteLine("");
                file.WriteLine("------------------------------------------------------------------------------------");
                file.WriteLine("");

                // For commands
                if (commands.Count > 0)
                    for (int i = 0; i < commands.Count; i++)
                    {
                        file.WriteLine("commands " + commands[i].objectId + ": Name = " + commands[i].commandName);
                    }

                file.WriteLine("");
                file.WriteLine("------------------------------------------------------------------------------------");
                file.WriteLine("");
            }
        }
    }
}
