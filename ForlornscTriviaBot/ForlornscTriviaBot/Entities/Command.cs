﻿using System;
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
    [DataContract]
    public class Command
    {
        //
        // Identifier for command
        //
        private int _objectId;
        [DataMember]
        public int objectId
        {
            get { return _objectId; }
            set { _objectId = value; }
        }

        //
        // The channel that the command is linked to.
        //
        private int _channelId;
        [DataMember]
        public int channelId
        {
            get { return _channelId; }
            set { _channelId = value; }
        }

        //
        // The name of the command
        //
        private String _commandName;
        [DataMember]
        public String commandName
        {
            get { return _commandName; }
            set { _commandName = value; }
        }

        //
        // The string response that appears when a user uses a command.
        //
        private String _commandBody;
        [DataMember]
        public String commandBody
        {
            get { return _commandBody; }
            set { _commandBody = value; }
        }

        //
        // Constructor
        //
        public Command(int objectId, int channelId, String strCommandName, String strCommandBody)
	    {
            this._objectId = objectId;
            this._channelId = channelId;
            this._commandName = strCommandName;
            this._commandBody = strCommandBody;
	    }
    }
}
