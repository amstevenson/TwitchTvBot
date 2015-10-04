using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForlornscTriviaBot.Entities;

namespace ForlornscTriviaBot.IRC
{
    class ChatCommands
    {
        private String _channel;
        private Bot _bot;
        private DatabaseCommands _databaseCommands;

        public ChatCommands(String channel, Bot bot, DatabaseCommands databaseCommands)
        {
            // Default constructor, requires all of the global variables.
            this._channel = channel;
            this._bot = bot;
            this._databaseCommands = databaseCommands;
        }

        //
        // Get all of the commands. Called as a reponse to !cmds. 
        //
        public void AllCommands(Command[] channelCommands, int commandCount, int channelID)
        {
            String commandsMessage = "The commands for this channel are: ";

            if (commandCount > 0)
                for (int i = 0; i < commandCount; i++)
                {
                    if (i == 0)
                        commandsMessage += channelCommands[i].commandName;
                    else
                        commandsMessage += ", " + channelCommands[i].commandName;
                }
            else
                commandsMessage += "Unfortunately, this channel currently has no commands. To add one, please use !addCommand. " +
                "The format for this operation is: !addCommand !CommandName CommandContent (as a message) .";

            // show a list of commands
            _bot.SendMessage(commandsMessage);
        }

        //
        // Adds a new command, called as a response to !addCommand. Should only be used by a moderator. 
        //
        public void AddCommand(String wholeChatMessage, String createdBy, BotData botData, int commandCount, int channelID)
        {
            int numberOfSpaces = wholeChatMessage.TrimEnd(' ').Split(' ').Length - 1;

            // Check that the command string is in the correct format.
            if (!wholeChatMessage.TrimEnd(' ').Equals("!addcommand") && 
                numberOfSpaces > 1)
            {

                // Extract new command strings
                String[] commandContent = wholeChatMessage.Split(new char[] { ' ' }, 3);
                String commandName = commandContent[1];
                String commandBody = commandContent[2];

                if (commandName.StartsWith("!"))
                {
                    // If we dont start with illegal functions
                    if (!commandBody.ToLower().StartsWith("/ban") && !commandBody.ToLower().StartsWith("/timeout")
                        && !commandBody.ToLower().StartsWith("/unban") && !commandBody.ToLower().StartsWith("/slow")
                        && !commandBody.ToLower().StartsWith("/slowoff") && !commandBody.ToLower().StartsWith("/subscribers")
                        && !commandBody.ToLower().StartsWith("/subscribersoff") && !commandBody.ToLower().StartsWith("/clear")
                        && !commandBody.ToLower().StartsWith("/r9kbeta") && !commandBody.ToLower().StartsWith("/r9kbetaoff"))
                    {
                        // If the length is correct
                        if (commandBody.Length < 300)
                        {
                            // If we are below the maximum ammount of commands
                            if (commandCount < 30)
                            {
                                // If the command doesnt already exist.
                                if (!_databaseCommands.DoesCommandExist(botData.channels[channelID].objectId, commandName))
                                {
                                    String messageToSend = commandName + " has been added successfully.";
                                    _databaseCommands.AddCommand(botData.channels[channelID].objectId, commandName, commandBody, createdBy);

                                    // Add the new command to the list.
                                    List<Command> commands = _databaseCommands.GetCommands(botData.channels[channelID].objectId);
                                    botData.channels[channelID].channelCommands = commands.ToArray();

                                    // Send confirmation of success.
                                    _bot.SendMessage(messageToSend);
                                }
                                else
                                {
                                    String messageToSend = commandName + " already exists. Please delete it " +
                                                                                                         "and add again for alteration purposes.";
                                    _bot.SendMessage(messageToSend);
                                }
                            }
                            else
                            {
                                String messageToSend = "The maximum amount of commands per channel is 30. " +
                                    "To add more, delete a previous command and add a new one.";
                                _bot.SendMessage(messageToSend);
                            }
                        }
                        else
                        {
                            String messageToSend = "The command exceeds 200 characters, please shorten it and try again :)";
                            _bot.SendMessage(messageToSend);
                        }
                    }
                    else
                    {
                        String messageToSend = "Please don't be naughty! I won't ban or kick people without a good reason..."
                                             + "at least offer me money or food first!";
                        _bot.SendMessage(messageToSend);
                    }
                }
                else
                {
                    String messageToSend = "The command does not start with a '!', please revise and try again. ";
                    _bot.SendMessage(messageToSend);
                }
            }
            else
            {
                String messageToSend = "To add a new command, please use the format of: {!addcommand} {!name} {body} - remove brackets. "
                    + "For example: !addcommand !specialFriedRice This tastes marvelous.";
                _bot.SendMessage(messageToSend);
            }
        }

