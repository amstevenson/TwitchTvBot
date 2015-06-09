using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public BotCommands(String botName)
        {
            // Default constructor
            this.botName = botName;
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
        // Add a new string command. 
        // Others will probably have different formats, such as shuffle etc
        // but giving people the ability to add string commands is not too bad for now.
        // Note to self however that this should only be given to moderators, else things
        // can get a little bit tricky of complicated if people decide to spam it. 
        //
        public void AddCommand(String channelName)
        {

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

    }
}
