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
        private TriviaBot _mainForm;
        
        // Client variables
        private TcpClient _client;
        private NetworkStream _nwStream;
        private StreamReader _reader;
        private StreamWriter _writer;

        private String _server = "", _botName = "", _channel = "", _password = "";
        private int _port = 6667;
        private int _messageCount = 0;
        private bool _threadData, _threadTrivia, _threadUsers;

        // Threading/processor variables
        private Thread _checkData;
        private Thread _checkTrivia;
        private Thread _checkUsers;

        private ThreadProcessor _triviaProcessor;
        private ThreadProcessor _userProcessor;

        // Database Variables
        private DatabaseCommands _databaseCommands;
        private ChatCommands _chatCommands;
        private BotData _botData;
        private int _channelID;

        public Bot(String server, int port, String botName, String channel, String password, TriviaBot mainForm)
        {
            // Assign values to global variables
            this._server = server;
            this._port = port;
            this._botName = botName;
            this._channel = channel;
            this._password = password;
            this._mainForm = mainForm;
            this._databaseCommands = new DatabaseCommands(botName); // Assigns the bot name in the BotCommands class; used for CRUD operations.
            this._chatCommands = new ChatCommands(channel, this, _databaseCommands); // Create an object of a class that will be used for the chat commands. For example "!cmds".
            this._threadData = true;
            this._threadTrivia = true;
            this._threadUsers = true;

            // Check to see if the bot is recorded in the database or not.
            // if it is not in the database, add it.
            if (!_databaseCommands.DoesBotExist())
                _databaseCommands.AddNewBot();

            // Retrieve Database related values specific to the bot.
            this._botData = _databaseCommands.GetBotResults();

            // Check to see if the bot is already in the channel that is entered in the GUI
            // If it doesnt exist in the database, then add it. 
            if (!_databaseCommands.DoesChannelExist(_botData.objectId, channel))
                _databaseCommands.AddChannel(_botData.objectId, channel);

            // Get the identifer for the channel to be used in the listen thread. 
            if (_botData.channels.Length > 0)
                for (int i = 0; i < _botData.channels.Length; i++)
            {
                if (_botData.channels[i].channelName.Equals(channel))
                    _channelID = i;
            }

            // Open the connection to Twitch IRC
            _client = new TcpClient("irc.twitch.tv", port);
            _nwStream = _client.GetStream();
            _reader = new StreamReader(_nwStream, Encoding.GetEncoding("iso8859-1"));
            _writer = new StreamWriter(_nwStream, Encoding.GetEncoding("iso8859-1"));

            // Start a thread that reads all data from IRC
            _checkData = new Thread(new ThreadStart(Listen));
            _checkData.IsBackground = true;
            _checkData.Start();

            // Start a thread that checks the trivia states once every 3 seconds. 
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Console.WriteLine(DateTime.Now.ToString("yyyy-mm-dd H:mm:ss") + "... ... PROGRAM START");
            int intervalTimeInMilliseconds = 3000;
            _triviaProcessor = new ThreadProcessor(new Worker(new Logger(), _botData, "trivia", _channelID), new Logger(), intervalTimeInMilliseconds);

            _checkTrivia = new Thread(new ThreadStart(CheckTrivia));
            _checkTrivia.IsBackground = true;
            _checkTrivia.Priority = ThreadPriority.Lowest;
            _checkTrivia.Start();
            
            // Start a thread that refreshes the users in the channel every minute or so.
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Console.WriteLine(DateTime.Now.ToString("yyyy-mm-dd H:mm:ss") + "... ... PROGRAM START");
            intervalTimeInMilliseconds = 20000;
            _userProcessor = new ThreadProcessor(new Worker(new Logger(), _botData, "user", _channelID), new Logger(), intervalTimeInMilliseconds);

            _checkUsers = new Thread(new ThreadStart(CheckChannelUsers));
            _checkUsers.IsBackground = true;
            _checkUsers.Priority = ThreadPriority.Lowest;
            _checkUsers.Start();

            // Login to the service by providing credentials
            LoginToIRC(password, botName, channel);

            // Test for printing all values.
            _databaseCommands.PrintAllTestData();

            // TESTING PURPOSES ONLY
            _databaseCommands.PrintBotData(_botData);
        }

        // Read all information from the IRC server - when a message comes through
        // this interprets the response and appends it to the chat. 
        private void Listen()
        {
            try
            {
                while (_threadData)
                {
                    String data = "";

                    while ((data = _reader.ReadLine()) != null)
                    {
                        // String array to contain strings, split upon certain criteria
                        String[] pingCheck;

                        // Write out info to the GUI
                        _mainForm.SetText(data);

                        // If we have PING, reply with PONG
                        // This needs to be here, else IRC will kick the bot out of the channel.
                        pingCheck = data.Split(new char[] { ' ' }, 5);
                        if (pingCheck[0] == "PING")
                        {
                            // pingCheck[1] is the server.
                            _writer.WriteLine("PONG " + pingCheck[1]);
                            _writer.Flush();
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
                            String message = normalMessage[3] = normalMessage[3].TrimStart(':', ' ').Replace("'", "");

                            // The amount of commands we have.
                            int commandCount = _botData.channels[_channelID].channelCommands.Length;

                            // PRIVMSG refers to a message by a user. 
                            if (type == "PRIVMSG")
                            {
                                // Extract strings. 
                                String firstWord = message.Split(new char[] {' '}, 2)[0];
                                String username = serverAndUsername.Split(new char[] { '!' }, 2)[0].Replace(":", "");

                                // Increment the message count.
                                _messageCount++;

                                // Perform all of the command checks. 
                                for(int i = 0; i < commandCount; i++)
                                {
                                    // Check if the starting message features a command. 
                                    if (firstWord.ToLower().Equals(_botData.channels[_channelID].channelCommands[i].commandName.ToLower()))
                                    {
                                        String commandMessage = _botData.channels[_channelID].channelCommands[i].commandBody;

                                        SendMessage(commandMessage);
                                    }

                                    // Check if a command is repeatable or not, and if so, determine if we are at the point
                                    // where we need to write it to the IRC.
                                    if(_botData.channels[_channelID].channelCommands[i].commandRepeat)
                                    {
                                        if (_messageCount % _botData.channels[_channelID].channelCommands[i].commandRepeatCount == 0)
                                        {
                                            String commandMessage = _botData.channels[_channelID].channelCommands[i].commandBody;

                                            SendMessage(commandMessage);
                                        }
                                    }
                                }

                                //
                                // Check if the starting message is the answer to a trivia question
                                // TODO


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

                                        _chatCommands.AllCommands(_botData.channels[_channelID].channelCommands, commandCount, _channelID);

                                        break;

                                    // Add a new command.
                                    case "!addcommand":

                                        _chatCommands.AddCommand(message, username, _botData, commandCount, _channelID);

                                        break;
                                    
                                    // Delete a command
                                    case "!deletecommand":

                                        _chatCommands.DeleteCommand(username, message, _botData, _channelID);

                                        break;

                                    // Change the repeatable state of a command. The first parameter is a boolean and the second
                                    // the number of lines before the command body is written to the IRC.
                                    case "!editcommandrepeat":

                                        _chatCommands.UpdateCommandRepeat(username, message, _botData, _channelID);

                                        break;

                                    // Change the body of a command.
                                    case "!editcommand":

                                        _chatCommands.UpdateCommand(username, message, _botData, _channelID);

                                        break;

                                    // Start the trivia
                                    case "!starttrivia":

                                        //if(isUserModerator(username))
                                        //{
                                            // Toggle state and start trivia
                                            _botData.triviaActive = true;
                                            _botData.triviaTimePQuestion = 15.00f;
                                            _chatCommands.SetTriviaQuestion(_botData);
                                        //}

                                        break;

                                    // Add a new trivia question
                                    case "!addtriviaquestion":

                                        //if (isUserModerator(username))
                                            _chatCommands.AddTriviaQuestion(message, _botData);
                                        
                                        break;

                                    // Delete a trivia question
                                    case "!deletetriviaquestion":

                                        break;

                                    // Stop the trivia
                                    case "!stoptrivia":

                                        //if (isUserModerator(username))
                                            _botData.triviaActive = false;
                                        
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
        }

        // Create a new thread that listens to the state of the BotData's
        // booleans to determine whether or not trivia is active, and also
        // if a question is set or not - 'set' is defined by if there is not an
        // active question that is being 'counted' down, I.e in the progress of
        // being answered.
        private void CheckTrivia()
        {
            while (_threadTrivia)
            {
                _triviaProcessor.Run();
            }
        }

        //
        // Refresh the users in the channel. The thread assigned to this method determines the users in the
        // channel by creating a certain amount of objects (Users). This is then used for bot command validations.
        // For example, mods can only use !deleteCommand etc. 
        //
        private void CheckChannelUsers()
        {
            while (_threadUsers)
            {
                _userProcessor.Run();
            }
        }

        //
        // Identify exceptions global to the variables defined by the thread processors. 
        //
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Log exception / send notification.
            new Logger().Log("-Global unhandled exception:" + e.ExceptionObject.ToString(), 0);
        }

        //
        // Make the bot leave the channel from an IRC point of view.
        // Independant from the database.
        //
        public void LeaveChannel()
        {
            // Make the bot leave the current channel
            _writer.WriteLine("PART #" + _channel.ToLower());
            _writer.Flush();
        }

        //
        // Provide all of the details requested by the IRC to log into the server.
        // Password, nickname and channel, in that order.
        //
        private void LoginToIRC(String pass, String nick, String channel)
        {
            _writer.WriteLine("PASS " + pass);
            _writer.Flush();
            _writer.WriteLine("NICK " + nick);
            _writer.Flush();
            _writer.WriteLine("JOIN #" + channel.ToLower());
            _writer.Flush();
        }

        //
        // Write a message to the channel the bot is currently in.
        //
        public void SendMessage(string message)
        {
            _writer.WriteLine("PRIVMSG #" + _channel + " : " + message + "\r\n");
            _writer.Flush();
        }

        //
        // Make sure we close the threads down from above upon exiting. 
        // Else we will have memory leaks and an application that will
        // not fully close.
        //
        public void DisposeThreads()
        {
            Console.WriteLine("Closing application, aborting threads.");

            // Try, catch, finally required to abort threads, as per
            // MSDN specification.
            try
            {
                _checkData.Abort();
                _checkTrivia.Abort();
                _checkUsers.Abort();
            }
            catch (ThreadAbortException)
            { }
            finally
            { }

            // Just incase a thread remains active after closure of the application.
            _threadData = false;
            _threadTrivia = false;
            _threadUsers = false;

            // Thread output
            try
            {
                Console.WriteLine("Data thread: " + _checkData.IsAlive);
                Console.WriteLine("Trivia thread: " + _checkTrivia.IsAlive);
                Console.WriteLine("Users thread: " + _checkUsers.IsAlive);
            } catch(NullReferenceException)
            {
                Console.WriteLine("Thread not defined/instansiated.");
            }
        }
    }
}
