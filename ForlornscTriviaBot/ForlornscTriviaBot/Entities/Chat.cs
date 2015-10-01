using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Runtime.Serialization;

namespace ForlornscTriviaBot.Entities
{
    //
    // Store the information for the users in the channel. This will be
    // used by a thread to constantly update the moderators for the purpose
    // of only allowing moderators to use specific commands.
    //
    [DataContract]
    public class Chat
    {
        //
        // The number of moderators. If this doesnt change in validation for the thread
        // then we can not perform the logic to update it and therefore increase the
        // efficiency of the program.
        //
        private int _numModerators;
        [DataMember]
        public int numModerators
        {
            get { return _numModerators; }
            set { _numModerators = value; }
        }

        //
        // The moderators of the channel.
        // It would be a waste of computational power to add the users too.
        //
        private User[] _channelModerators;
        [DataMember]
        public User[] channelModerators
        {
            get { return _channelModerators; }
            set { _channelModerators = value; }
        }

        public Chat()
        {

        }

        public Chat(int numModerators, User[] channelModerators)
        {
            _numModerators = numModerators;
            _channelModerators = channelModerators;
        }
    }
}
