﻿using System;
using System.Configuration;
using Module2HW6.Factory;

namespace Module2HW6.Logger
{
    public sealed class Logger
    {
        private const string DEFAULT_PATH = "system.log";
        private const string DEFAULT_BATCH_SIZE = "100";

        private enum Level : byte
        {
            Info = 1,
            Warn = 2,
            Error = 3,
            Debug = 4
        }

        private int _counter = 0;
        private int _batchSize;
        private LogItem[] _logItems;

        private LogItemFactory _factory;

        private readonly string _path;

        private static Logger? _instance;

        private Logger()
        {
            _path = ConfigurationManager.AppSettings
                                        .Get("system_log") ?? DEFAULT_PATH;
            _factory   = new LogItemFactory();
            _batchSize = Convert.ToInt32(
                ConfigurationManager.AppSettings.Get("batch_size") ?? DEFAULT_BATCH_SIZE
            );
            _logItems  = new LogItem[_batchSize];
        }

        ~Logger()
        {
            Flush();
        }

        public static Logger GetInstance()
        {
            if (null == _instance)
            {
                _instance = new Logger();
            }

            return _instance;
        }

        public void Info(string Message)
        {
            Log(Level.Info, Message);
        }

        public void Warning(string Message)
        {
            Log(Level.Warn, Message);
        }

        public void Error(string Message)
        {
            Log(Level.Error, Message);
        }

        public void Debug(string Message)
        {
            Log(Level.Debug, Message);
        }

        private void Log(Level Level, string Message, Object? Context = null)
        {
            string type;

            switch (Level)
            {
                case Level.Info:
                {
                    type = "INFO";
                    break;
                }
                case Level.Warn:
                {
                    type = "WARN";
                    break;
                }
                case Level.Error:
                {
                    type = "ERROR";
                    break;
                }
                case Level.Debug:
                {
                    type = "DEBUG";
                    break;
                }
                default:
                {
                    type = "DEBUG: ";
                    break;
                 }
            }

            try
            {
                _logItems[_counter] = _factory.Create(type, Message);
            }
            catch (System.IndexOutOfRangeException e)
            {
                Flush();
                _logItems[_counter] = _factory.Create(type, Message);
            }

#if DEBUG
            Console.WriteLine(_logItems[_counter]);
#endif
            _counter++;
        }

        public void Flush()
        {
            _counter = 0;

            foreach (LogItem logItem in _logItems)
            {
                if (null == logItem) {
                    break;
                }

                File.AppendAllText(_path, logItem.ToString());
            }

            _logItems = new LogItem[_batchSize];
        }
    }
}
