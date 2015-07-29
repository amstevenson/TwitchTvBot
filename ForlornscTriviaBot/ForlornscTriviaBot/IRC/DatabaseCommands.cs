using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ForlornscTriviaBot.Entities;
using ForlornscTriviaBot.Database;

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
    class DatabaseCommands
    {
        // Class variables
        private String botName;

        // Database for retrieving values from Parse
        private Database.Database db;

        public DatabaseCommands(String botName)
        {
            // Default constructor
            this.botName = botName;
            this.db = new Database.Database();
        }

        //
        // Keep a database record of a bots new entry
        // into another channel.
        //
        public bool AddChannel(int botID, String channelName)
        {
            String query = "INSERT INTO Channel (BotID, ChannelName) " +
               "VALUES (' " + botID + "', '" + channelName + "');" +
               "SELECT CAST(Scope_Identity() AS int)";

            // Add new Channel entry to the database
            if (db.InsertUpdateDeleteRowQuery(query))
            {
                Console.WriteLine("Channel added = true");
                return true;
            }
            else
            {
                Console.WriteLine("Channel added = false");
                return false;
            }
        }

        //
        // Remove the database record pertaining to a specific channel
        // that the bot is currently in.
        //
        public bool DeleteChannel(int channelID, int botID)
        {
            String query = "DELETE FROM Channel " +
                            "WHERE ChannelID = '" + channelID + "' " + 
                            "AND BotID = '" + botID + "' ";

            // Delete a specific channel from the database
            if (db.InsertUpdateDeleteRowQuery(query))
            {
                Console.WriteLine("Deleting Channel, success = true");

                // Deleting other database rows related to this operation. 
                DeleteAllCommands(channelID);

                // Delete all scorers related to this operation. 
                DeleteAllChannelViewers(channelID);

                return true;
            }
            else
            {
                Console.WriteLine("Deleting Channel, success = false");
                return false;
            }
        }

        //
        // Get all channels attributed to a bot
        //
        public List<Channel> GetChannels()
        {
            List<Channel> channels = db.GetResultsQuery("SELECT * FROM Channel " + 
                                                           "JOIN Bot ON Channel.BotID = Bot.BotID WHERE BotName = '" + this.botName + "';"
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
        public bool AddCommand(int channelID, String commandName, String commandBody)
        {
            String query = "INSERT INTO Command (ChannelID, CommandName, CommandBody) " +
                           "VALUES (' " + channelID + "', '" + commandName + "', '" + commandBody + "' );" +
                           "SELECT CAST(Scope_Identity() AS int)";

            // Add new Command entry to the database
            if (db.InsertUpdateDeleteRowQuery(query))
            {
                Console.WriteLine("Command added = true");
                return true;
            }
            else
            {
                Console.WriteLine("Command added = false");
                return false;
            }
        }

        //
        // Search for the commands for a channel related to a specific bot. 
        // This will normally be called when the user uses !cmds in the chat. 
        // I am not quite sure where else this would be used for now.
        //
        public List<Command> GetCommands(int channelID)
        {
            List<Command> commands = db.GetResultsQuery("SELECT * FROM Command WHERE ChannelID = '" + channelID + "'", "command");
            return commands;
        }

        //
        // Delete a string command for a channel. 
        //
        public bool DeleteCommand(int commandID)
        {
            String query = "DELETE FROM Command " +
                            "WHERE CommandID = '" + commandID + "'";

            // Delete a specific command
            if (db.InsertUpdateDeleteRowQuery(query))
            {
                Console.WriteLine("Deleting command, success = true");
                return true;
            }
            else
            {
                Console.WriteLine("Deleting command, success = false");
                return false;
            }
        }

        //
        // Delete all commands related to a channel
        //
        public bool DeleteAllCommands(int channelID)
        {
            String query = "DELETE FROM Command " +
                            "WHERE ChannelID = '" + channelID + "'";

            // Delete a specific command
            if (db.InsertUpdateDeleteRowQuery(query))
            {
                Console.WriteLine("Deleting all commands, success = true");
                return true;
            }
            else
            {
                Console.WriteLine("Deleting all command, success = false");
                return false;
            }
        }

        //
        // Get all trivia questions related to a specific bot. This may be changed in the future
        // so that the questions can be attributed to an individual channel instead, however, chances
        // are that the change would not be the right one.
        //
        public List<TriviaQuestion> GetTriviaQuestions(int botID)
        {
            List<TriviaQuestion> questions = db.GetResultsQuery("SELECT * FROM TriviaQuestion " 
                                                              + "WHERE BotID = '" + botID + "'"
                                                              , "triviaquestion");
            return questions;
        }

        //
        // Add a new trivia question for a channel.
        // Obviously, the answer will need to be set here too, alongside
        // the question body etc. There could additionally be a points value
        // attributed to it in the future. For example, harder questions awarded more
        // than easier ones. Not for now however...
        //
        public bool AddTriviaQuestion(int botID, String questionText, String questionAnswer)
        {
            String query = "INSERT INTO TriviaQuestion (BotID, QuestionText, QuestionAnswer) " +
                           "VALUES (' " + botID + "', '" + questionText + "', '" + questionAnswer + "' );" +
                           "SELECT CAST(Scope_Identity() AS int)";

            // Add new TriviaQuestion entry to the database
            if (db.InsertUpdateDeleteRowQuery(query))
            {
                Console.WriteLine("TriviaQuestion added = true");
                return true;
            }
            else
            {
                Console.WriteLine("TriviaQuestion added = false");
                return false;
            }
        }

        //
        // Delete a trivia question belonging to a channel. 
        //
        public bool DeleteTriviaQuestion(int triviaQuestionID)
        {
            String query = "DELETE FROM TriviaQuestion " +
                            "WHERE TriviaQuestionID = '" + triviaQuestionID + "'";

            // Delete a trivia question
            if (db.InsertUpdateDeleteRowQuery(query))
            {
                Console.WriteLine("trivia question deleted = true");
                return true;
            }
            else
            {
                Console.WriteLine("trivia question deleted = false");
                return false;
            }
        }

        //
        // Get channel scorers related to the channel for a specific bot. 
        //
        public List<Scorer> GetChannelViewers(int channelID)
        {
            List<Scorer> scorers = db.GetResultsQuery("SELECT * FROM Scorer "
                                                    + "WHERE ChannelID = '" + channelID + "'"
                                                    , "score");
            return scorers;
        }

        //
        // Add a viewer to a channel if it is not already there, update score
        // values where necessary by using other method. 
        //
        public bool AddChannelViewer(int channelID, int scoreValue, String scorerUsername)
        {
            String query = "INSERT INTO Scorer (ChannelID, ScorerValue, ScorerUsername) " +
                           "VALUES (' " + channelID + "', '" + scoreValue + "', '" + scorerUsername + "' );" +
                           "SELECT CAST(Scope_Identity() AS int)";

            // Add new viewer/scorer entry to the database
            if (db.InsertUpdateDeleteRowQuery(query))
            {
                Console.WriteLine("Scorer/viewer added = true");
                return true;
            }
            else
            {
                Console.WriteLine("Scorer/viewer added = false");
                return false;
            }
        }

        //
        // Award a scorer for answering a question correctly.  
        //
        public bool ChangeChannelViewerScore(int scorerID, int awardValue)
        {
            // Increment the score for a viewer. 
            String query = "UPDATE Scorer " +
                "SET ScorerValue = ScorerValue + '" + awardValue + "' " +
                "WHERE ScorerID = '" + scorerID + "' ";

            // Increment the score by a specific value. 
            if (db.InsertUpdateDeleteRowQuery(query))
            {
                Console.WriteLine("Score updated = true");
                return true;
            }
            else
            {
                Console.WriteLine("Score updated = false");
                return false;
            }
        }

        //
        // A setter for the score of a viewer. Probably not a good idea to include, so will only
        // reference this if there is a need for it in the future. 
        //
        // Differs from above, in that its not incrementing, but setting a specific value to a table row. 
        //
        public bool SetChannelViewerScore(int scorerID, int scoreValue)
        {
            // Set the scorevalue for a scorer
            String query = "UPDATE Scorer " +
                "SET ScorerValue = '" + scoreValue + "' " +
                "WHERE ScorerID = '" + scorerID + "' ";

            // Increment the score by a specific value. 
            if (db.InsertUpdateDeleteRowQuery(query))
            {
                Console.WriteLine("Score updated = true");
                return true;
            }
            else
            {
                Console.WriteLine("Score updated = false");
                return false;
            }
        }

        //
        // Delete a naughty channel viewer from the scorer table.
        //
        public bool DeleteChannelViewer(int scorerID)
        {
            String query = "DELETE FROM Scorer " +
                "WHERE ScorerID = '" + scorerID + "'";

            // Delete viewer from the database 
            if (db.InsertUpdateDeleteRowQuery(query))
            {
                Console.WriteLine("Scorer/viewer deleted = true");
                return true;
            }
            else
            {
                Console.WriteLine("Scorer/viewer added = false");
                return false;
            }
        }

        //
        // Delete all viewers from a channel
        //
        public bool DeleteAllChannelViewers(int channelID)
        {
            String query = "DELETE FROM Scorer " +
                "WHERE ChannelID = '" + channelID + "'";

            // Delete all of the viewers that relate to a specific channel.
            if (db.InsertUpdateDeleteRowQuery(query))
            {
                Console.WriteLine("Delete all channel viewers, success = true");
                return true;
            }
            else
            {
                Console.WriteLine("Delete all channel viewers, success= false");
                return false;
            }
        }

        //
        // Get all results for the bot.
        // This will be called when the bot is created and references most of the above get methods. 
        //
        public BotData GetBotResults()
        {
            // Getting all of the bot information.
            List<BotData> bd = db.GetResultsQuery("SELECT * FROM Bot WHERE BotName = '" + this.botName + "';", "botdata");

            // Create empty botdata. 
            BotData botData = new BotData();

            if (bd.Count > 0)
            {
                // Will only ever have one bot per name. Since Twitch doesn't allow for multiple usernames...kind of obvious I guess. 
                // Therefore reference first index. 
                botData = bd[0];

                // Get all of the channels
                List<Channel> channels = db.GetResultsQuery("SELECT * FROM Channel WHERE BotID = '" + botData.objectId + "';", "channel");
                botData.channels = channels.ToArray();

                // Once we have the channels, find the commands and scorers for each channel.
                if (channels.Count > 0)
                    for (int i = 0; i < channels.Count; i++)
                    {
                        // Get the ChannelID
                        int channelID = botData.channels[i].objectId;

                        // Get the commands attributed to the specific channel. 
                        List<Command> channelCommands = GetCommands(channelID);

                        // Allocate commands to bot
                        if (channelCommands.Count > 0)
                            botData.channels[i].channelCommands = channelCommands.ToArray();
                        else
                        {
                            botData.channels[i].channelCommands = new Command[0];
                            Console.WriteLine("Bot Results: No Commands for channel");
                        }

                        // Next get the scorers for each channel attributed to the bot
                        List<Scorer> channelScorers = GetChannelViewers(channelID);

                        if (channelScorers.Count > 0)
                            botData.channels[i].scorers = channelScorers.ToArray();
                        else
                        {
                            botData.channels[i].scorers = new Scorer[0];
                            Console.WriteLine("Bot results: No scorers for channel");
                        }
                    }
                else
                {
                    botData.channels = new Channel[0];
                    Console.WriteLine("Bot Results: No channels attributed to Bot");
                }

                // After commands and scorers, get the questions for the bot.
                List<TriviaQuestion> triviaQuestions = GetTriviaQuestions(botData.objectId);

                if (triviaQuestions.Count > 0)
                    botData.triviaQuestions = triviaQuestions.ToArray();
                else
                {
                    botData.triviaQuestions = new TriviaQuestion[0];
                    Console.WriteLine("Bot Results: No trivia questions are available for this bot. ");
                }
            }
            else 
                Console.WriteLine("Bot Results: No bots retrieved matching database record. ");

            // Return the BotData information. In the case where no bots match the name, which would be odd,
            // an empty one will be returned. 
            return botData;
        }

        //
        // Check to see if a channel exists. This is used during the bots default constructor to determine if
        // the bot is already connected to a specific channel.
        //
        public bool DoesChannelExist(int botID, String channelName)
        {
            String query = "SELECT * FROM Channel "
                + "WHERE BotID = '" + botID + "' " 
                + "AND ChannelName = '" + channelName + "'";
            if(db.DoesRowExist(query))
            {
                Console.WriteLine("Channel exists = true");
                return true;
            }
            else
            {
                Console.WriteLine("Channel exists = false");
                return false;
            }
        }

        //
        // Check to see if the trivia question exists within the database. 
        //
        public bool DoesTriviaQuestionExist(int botID, String triviaQuestion)
        {
            String query = "SELECT * FROM TriviaQuestion "
                + "WHERE BotID = '" + botID + "' "
                + "AND QuestionText = '" + triviaQuestion + "'";

            if (db.DoesRowExist(query))
            {
                Console.WriteLine("Trivia question exists = true");
                return true;
            }
            else
            {
                Console.WriteLine("Trivia question exists = false");
                return false;
            }
        }

        //
        // Check to see if a command already exists in the database.
        //
        public bool DoesCommandExist(int channelID, String commandName)
        {
            String query = "SELECT * FROM Command "
                + "WHERE ChannelID = '" + channelID + "' "
                + "AND CommandName = '" + commandName + "'";
            if (db.DoesRowExist(query))
            {
                Console.WriteLine("Channel exists = true");
                return true;
            }
            else
            {
                Console.WriteLine("Channel exists = false");
                return false;
            }
        }

        //
        // Check to see if the bot exists, if it doesn't then we will add it to the database
        // afterwards to be used as a foreign key. 
        //
        public bool DoesBotExist()
        {
            String query = "SELECT * FROM Bot WHERE BotName = '" + this.botName + "'";
            if (db.DoesRowExist(query))
            {
                Console.WriteLine("Bot exists = true");
                return true;
            }
            else
            {
                Console.WriteLine("Bot exists = false");
                return false;
            }
        }

        //
        // If a new bot is used and does not exist in the database, this
        // method will be used to add it.
        //
        public bool AddNewBot()
        {
            String query = "INSERT INTO Bot (BotName) " +
                           "VALUES ('" + this.botName + "');" +
                           "SELECT CAST(Scope_Identity() AS int)";

            // Add new bot entry to the database
            if (db.InsertUpdateDeleteRowQuery(query))
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
        // Testing purposes only. Print BotData out to Console. 
        //
        public void PrintBotData(BotData botData)
        {
            // Bot
            Console.WriteLine("Bot " + botData.objectId + ": Name = " + botData.BotName);

            // Channels
            for (int i = 0; i < botData.channels.Length; i++)
            {
                Console.WriteLine("Channel " + botData.channels[i].objectId + ": name = " + botData.channels[i].channelName);

                // Commands
                int amountOfCommands = botData.channels[i].channelCommands.Length;

                if(amountOfCommands > 0)
                    for (int j = 0; j < amountOfCommands; j++)
                        Console.WriteLine("Command " + botData.channels[i].channelCommands[j].objectId + 
                            ": Name = " + botData.channels[i].channelCommands[j].commandName + 
                            " Body = " + botData.channels[i].channelCommands[j].commandBody);
                
                // Scorers/viewers
                int amountOfScorers = botData.channels[i].scorers.Length;

                if (amountOfScorers > 0)
                    for (int j = 0; j < amountOfScorers; j++)
                        Console.WriteLine("Scorer " + botData.channels[i].scorers[j].objectId + 
                            ": Username = " + botData.channels[i].scorers[j].scorerUsername + 
                            " Score = " + botData.channels[i].scorers[j].scorerValue);
            }

            // Trivia Questions
            int amountOfQuestions = botData.triviaQuestions.Length;

            if (amountOfQuestions > 0)
                for (int i = 0; i < amountOfQuestions; i++)
                    Console.WriteLine("TriviaQuestion " + botData.triviaQuestions[i].objectId + 
                        ": body = " + botData.triviaQuestions[i].questionText + " Answer = " + 
                        botData.triviaQuestions[i].questionAnswer);
        }

        //
        // Testing purposes only. Write Database rows out to text file. For quick feedback.
        //
        public void PrintAllTestData()
        {
            List<BotData> bd = db.GetResultsQuery("SELECT * FROM Bot", "botdata");
            List<Channel> channels = db.GetResultsQuery("SELECT * FROM Channel", "channel");
            List<Command> commands = db.GetResultsQuery("SELECT * FROM Command", "command");
            List<Scorer> scorers = db.GetResultsQuery("SELECT * FROM Scorer", "scorer");
            List<TriviaQuestion> triviaQuestions = db.GetResultsQuery("SELECT * FROM TriviaQuestion", "triviaquestion");

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"DatabaseInformation.txt"))
            {
                try
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

                    // For scorers
                    if (scorers.Count > 0)
                        for (int i = 0; i < scorers.Count; i++)
                        {
                            file.WriteLine("scorers " + scorers[i].objectId + ": Username = " + scorers[i].scorerUsername +
                                " score = " + scorers[i].scorerValue);
                        }

                    file.WriteLine("");
                    file.WriteLine("------------------------------------------------------------------------------------");
                    file.WriteLine("");

                    // For questions
                    if (triviaQuestions.Count > 0)
                        for (int i = 0; i < triviaQuestions.Count; i++)
                        {
                            file.WriteLine("triviaQuestions " + triviaQuestions[i].objectId + ": Text = " + triviaQuestions[i].questionText +
                                " Answer = " + triviaQuestions[i].questionAnswer);
                        }

                    file.WriteLine("");
                    file.WriteLine("------------------------------------------------------------------------------------");
                    file.WriteLine("");

                } catch(NullReferenceException ex)
                {
                    Console.WriteLine("Exception in printing out results: " + ex);
                }
            }
        }
    }
}
