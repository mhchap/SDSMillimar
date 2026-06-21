using System;
using System.Collections.Generic;

namespace SDSMillimar.Utils
{
    public static class Messenger
    {
        private static readonly Dictionary<string, List<Action<object>>> _actions
            = new Dictionary<string, List<Action<object>>>();

        #region Register

        // 泛型注册（推荐用这个）
        public static void Register<T>(string message, Action<T> callback)
        {
            if (!_actions.ContainsKey(message))
                _actions[message] = new List<Action<object>>();

            // 包一层，保证类型安全
            Action<object> wrapper = obj =>
            {
                if (obj is T t)
                    callback(t);
            };

            _actions[message].Add(wrapper);
        }

        // 非泛型注册（保留，兼容老代码）
        public static void Register(string message, Action<object> callback)
        {
            if (!_actions.ContainsKey(message))
                _actions[message] = new List<Action<object>>();

            _actions[message].Add(callback);
        }

        #endregion

        #region Send

        public static void Send<T>(string message, T parameter)
        {
            if (!_actions.ContainsKey(message))
                return;

            foreach (var action in _actions[message])
            {
                action(parameter);
            }
        }

        public static void Send(string message, object parameter = null)
        {
            if (!_actions.ContainsKey(message))
                return;

            foreach (var action in _actions[message])
            {
                action(parameter);
            }
        }

        #endregion

        #region Unregister

        // ⚠ 注意：泛型 Register 的 Unregister 不能直接用 Action<T>
        // 所以这里保留原始版本
        public static void Unregister(string message, Action<object> callback)
        {
            if (_actions.ContainsKey(message))
            {
                _actions[message].Remove(callback);
            }
        }

        #endregion
    }
}