        //
        // Delete a command. Called by !deleteCommand followed by the name. Should only be used by a moderator. 
        //
        public void DeleteCommand(String username, String wholeChatMessage, BotData botData, int channelID)
        {
            int numberOfSpaces = wholeChatMessage.TrimEnd(' ').Split(' ').Length - 1;

            // Check that the command string is in the correct format.
            if (!wholeChatMessage.TrimEnd(' ').Equals("!deletecommand") &&
                numberOfSpaces > 0)
            {
                // Extract the name of the command.
                String commandName = wholeChatMessage.Split(new char[] { ' ' }, 3)[1];

                // Delete the command.
                if (_databaseCommands.DoesCommandExist(botData.channels[channelID].objectId, commandName))
                {
                    int commandID = 0;
                    String messageToSend = "";

                    // Find the command ID
                    for (int i = 0; i < botData.channels[channelID].channelCommands.Length; i++)
                    {
                        if (botData.channels[channelID].channelCommands[i].commandName.Equals(commandName))
                            commandID = botData.channels[channelID].channelCommands[i].objectId;
                    }

                    // If we have found a result, delete the command using the identifer.
                    // The identifer will never be 0, so we can use that as a condition. 
                    if (commandID != 0)
                    {
                        // First however, check that the user can perform this function.
                        if (isUserModerator(username, botData, channelID) || (isUserValidForCommand(username, botData, channelID)))
                        {
                            //if the user is validated, delete the command.
                            if (_databaseCommands.DeleteCommand(commandID))
                            {
                                // Refresh the commands list. 
                                List<Command> commands = _databaseCommands.GetCommands(botData.channels[channelID].objectId);
                                botData.channels[channelID].channelCommands = commands.ToArray();

                                // Alter the message. 
                                messageToSend = commandName + " has been deleted successfully.";
                            }
                            else
                                messageToSend = commandName + " has not been deleted. Please contact me, I want to find out why :(";
                        }
                        else
                        {
                            messageToSend = "Woah! You can't do this " + username + "! You need to be either a moderator, or the person " +
                                 "who created the command!";
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error deleting command, Identifer mismatch.");
                    }

                    if (!messageToSend.Equals(""))
                    {
                        // Send confirmation of success.
                        _bot.SendMessage(messageToSend);
                    }
                    else
                        Console.WriteLine("Error deleting command, technical error. Most likely a command identifer mismatch.");
                }
                else
                {
                    String messageToSend = commandName + " does not exist; it needs to be added before it can be deleted.";
                    _bot.SendMessage(messageToSend);
                }
            }
            else {
                String messageToSend = "To delete a command, please use the format of: {!deletecommand} {!commandname} - remove brackets. "
                    + "For example: !deletecommand !specialFriedRice";
                _bot.SendMessage(messageToSend);
            }
        }

        //
        // Update a command, so that it prints out it's body after a specified amount of lines, or vise versa.
        //
        public void UpdateCommandRepeat(String username, String wholeChatMessage, BotData botData, int channelID)
        {
            int numberOfSpaces = wholeChatMessage.TrimEnd(' ').Split(' ').Length - 1;

            // Check that the command string is in the correct format.
            if (!wholeChatMessage.TrimEnd(' ').Equals("!editcommandrepeat") &&
                numberOfSpaces > 2)
            {
                if (isUserModerator(username, botData, channelID))
                {
                    // Extract information.
                    String[] commandContent = wholeChatMessage.Split(new char[] { ' ' }, 4);
                    String commandName = commandContent[1];
                    String commandRepeatString = commandContent[2];
                    int commandRepeatBit = 0;
                    int commandAmountOfLines = 0;

                    try
                    {
                        // Collect the integers. 1 is true, 0 is false.
                        // The parsing will fail if the number is too high, or low...
                        // Basic kind of stuffs.
                        switch (commandRepeatString.ToLower())
                        {
                            case "!on":

                                commandRepeatBit = 1;

                                break;

                            case "!off":

                                commandRepeatBit = 0;

                                break;

                            default:

                                break;
                        }

                        commandAmountOfLines = Convert.ToInt16(commandContent[3]);
                    }
                    catch (FormatException ex)
                    {
                        Console.WriteLine("Invalid cast: " + ex);
                    }
                    catch (OverflowException ex)
                    {
                        Console.WriteLine("Overflow exception: " + ex);
                    }

                    if (commandName.StartsWith("!"))
                    {
                        // If the command doesnt already exist.
                        if (_databaseCommands.DoesCommandExist(botData.channels[channelID].objectId, commandName))
                        {
                            String messageToSend;

                            if (_databaseCommands.UpdateCommandRepeat(commandName, botData.channels[channelID].objectId, commandRepeatBit, commandAmountOfLines))
                            {

                                // Update the commands list.
                                List<Command> commands = _databaseCommands.GetCommands(botData.channels[channelID].objectId);
                                botData.channels[channelID].channelCommands = commands.ToArray();

                                messageToSend = commandName + "'s repeating settings have been updated.";

                                // Send confirmation of success.
                                _bot.SendMessage(messageToSend);
                            }
                            else
                            {
                                messageToSend = "There has been an error updating this command, please try again or contact Forlornsc.";
                            }
                        }
                        else
                        {
                            String messageToSend = commandName + " doesn't exist.";
                            _bot.SendMessage(messageToSend);
                        }
                    }
                    else
                    {
                        String messageToSend = "The command does not start with a '!', please revise and try again. ";
                        _bot.SendMessage(messageToSend);
                    }
                }
                else
                {
                    String messageToSend = "Woah! You can't do this " + username + "! You need to be either a moderator! ";
                    _bot.SendMessage(messageToSend);
                }
            }
            else
            {
                String messageToSend = "To specify how many lines it will take before the command is repeated, use the format of: " + 
                    "{!editcommandrepeat} {!commandname} {on/off} {number of lines}. For example: !editcommandrepeat !specialfriedrice !on !20; this repeats the commands " + 
                    "message body every 20 lines. NOTE: only moderators can use this function.";
                _bot.SendMessage(messageToSend);
            }
        }

        public void UpdateCommand(String username, String wholeChatMessage, BotData botData, int channelID)
        {
            int numberOfSpaces = wholeChatMessage.TrimEnd(' ').Split(' ').Length - 1;

            // Check that the command string is in the correct format.
            if (!wholeChatMessage.TrimEnd(' ').Equals("!editcommand") &&
                numberOfSpaces > 1)
            {
                // Extract new command strings
                String[] commandContent = wholeChatMessage.Split(new char[] { ' ' }, 3);
                String commandName = commandContent[1];
                String commandBody = commandContent[2];

                if (commandName.StartsWith("!"))
                {
                    // If we dont start with illegal functions
                    if (!commandBody.ToLower().StartsWith("/ban") && !commandBody.ToLower().StartsWith("/timeout")
                        && !commandBody.ToLower().StartsWith("/unban") && !commandBody.ToLower().StartsWith("/slow")
                        && !commandBody.ToLower().StartsWith("/slowoff") && !commandBody.ToLower().StartsWith("/subscribers")
                        && !commandBody.ToLower().StartsWith("/subscribersoff") && !commandBody.ToLower().StartsWith("/clear")
                        && !commandBody.ToLower().StartsWith("/r9kbeta") && !commandBody.ToLower().StartsWith("/r9kbetaoff"))
                    {
                        // If the length is correct
                        if (commandBody.Length < 300)
                        {
                            // If we are below the maximum ammount of commands
                            if (botData.channels[channelID].channelCommands.Length < 30)
                            {
                                // If the command doesnt already exist.
                                if (_databaseCommands.DoesCommandExist(botData.channels[channelID].objectId, commandName))
                                {
                                    // Before updating the command, check that the user is either a moderator or the person who created
                                    // the command.
                                    if (isUserModerator(username, botData, channelID) || (isUserValidForCommand(username, botData, channelID)))
                                    {
                                        if (_databaseCommands.UpdateCommand(commandName, botData.channels[channelID].objectId, commandBody))
                                        {
                                            // Add the new command to the list.
                                            List<Command> commands = _databaseCommands.GetCommands(botData.channels[channelID].objectId);
                                            botData.channels[channelID].channelCommands = commands.ToArray();

                                            Console.WriteLine(commands[0].commandBody);

                                            String messageToSend = commandName + "'s message body has been updated.";

                                            // Send confirmation of success.
                                            _bot.SendMessage(messageToSend);
                                        }
                                        else
                                        {
                                            String messageToSend = "There has been an error updating this command.";
                                            _bot.SendMessage(messageToSend);
                                        }
                                    }
                                    else
                                    {
                                        String messageToSend = "Woah! You can't do this " + username + "! You need to be either a moderator, or the person who created the command!";
                                        _bot.SendMessage(messageToSend);
                                    }
                                }
                                else
                                {
                                    String messageToSend = commandName + " doesn't exist. If this is incorrect, please contact Forlornsc.";
                                    _bot.SendMessage(messageToSend);
                                }
                            }
                            else
                            {
                                String messageToSend = "The maximum amount of commands per channel is 30. " +
                                    "To add more, delete a previous command and add a new one.";
                                _bot.SendMessage(messageToSend);
                            }
                        }
                        else
                        {
                            String messageToSend = "The command exceeds 200 characters, please shorten it and try again :)";
                            _bot.SendMessage(messageToSend);
                        }
                    }
                    else
                    {
                        String messageToSend = "Please don't be naughty! I won't ban or kick people without a good reason..."
                                             + "at least offer me money or food first!";
                        _bot.SendMessage(messageToSend);
                    }
                }
                else
                {
                    String messageToSend = "The command does not start with a '!', please revise and try again. ";
                    _bot.SendMessage(messageToSend);
                }
            }
            else
            {
                String messageToSend = "To update the message body of a command, use the format of: " +
                    "{!editcommand} {!commandname} {new command message}. For example: !editcommand !specialfriedrice I taste brilliant all spicy with prawns and curry sauce. " + 
                    "NOTE: only moderators can use this command.";
                _bot.SendMessage(messageToSend); 
            }
        }

        //
        // Add a new trivia question.
        //
        public void AddTriviaQuestion(String wholeChatMessage, BotData botData)
        {
            // Remove command from string. 
            wholeChatMessage = wholeChatMessage.Replace("!addtriviaquestion ", "");

            // Make sure we don't have an index out of bounds exception.
            int amountOfSemicolons = wholeChatMessage.Count(c => c == ';');

            if (amountOfSemicolons == 1)
            {
                // Extract relevant strings; question body, question answer. 
                String[] triviaQuestionContent = wholeChatMessage.Split(new char[] { ';' }, 2);
                String triviaQuestion = triviaQuestionContent[0].TrimStart().TrimEnd().Replace("'", "");
                String triviaAnswer = triviaQuestionContent[1].TrimStart().TrimEnd().Replace("'", "");

                // Add the new question if it doesn't already exist from within the database.
                if (!_databaseCommands.DoesTriviaQuestionExist(botData.objectId, triviaQuestion))
                {
                    if (_databaseCommands.AddTriviaQuestion(botData.objectId, triviaQuestion, triviaAnswer))
                    {
                        // Refresh the questions list; essentially retrieves all of them again.
                        List<TriviaQuestion> questions = _databaseCommands.GetTriviaQuestions(botData.objectId);
                        botData.triviaQuestions = questions.ToArray();

                        String messageToSend = "Question has been added successfully:) ";
                        _bot.SendMessage(messageToSend);
                    }
                    else
                    {
                        // If adding fails.
                        String messageToSend = "Error adding trivia bot, please contact Forlornsc " +
                                 "if this persists.";
                        _bot.SendMessage(messageToSend);
                    }
                }
                else
                {
                    // If the question already exists.
                    String messageToSend = "Unforunately, this question already exists, please ';' " +
                                                     "revise and enter it again : (";
                    _bot.SendMessage(messageToSend);
                }
            }
            else
            {
                // If there is a formatting error with the provided string.
                String messageToSend = "Formatting error, please make sure there is one ';' " +
                                                                     "seperator between the question and answer.";
                _bot.SendMessage(messageToSend);
            }
        }

        //
        // Set the active trivia question. 
        //
        public void SetTriviaQuestion(BotData botData)
        {
            // Check to see if trivia is enabled. 
            // If a question is not set, it is set here. This may happen
            // in the case where people are not providing the correct answer
            // and the "times up" condition triggers. 
            if (botData.triviaActive)
            {
                // Set a new question. 
                if (!botData.questionActive)
                {
                    int amountOfQuestions = botData.triviaQuestions.Length;
                }
            }
        }

        //
        // Go through each of the moderators that we have in the current channel, and
        // if there is a match, allow the method to be invoked.
        //
        private bool isUserModerator(String username, BotData botData, int channelID)
        {
            try
            {
                for (int i = 0; i < botData.channels[channelID].channelViewers.numModerators; i++)
                {
                    if (botData.channels[channelID].channelViewers.channelModerators[i].username.Equals(username))
                        return true;
                }
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Null reference: " + ex.Message);
            }

            // If there are no moderators, or if there is not a match.
            return false;
        }

        //
        // This method checks if a command is being accessed by the user who created it. So this should be external to the above method for
        // that reason alone. This method does not allow non moderators to delete other non moderators commands, or alter them.
        //
        private bool isUserValidForCommand(String username, BotData botData, int channelID)
        {
            try
            {
                for (int i = 0; i < botData.channels[channelID].channelCommands.Length; i++)
                {
                    // For each command we have for this channel (max 30), determine if the user who is requesting to
                    // use an elevated method, is the one who created it or not. If so, then they should be allowed to
                    // delete or modify it. So in this case, return true.
                    if (botData.channels[channelID].channelCommands[i].commandCreatedBy.Equals(username))
                        return true;
                }
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Null reference: " + ex.Message);
                return false;
            }

            // If the above does not trigger true, return false by default.
            return false;
        }
    }
}
