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

//
// For each channel a bot is connected to, we have
// the specific values for commands, scores, questions etc.
//
namespace ForlornscTriviaBot.Entities
{
    [DataContract]
    public class Channel
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
        // The foreign key determining the bot the channel belongs to
        //
        private int _botID;
        [DataMember]
        public int botID
        {
            get { return _botID; }
            set { _botID = value; }
        }

        //
        // The name of the channel
        //
        private String _channelName;
        [DataMember]
        public String channelName
        {
            get { return _channelName; }
            set { _channelName = value; }
        }

        //
        // The commands for the channel
        //
        private Command[] _channelCommands;
        [DataMember]
        public Command[] channelCommands
        {
            get { return _channelCommands; }
            set { _channelCommands = value; }
        }

        //
        // The scores for viewers for correct answers
        //
        private Scorer[] _scorers;
        [DataMember]
        public Scorer[] scorers
        {
            get { return _scorers; }
            set { _scorers = value; }
        }

        //
        // First constructor, without scorers and commands
        //
        public Channel(int objectId, int botID, String strChannelName)
	    {
            this._objectId = objectId;
            this._botID = botID;
            this._channelName = strChannelName;
	    }

        //
        // Second constructor, without scorers
        //
        public Channel(int objectId, int botID, String strChannelName, Command[] constrCommands)
        {
            this._objectId = objectId;
            this._botID = botID;
            this._channelName = strChannelName;
            this._channelCommands = constrCommands;
        }

        //
        // Third contructor - with scorers
        //
        public Channel(int objectId, int botID, String strChannelName, Command[] constrCommands, Scorer[] constrScorers)
        {
            this._objectId = objectId;
            this._botID = botID;
            this._channelName = strChannelName;
            this._channelCommands = constrCommands;
            this._scorers = constrScorers;
        }
    }
}
