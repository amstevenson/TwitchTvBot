using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForlornscTriviaBot.Entities;
using ForlornscTriviaBot.JsonParser;

namespace ForlornscTriviaBot.IRC
{
    public interface IProcessor
    {
        void Run();
    }

    public interface IWorker
    {
        void DoWork();
    }

    public interface ILogger
    {
        void Log(string logDetails, int level);
    }

    /// <summary>
    /// - A means of performing a process every x milliseconds.
    /// - Just need to change the interval and the worker injected (or the content of the method in the worker)
    /// - Remove the aTimer.Stop() and Start() if you want it to work every x milliseconds regardless of 
    ///   whether it has finished the previous process.
    /// </summary>
    class ThreadProcessor : IProcessor
    {
        IWorker _worker;
        ILogger _logger;
        int _intervalInMilliseconds = 1000;
        System.Timers.Timer _timer;
        bool _running;
        

        public ThreadProcessor(IWorker worker, ILogger logger, int intervalInMilliseconds)
        {
          _worker = worker;
          _logger = logger;
          _intervalInMilliseconds = intervalInMilliseconds;
        }

        public void Run()
        {
          _timer = new System.Timers.Timer();
          _timer.Elapsed += aTimer_Elapsed;
          _timer.Interval = _intervalInMilliseconds;
          _timer.Enabled = true;
          _timer.Start();

          // Use this if needing to run something at the same time as the timer. E.g a key enter for cancellation.
          _running = true;
          while (_running)
          {
              // Tell the thread that uses this processor to sleep for a second, to avoid CPU burnout.
              // Remove this line if I want to blow up my computer after around 10 mins or so...
             System.Threading.Thread.Sleep(1000);
          }
        }

        void aTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
          // Need to stop the timer, otherwise it will continue at it's set interval wether this process has finished or not.
          // Remove the stop if you want the event to fire every x seconds as opposed to x seconds after finish.
          _timer.Stop();

          //Main method to run
          RunProcess_WithExceptionHandling();

          // Start the timer back up
          _timer.Start();
        }


        public void RunProcess_WithExceptionHandling()
        {
          try
          {
              //_logger.Log("... ... Start of Trivia Check Process", 3);

            _worker.DoWork();

              // _logger.Log("... ... End of Trivia Check Process", 3);
          }
          catch (Exception ex)
          {
            // Log exception and/or send notification.
            _logger.Log(ex.ToString(), 0);
          }
        }
    }

    /// <summary>
    /// This is the class where you do what you want to do at the specified interval
    /// </summary>
    public class Worker : IWorker
    {
        ILogger _logger;
        BotData _botData;
        String _threadType;
        int _channelID;

        public Worker(ILogger logger, BotData botData, String threadType, int channelID)
        {
            _logger = logger;
            _botData = botData;
            _threadType = threadType;
            _channelID = channelID;
        }


        /// <summary>        
        /// *** THIS IS WHERE WE DO THE WORK AT THE SET INTERVAL. IWorker.DoWork is the only method we would need to change. ***
        /// No need for standard exception handling here. Can throw up to the RunProcessHandler.
        /// </summary>
        public void DoWork()
        {
            // Do work based on the type of thread we have.
            switch(_threadType)
            {
                case "trivia":
                    ProcessTrivia();
                    break;

                case "user":
                    ProcessUsers();
                    break;

                default: break;
            }
        }

        private void ProcessTrivia()
        {
            // The logic for determining whether or not a question has been set for the trivia or not.
            // If it hasen't been, a new question will be set. 
            if (_botData.triviaActive)
            {
                _logger.Log("Trivia is now set", 1);

                // If question is not set, then get a new random question.
                // Needs to also keep track of all the ones that have been asked.
                // A queue has been added that consists of the questions for a bot in 
                // a certain order. 
                if (_botData.questionActive)
                {
                    // If questions active, then query the time remaining.
                    // If the allocated time is elapsed, then send a message informing
                    // the chat and go to the next question.



                }
                else
                {
                    // If no question has been set, this will be done here.

                }

            }
        }

        private void ProcessUsers()
        {

            _logger.Log("Updating channel users ", 1);

            // Update the list of users(moderators)/those that are able to use elevated commands,
            // such as !deletecommand etc. 
            string url = "http://tmi.twitch.tv/group/user/" + _botData.channels[_channelID].channelName.ToLower() + "/chatters";
            string method = "get";

            // Create a json parser 
            JsonParser.JsonParser jsonParser = new JsonParser.JsonParser();

            // Retrieve the results
            Dictionary<string, object> returnDictionary = jsonParser.GetWebResponseDictionary(url, method);

            // Create a results object based on the response of the distributed API
            Chat returnModerators = jsonParser.ConvertDictionaryToResults(returnDictionary);

            // Add the moderators to the BotData list
            _botData.channels[_channelID].channelViewers = returnModerators;
            _botData.channels[_channelID].channelViewers.numModerators = returnModerators.channelModerators.Length;

            Console.WriteLine("Updating moderators has been triggered.");
        }
    }

    public class Logger : ILogger
    {
        public void Log(string logDetails, int level)
        {
            if (level == 0)
                Console.WriteLine(DateTime.Now.ToString("yyyy-mm-dd H:mm:ss") + ":EXCEPTION -" + logDetails);
            else
                Console.WriteLine(DateTime.Now.ToString("yyyy-mm-dd H:mm:ss") + ": " + logDetails);
        }

    }
}
