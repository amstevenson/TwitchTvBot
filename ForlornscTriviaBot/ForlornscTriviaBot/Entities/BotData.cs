using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Runtime.Serialization;

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

namespace ForlornscTriviaBot.Entities
{
    //
    // The results comprises all of the information for the channel(s) that the bot is going to.
    // There will be a seperate one for each bot. For example: ForlornBot may go to three channels
    // and will have seperate commands to another bot. 
    // Another example: In terms of trivia, each channel will have a different score for each player. 
    //
    [DataContract]
    public class BotData
    {
        //
        // The objectId is the primary key
        //
        private int _objectId;
        [DataMember]
        public int objectId
        {
            get { return _objectId; }
            set { _objectId = value; }
        }

        //
        // The name of the bot.
        //
        private String _botName;
        [DataMember]
        public String BotName
        {
            get { return _botName; }
            set { _botName = value; }
        }

        //
        // All of the channels the bot will go to automatically. 
        // Each channel has: scores for viewers for questions and commands.
        //
        private Channel[] _channels;
        [DataMember]
        public Channel[] channels
        {
            get { return _channels; }
            set { _channels = value; }
        }

        //
        // The trivia questions for the bot
        //
        private TriviaQuestion[] _triviaQuestions;
        [DataMember]
        public TriviaQuestion[] triviaQuestions
        {
            get { return _triviaQuestions; }
            set { _triviaQuestions = value; }
        }

        // First (empty) constructor
        public BotData()
        {
            // Empty
        }

        // Second Constructor
        public BotData(int botID, String botName)
        {
            this._objectId = botID;
            this._botName = botName;
        }

        // Second constructor
        public BotData(int botID, String botName, Channel[] channels)
        {
            this._objectId = botID;
            this._botName = botName;
            this._channels = channels;
        }

        // third constructor
        public BotData(int botID, Channel[] channels, TriviaQuestion[] triviaQuestions)
        {
            this._objectId = botID;
            this._channels = channels;
            this._triviaQuestions = triviaQuestions;
        }

    }
}
