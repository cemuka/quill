using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace QuillLib
{
    public class Message
    {
        private event Action<MessageData> _event;

        public void Register(Action<MessageData> callback)
        {
            _event += callback;
        }

        public void Unregister(Action<MessageData> callback)
        {
            _event -= callback;
        }

        public void Post(MessageData data)
        {
            _event?.Invoke(data);
        }
    }

    public class MessageData
    {
        public string id;
        public Dictionary<string, object> container;
    }
}