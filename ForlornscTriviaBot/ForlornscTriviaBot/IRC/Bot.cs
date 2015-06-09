using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Forms;

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

        private BotCommands botCommands;

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
            this.botCommands = new BotCommands(botName);
            this.threadActive = true;

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
                            // 0: Server and channel (irrelevent to an extent)
                            // 1: Type (PRIVMSG, JOIN, PART ETC)
                            // 2: channel
                            // 3: message
                            //
                            String type = normalMessage[1];
                            String channel = normalMessage[2] = normalMessage[2].Replace("#", "");
                            String message = normalMessage[3] = normalMessage[3].Replace(":", "");

                            if (type == "PRIVMSG")
                            {
                                // We know this is a message
                                if (message.StartsWith("!cmds"))
                                {
                                    // show a list of commands
                                    SendMessage("PRIVMSG #" + channel + " : list: blargle blargle");
                                    //Console.WriteLine("goes in here");
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
