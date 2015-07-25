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
    // A scorer is classed as a person who answers at least one question correcltly.
    // Their "score" is stored in the database object that is related to them.
    // I hate the term "scorer"...must be something better.
    //
    [DataContract]
    public class Scorer
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
        private int _channelId;
        [DataMember]
        public int channelId
        {
            get { return _channelId; }
            set { _channelId = value; }
        }

        //
        // The value of the scorer
        //
        private int _scorerValue;
        [DataMember]
        public int scorerValue
        {
            get { return _scorerValue; }
            set { _scorerValue = value; }
        }

        //
        // The username of the scorer
        //
        private String _scorerUsername;
        [DataMember]
        public String scorerUsername
        {
            get { return _scorerUsername; }
            set { _scorerUsername = value; }
        }

        //
        // Constructor
        //
        public Scorer(int intObjectId, int intChannelId, int intScorerValue, String strScorerUsername)
        {
            this._objectId = intObjectId;
            this._channelId = intChannelId;
            this._scorerValue = intScorerValue;
            this._scorerUsername = strScorerUsername;
        }
    }
}
