using StorageTool.Lib.Interfaces;
using System;

namespace StorageTool.Feedback
{
    class FancyFeedback : IUserFeedback
    {
        private int _windowWidth;
        private int _windowHeight;
        private int _windowXOffset;
        private int _windowYOffset;

        ConsoleColor _originalBackground;
        ConsoleColor _originalForeground;

        private object _token = new object();

        private int _objectCount = 0;
        private int _objectsDone = 0;
        private int _blocksOnScreen = 0;
        private string _title;

        private bool _drawBox;
        private int _progressRowOffset;
        private string _lastErrorObject = "";
        private string _lastErrorMessage = "";

        internal FancyFeedback()
        {
            _originalBackground = Console.BackgroundColor;
            _originalForeground = Console.ForegroundColor;
            ResetDimensions();
        }

        public void Init(int objectCount, string title)
        {
            _objectCount = objectCount;
            _title = title;
            RedrawBox();
        }

        public void Finished()
        {
            Console.BackgroundColor = _originalBackground;
            Console.ForegroundColor = _originalForeground;
            Console.Clear();
        }

        public void Error(string name, string error)
        {
            _lastErrorObject = name;
            _lastErrorMessage = error;
        }

        public void FolderDeleteFinished(string fullName, bool suppressMessage = false)
        {
            
        }

        public void FolderDeleteStarted(string fullName)
        {
            
        }

        public void FolderUploadFinished(string fullName, bool suppressMessage = false)
        {
            
        }

        public void ItemUploadFinished(string fullName)
        {
            
        }

        public void FolderUploadStarted(string fullName)
        {
            
        }

        public void ObjectDeleteFinished(string fullName, bool suppressMessage = false)
        {
            
        }

        public void ObjectDeleteStarted(string fullName)
        {
            
        }

        public void ObjectUploadFinished(string fullName, bool suppressMessage = false)
        {
            IncrementProgress();
        }

        public void ObjectUploadSkipped(string fullname)
        {
            IncrementProgress();
        }

        private void IncrementProgress()
        {
            lock (_token)
            {
                CheckDimensions();
                _objectsDone++;
                int _pbOffset = _drawBox ? 6 : 2;
                if ((_objectsDone * (_windowWidth - 6)) / _objectCount > _blocksOnScreen)
                {
                    _blocksOnScreen = (_objectsDone * (_windowWidth - 6)) / _objectCount;
                    DrawBlocks();
                }
            }
        }

        public void ObjectUploadStarted(string fullName)
        { 
            
        }

        private void CheckDimensions()
        {
            if (_windowHeight != Console.WindowHeight || _windowWidth != Console.WindowWidth || _windowXOffset != Console.WindowLeft || _windowYOffset != Console.WindowTop)
            {
                ResetDimensions();
                _blocksOnScreen = 0;
                RedrawBox();
            }
        }

        private void ResetDimensions()
        {
            _windowWidth = Console.WindowWidth;
            _windowHeight = Console.WindowHeight;
            _windowXOffset = Console.WindowLeft;
            _windowYOffset = Console.WindowTop;
            if (_windowHeight >= 12)
            {
                _drawBox = true;
                _progressRowOffset = 4;
            }
            else
            {
                _drawBox = false;
                _progressRowOffset = 1;
            }
        }

        private void RedrawBox()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.SetCursorPosition(_windowXOffset, _windowYOffset);
            if (_drawBox)
            {                
                string padString = new string(' ', _windowWidth - 2);
                Console.Write(new string('*', _windowWidth));
                Console.Write("*");
                Console.Write(padString);
                Console.Write($"** {_title}");
                if (_windowWidth > (_title.Length + 3))
                {
                    Console.Write(new string(' ', _windowWidth - (_title.Length + 3)));
                }
                Console.Write("**");
                Console.Write(padString);
                Console.Write("** [");
                Console.Write(new string(' ', _windowWidth - 6));
                Console.Write("] **");
                Console.Write(padString);
                Console.Write("*");
                Console.Write(new string('*', _windowWidth));
            }
            else
            {
                Console.WriteLine(_title);                
                Console.Write("[");
                Console.Write(new string(' ', _windowWidth - 2));
                Console.Write("]");
            }
            if (!string.IsNullOrEmpty(_lastErrorObject))
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(TruncateOrPadString(_lastErrorObject, _windowWidth));
                Console.WriteLine(TruncateOrPadString(_lastErrorMessage, _windowWidth));
            }
        }

        private void DrawBlocks()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Blue;
            int blockOffset = _drawBox ? 3 : 1;
            Console.SetCursorPosition(_windowXOffset + blockOffset, _windowYOffset + _progressRowOffset);
            Console.Write(new string('O', _blocksOnScreen));
        }

        private static string TruncateOrPadString(string input, int width)
        {
            if (input == null)
            {
                return null;
            }
            if (input.Length == width)
            {
                return input;
            }
            if (input.Length < width)
            {
                return input + new string(' ', input.Length - width);
            }

            const string ellipses = "...";
            const int ellipsesLength = 3;

            if (width > ellipsesLength + 10)
            {
                int firstPortion = (width - 3) / 2;
                return input.Substring(0, firstPortion) + ellipses + input.Substring(input.Length - (width - (firstPortion + ellipsesLength)));
            }
            if (width > ellipsesLength + 3)
            {
                return input.Substring(0, width - (ellipsesLength)) + ellipses;
            }
            return input.Substring(0, width);
        }
    }
}
