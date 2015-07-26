using System;
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
        private BotCommands botCommands;
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
            this.botCommands = new BotCommands(botName); // Assigns the bot name in the BotCommands class; used for CRUD operations.
            this.threadActive = true;

            // Check to see if the bot is recorded in the database or not.
            // if it is not in the database, add it.
            if (!botCommands.DoesBotExist()) 
                botCommands.AddNewBot();

            // Retrieve Database related values specific to the bot.
            this.botData = botCommands.GetBotResults();
            
            // Check to see if the bot is already in the channel that is entered in the GUI
            // If it doesnt exist in the database, then add it. 
            if (!botCommands.DoesChannelExist(botData.objectId, channel)) 
                botCommands.AddChannel(botData.objectId, channel);

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
            botCommands.PrintAllTestData();

            // Print out the botData for testing purposes.
            botCommands.PrintBotData(botData);

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

                        // Write to the GUI
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
                                // Extract the username for database purposes later on.
                                String username = "";
                                char nameCharacter = ' ';

                                for (int i = 1; i < serverAndUsername.Length -1; i++)
                                {
                                    if (nameCharacter.Equals('!')) break;

                                    // Extract the username from the serverandusername string.
                                    nameCharacter = serverAndUsername.ElementAt(i);
                                    username += nameCharacter;

                                }

                                // Check if the starting message features a command. 
                                for(int i = 0; i < commandCount; i++)
                                {
                                    if(message.StartsWith(botData.channels[channelID].channelCommands[i].commandName))
                                    {
                                        String commandMessage = "PRIVMSG #" + channel + " : " + 
                                            botData.channels[channelID].channelCommands[i].commandBody;

                                        SendMessage(commandMessage);
                                    }
                                }

                                // A command used to print out the commands for a specific channel.
                                if (message.StartsWith("!cmds"))
                                {
                                    String commandsMessage = "PRIVMSG #" + channel + " : The commands for this channel are: ";

                                    if (commandCount > 0)
                                        for (int i = 0; i < commandCount; i++)
                                        {
                                            if(i == 0)
                                                commandsMessage += botData.channels[channelID].channelCommands[i].commandName;
                                            else
                                                commandsMessage += ", " + botData.channels[channelID].channelCommands[i].commandName;
                                        }
                                    else
                                        commandsMessage += "Unfortunately, this channel currently has no commands. To add one, please use !addCommand. " + 
                                        "The format for this operation is: !addCommand !CommandName CommandContent (as a message) .";

                                    // show a list of commands
                                    SendMessage(commandsMessage);
                                }

                                // A command used to add a new command to the database.
                                else if(message.StartsWith("!addCommand"))
                                {
                                    // Extract new command strings
                                    String[] commandContent = message.Split(new char[] { ' ' }, 3);
                                    String commandName = commandContent[1];
                                    String commandBody = commandContent[2];

                                    // Add the new command to the database.
                                    if (commandCount < 10)
                                    {
                                        if (!botCommands.DoesCommandExist(botData.channels[channelID].objectId, commandName))
                                        {
                                            String messageToSend = "PRIVMSG #" + channel + " : " + commandName + " has been added successfully.";
                                            botCommands.AddCommand(botData.channels[channelID].objectId, commandName, commandBody);

                                            // Add the new command to the list.
                                            List<Command> commands = botCommands.GetCommands(botData.channels[channelID].objectId);
                                            botData.channels[channelID].channelCommands = commands.ToArray();

                                            // Send confirmation of success.
                                            SendMessage(messageToSend);
                                        }
                                        else
                                        {
                                            String messageToSend = "PRIVMSG #" + channel + " : " + commandName + " already exists. Please delete it " +
                                                                                                                 "and add again for alteration purposes.";
                                            SendMessage(messageToSend);
                                        }
                                    }
                                    else
                                    {
                                        String messageToSend = "PRIVMSG #" + channel + " : The maximum amount of commands per channel is 10. " + 
                                            "To add more, delete a previous command and add a new one.";
                                        SendMessage(messageToSend);
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                listen.Abort();
                Console.WriteLine("Error: " + e.Message);
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
