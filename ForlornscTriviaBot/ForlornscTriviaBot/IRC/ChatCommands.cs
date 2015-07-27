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
        private String channel;
        private Bot bot;
        private DatabaseCommands databaseCommands;

        public ChatCommands(String channel, Bot bot, DatabaseCommands databaseCommands)
        {
            // Default constructor, requires all of the global variables.
            this.channel = channel;
            this.bot = bot;
            this.databaseCommands = databaseCommands;
        }

        //
        // Get all of the commands. Called as a reponse to !cmds. 
        //
        public void AllCommands(Command[] channelCommands, int commandCount, int channelID)
        {
            String commandsMessage = "PRIVMSG #" + channel + " : The commands for this channel are: ";

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
            bot.SendMessage(commandsMessage);
        }

        //
        // Adds a new command, called as a response to !addCommand. Should only be used by a moderator. 
        //
        public void AddCommand(String wholeChatMessage, BotData botData, int commandCount, int channelID)
        {
            // Extract new command strings
            String[] commandContent = wholeChatMessage.Split(new char[] { ' ' }, 3);
            String commandName = commandContent[1];
            String commandBody = commandContent[2];

            if (commandName.StartsWith("!"))
            {
                // Add the new command to the database.
                if (commandCount < 10)
                {
                    if (!databaseCommands.DoesCommandExist(botData.channels[channelID].objectId, commandName))
                    {
                        String messageToSend = "PRIVMSG #" + channel + " : " + commandName + " has been added successfully.";
                        databaseCommands.AddCommand(botData.channels[channelID].objectId, commandName, commandBody);

                        // Add the new command to the list.
                        List<Command> commands = databaseCommands.GetCommands(botData.channels[channelID].objectId);
                        botData.channels[channelID].channelCommands = commands.ToArray();

                        // Send confirmation of success.
                        bot.SendMessage(messageToSend);
                    }
                    else
                    {
                        String messageToSend = "PRIVMSG #" + channel + " : " + commandName + " already exists. Please delete it " +
                                                                                             "and add again for alteration purposes.";
                        bot.SendMessage(messageToSend);
                    }
                }
                else
                {
                    String messageToSend = "PRIVMSG #" + channel + " : The maximum amount of commands per channel is 10. " +
                        "To add more, delete a previous command and add a new one.";
                    bot.SendMessage(messageToSend);
                }
            }
            else
            {
                String messageToSend = "PRIVMSG #" + channel + " : The command does not start with a '!', please revise and try again. ";
                bot.SendMessage(messageToSend);
            }
        }

        //
        // Delete a command. Called by !deleteCommand followed by the name. Should only be used by a moderator. 
        //
        public void DeleteCommand(String wholeChatMessage, BotData botData, int channelID)
        {
            // Extract the name of the command.
            String commandName = wholeChatMessage.Split(new char[] { ' ' }, 3)[1];

            // Delete the command.
            if (databaseCommands.DoesCommandExist(botData.channels[channelID].objectId, commandName))
            {
                int commandID = 0;
                String messageToSend = "";

                // Find the command ID
                for(int i = 0; i < botData.channels[channelID].channelCommands.Length; i++)
                {
                    if (botData.channels[channelID].channelCommands[i].commandName.Equals(commandName))
                        commandID = botData.channels[channelID].channelCommands[i].objectId;
                }

                // If we have found a result, delete the command using the identifer.
                // The identifer will never be 0, so we can use that as a condition. 
                if (commandID != 0)
                {
                    if(databaseCommands.DeleteCommand(commandID))
                    {
                        // Refresh the commands list. 
                        List<Command> commands = databaseCommands.GetCommands(botData.channels[channelID].objectId);
                        botData.channels[channelID].channelCommands = commands.ToArray();

                        // Alter the message. 
                        messageToSend = "PRIVMSG #" + channel + " : " + commandName + " has been deleted successfully.";
                    }
                    else
                        messageToSend = "PRIVMSG #" + channel + " : " + commandName + " has not been deleted. Please contact me, I want to find out why :(";
                }
                else
                {
                    Console.WriteLine("Error deleting command, Identifer mismatch.");
                }

                if (!messageToSend.Equals(""))
                {
                    // Send confirmation of success.
                    bot.SendMessage(messageToSend);
                }
                else
                    Console.WriteLine("Error deleting command, technical error. Most likely a command identifer mismatch.");
            }
            else
            {
                String messageToSend = "PRIVMSG #" + channel + " : " + commandName + " does not exist; it needs to be added before " +
                                                                                     "it can be deleted.";
                bot.SendMessage(messageToSend);
            }
        }
    }
}
