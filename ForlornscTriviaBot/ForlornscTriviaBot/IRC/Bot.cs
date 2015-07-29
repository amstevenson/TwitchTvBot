﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ForlornscTriviaBot.Entities;

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
    class Bot
    {
        // Form for calling methods
        private TriviaBot mainForm;

        // Client variables
        private TcpClient client;
        private NetworkStream nwStream;
        private StreamReader reader;
        private StreamWriter writer;

        private String server = "", botName = "", channel = "", password = "";
        private int port = 6667;
        private bool threadActive;

        // Database Variables
        private DatabaseCommands databaseCommands;
        private ChatCommands chatCommands;
        private BotData botData;
        private int channelID;

        // Listen thread
        Thread listen;

        public Bot()
        { 
            // Default constructor
        }

        public Bot(String server, int port, String botName, String channel, String password, TriviaBot mainForm)
        {
            // Assign values to global variables
            this.server = server;
            this.port = port;
            this.botName = botName;
            this.channel = channel;
            this.password = password;
            this.mainForm = mainForm;
            this.databaseCommands = new DatabaseCommands(botName); // Assigns the bot name in the BotCommands class; used for CRUD operations.
            this.chatCommands = new ChatCommands(channel, this, databaseCommands); // Create an object of a class that will be used for the chat commands. For example "!cmds".
            this.threadActive = true;

            // Check to see if the bot is recorded in the database or not.
            // if it is not in the database, add it.
            if (!databaseCommands.DoesBotExist())
                databaseCommands.AddNewBot();

            // Retrieve Database related values specific to the bot.
            this.botData = databaseCommands.GetBotResults();

            // Check to see if the bot is already in the channel that is entered in the GUI
            // If it doesnt exist in the database, then add it. 
            if (!databaseCommands.DoesChannelExist(botData.objectId, channel))
                databaseCommands.AddChannel(botData.objectId, channel);

            // Get the identifer for the channel to be used in the listen thread. 
            if(botData.channels.Length > 0)
            for (int i = 0; i < botData.channels.Length; i++)
            {
                if (botData.channels[i].channelName.Equals(channel))
                    channelID = i;
            }

            // Open the connection to Twitch IRC
            client = new TcpClient("irc.twitch.tv", port);
            nwStream = client.GetStream();
            reader = new StreamReader(nwStream, Encoding.GetEncoding("iso8859-1"));
            writer = new StreamWriter(nwStream, Encoding.GetEncoding("iso8859-1"));

            // Start a thread that reads all data from IRC
            listen = new Thread(new ThreadStart(Listen));
            listen.Start();

            // Login to the service by providing credentials
            SendMessage("PASS " + password);
            SendMessage("NICK " + botName);
            SendMessage("JOIN #" + channel.ToLower());

            // Test for printing all values.
            databaseCommands.PrintAllTestData();

            // Print out the botData for testing purposes.
            databaseCommands.PrintBotData(botData);

        }

        // Read all information from the IRC server - when a message comes through
        // this interprets the response and appends it to the chat. 
        private void Listen()
        {
            try
            {
                while (threadActive)
                {
                    String data = "";

                    while ((data = reader.ReadLine()) != null)
                    {
                        // String array to contain strings, split upon certain criteria
                        String[] pingCheck;

                        // Write out info to the GUI
                        mainForm.SetText(data);

                        // If we have PING, reply with PONG
                        // This needs to be here, else IRC will kick the bot out of the channel.
                        pingCheck = data.Split(new char[] { ' ' }, 5);
                        if (pingCheck[0] == "PING")
                        {
                            // pingCheck[1] is the server.
                            SendMessage("PONG " + pingCheck[1]);
                            Console.WriteLine("PONG " + pingCheck[1]);
                        }

                        // If we have a normal message, gather the contents and determine what to do with it.
                        String[] normalMessage;

                        //
                        // For chat related comments/commands.
                        //
                        normalMessage = data.Split(new char[] { ' ' }, 4);
                        if (normalMessage.Length > 3)
                        {
                            //
                            // Order:
                            // 0: Username, server etc. 
                            // 1: Type (PRIVMSG, JOIN, PART ETC)
                            // 2: channel
                            // 3: message
                            //
                            String serverAndUsername = normalMessage[0];
                            String type = normalMessage[1];
                            String channel = normalMessage[2] = normalMessage[2].Replace("#", "");
                            String message = normalMessage[3] = normalMessage[3].Replace(":", "");

                            int commandCount = botData.channels[channelID].channelCommands.Length;

                            // PRIVMSG refers to a message by a user. 
                            if (type == "PRIVMSG")
                            {
                                // Extract strings. 
                                String firstWord = message.Split(new char[] {' '}, 2)[0];
                                String username = serverAndUsername.Split(new char[] { '!' }, 2)[0].Replace(":", "");

                                // Check if the starting message features a command. 
                                for(int i = 0; i < commandCount; i++)
                                {
                                    if(firstWord.Equals(botData.channels[channelID].channelCommands[i].commandName))
                                    {
                                        String commandMessage = "PRIVMSG #" + channel + " : " + 
                                            botData.channels[channelID].channelCommands[i].commandBody;

                                        SendMessage(commandMessage);
                                    }
                                }

                                // Check if the starting message is the answer to a trivia question
                                // TODO STILL
                                //
                                //

                                //
                                // Chat Commands
                                //
                                switch(firstWord.ToLower())
                                {
                                    // Print out the help for the chat, to give a summary of commands. 
                                    case "!help":

                                        break;

                                    // Print out a list of commands.
                                    case "!cmds":

                                        chatCommands.AllCommands(botData.channels[channelID].channelCommands, commandCount, channelID);

                                        break;

                                    // Add a new command.
                                    case "!addcommand":

                                        chatCommands.AddCommand(message, botData, commandCount, channelID);

                                        break;
                                    
                                    // Delete a command
                                    case "!deletecommand":

                                        chatCommands.DeleteCommand(message, botData, channelID);

                                        break;

                                    // Start the trivia
                                    case "!starttrivia":

                                        // Toggle state and start trivia
                                        botData.triviaActive = true;
                                        botData.triviaTimePQuestion = 15.00f;
                                        chatCommands.SetTriviaQuestion(botData);

                                        break;

                                    // Add a new trivia question
                                    case "!addtriviaquestion":

                                        chatCommands.AddTriviaQuestion(message, botData);

                                        break;

                                    // Delete a trivia question
                                    case "!deletetriviaquestion":

                                        break;

                                    // Stop the trivia
                                    case "!stoptrivia":

                                        // Toggle state and stop trivia
                                        botData.triviaActive = false;

                                        break;

                                    // Set the timeout for the trivia questions.
                                    // Determines how long people have to answer.
                                    case "!settimeperquestion":

                                        break;

                                    // Change the topic of the trivia. 
                                    // For example, history, gaming etc. 
                                    case "!changetriviatopic":

                                        break;

                                    // Do nothing by default. 
                                    default: 
                                        break;
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error triggered: " + e.Message);
            }

            // If not active, abort. 
            if (!threadActive)
                listen.Abort();
        }

        //
        // Make the bot leave the channel from an IRC point of view.
        // Independant from the database.
        //
        public void LeaveChannel()
        {
            // Make the bot leave the current channel
            writer.WriteLine("PART #" + channel.ToLower());
            writer.Flush();
        }

        //
        // Write a message to the channel the bot is currently in.
        //
        public void SendMessage(string message)
        {
            writer.WriteLine(message + "\r\n");
            writer.Flush();
        }

        //
        // Make sure we close the thread down from above upon exiting. 
        // Else we will have memory leaks and an application that will
        // not fully close.
        //
        public void DisposeThread()
        {
            listen.Abort();
            threadActive = false;
        }

    }
}
