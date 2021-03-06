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

    //
    // The questions belonging to a specific bot.
    // This kind of implies that the same set of questions will not be available for
    // more than one bot, but I guess it makes sense to do it that way. At any rate, its
    // how I have done it for now. 
    //
    [DataContract]
    public class TriviaQuestion
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
        // The question itself. Text and all. For example: "What is 2 + 2?". 
        //
        private String _questionText;
        [DataMember]
        public String questionText
        {
            get { return _questionText; }
            set { _questionText = value; }
        }

        //
        // The answer to the question. For example: " 5 (it actually is...kind of...why not google that one)."
        //
        private String _questionAnswer;
        [DataMember]
        public String questionAnswer
        {
            get { return _questionAnswer; }
            set { _questionAnswer = value; }
        }

        //
        // Constructor
        //
        public TriviaQuestion(int intObjectId, int intBotId, String strQuestionText, String strQuestionAnswer)
        {
            this._objectId = intObjectId;
            this._botID = intBotId;
            this._questionText = strQuestionText;
            this._questionAnswer = strQuestionAnswer;
        }
        
    }
}
