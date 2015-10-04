using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ForlornscTriviaBot.IRC;
using System.IO;
using ForlornscTriviaBot.Database;
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

namespace ForlornscTriviaBot
{
    public partial class TriviaBot : Form
    {
        // public variables
        private static IRC.Bot bot;

        // Delegate - for using threads in an asynchronous manner
        delegate void SetTextCallback(string message);

        public TriviaBot()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            //
            // Create a new instance of a bot for a specific channel
            // Required: Nick:    an actual registered account
            //           pass:    an oauth password
            //           channel: the channel for the bot to join
            //
            try
            {
                // Collect variable information
                String server = txtServer.Text.ToString();
                String botName = txtBotName.Text.ToString();
                String channel = txtChannel.Text.ToString();
                String password = txtPassword.Text.ToString();
                int port = Convert.ToInt16(txtPort.Text.ToString());

                if (port == 80 || port == 6667)
                {
                    // Retrieve values from the database for the specific bot
                    //Database.Database database = new Database.Database();
                    //BotData botResults = database.GetBotResults(txtBotName.Text.ToString());

                    // Create the new bot
                    bot = new IRC.Bot(server, port, botName, channel, password, this);
                }
                else
                    // Requires a TCP port
                    txtChat.Text += "Uncompatiable port number, please revise (80 and 6667 have been tested and are usable) \r\n";
            }
            catch (FormatException err)
            {
                txtChat.Text += err.Message + "\r\n";
            }

        }

        //
        // If the calling thread is different from the thread that 
        // created the TextBox control, this method creates a 
        // SetTextCallback and calls itself asynchronously using the 
        // Invoke method. 
        //
        // Therefore using a delegate is required to ensure thread safety. 
        //
        public void SetText(String message)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            //
            // The application will crash if this is not checked, however this
            // is probably only accurate in terms of debugging and will be okay during runtime.
            //
            if (this.txtChat.InvokeRequired)
            {
                SetTextCallback callback = new SetTextCallback(SetText);
                this.Invoke(callback, new object[] { message });
            }
            else
            {
                // Append message to the chat
                txtChat.Text += message + "\r\n";
            }
        }

        private void TriviaBot_FormClosed(object sender, FormClosedEventArgs e)
        {
            // When the form is closed, make the bot(s) leave the channels they are in.
            try
            {
                // Make the bot leave the channel(test)
                //bot.LeaveChannel();
                bot.DisposeThreads();
            }
            catch (NullReferenceException)
            { } // For when we exit without connecting
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            // Send a message
            try
            {
                // Remember: hashtag denotes channel and : denotes a sentence. 
                bot.SendMessage(txtSend.Text.ToString());
            }
            catch (NullReferenceException err)
            {
                txtChat.Text += "Please connect to a channel before sending a message.";
                Console.WriteLine("Error: " + err.Message);
            }
        }

    }
}
