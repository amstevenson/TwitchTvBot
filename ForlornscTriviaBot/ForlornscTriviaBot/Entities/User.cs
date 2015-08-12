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
    // A user is effectively a moderator for a channel.
    // See the description of the Chat class for more information.
    //
    [DataContract]
    class User
    {
        //
        // The name of the user.
        //
        private String _username;
        [DataMember]
        public String username
        {
            get { return _username; }
            set { _username = value; }
        }

        //
        // Default constructor, needs a name.
        //
        public User(String username)
        {
            _username = username;
        }
    }
}
