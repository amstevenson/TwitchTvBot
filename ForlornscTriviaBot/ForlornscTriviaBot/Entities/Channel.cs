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
        private String _objectId;
        [DataMember]
        public String objectId
        {
            get { return _objectId; }
            set { _objectId = value; }
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
        private Score[] _scores;
        [DataMember]
        public Score[] scores
        {
            get { return _scores; }
            set { _scores = value; }
        }

        //
        // Channel created at
        //
        private String _createdAt;
        [DataMember]
        public String createdAt
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        //
        // Last updated
        //
        private String _updatedAt;
        [DataMember]
        public String updatedAt
        {
            get { return _updatedAt; }
            set { _updatedAt = value; }
        }

        //
        // Constructors - other values are added in later. 
        //
        public Channel(String strObjectId, String strChannelName, String strCreatedAt, String strUpdatedAt)
	    {
            this._objectId = strObjectId;
            this._channelName = strChannelName;
            this._createdAt = strCreatedAt;
            this._updatedAt = strUpdatedAt;
	    }
    }
}
