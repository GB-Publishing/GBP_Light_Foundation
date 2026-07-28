using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static UnityEngine.PlayerPrefs;

namespace Gamebee.RemoteConfig
{
    public class RCVariable<T>
    {
        private readonly string _key;
        private T _value;

        internal Parser<T> Parser;

        internal RCVariable(string key, T defaultValue)
        {
            _key = key;

            Parser = Parser<T>.Get();
            _value = Parser.LoadFromCache(_key, defaultValue);
            RemoteConfigStore.ConfigDataChanged += OnConfigDataChanged;
        }

        public virtual T Value
        {
            get => _value;
            protected set
            {
                _value = value;
                Parser.SaveToCache(_key, value);
            }
        }

        private void OnConfigDataChanged(JObject data)
        {
            var token = data[_key];
            if (token == null) return;

            Value = token.Value<T>();
        }

        public static implicit operator T(RCVariable<T> variable) => variable.Value;
    }

    public abstract class Parser<T>
    {
        public abstract T LoadFromCache(string key, T defaultValue);
        public abstract void SaveToCache(string key, T value);

        public static Parser<T> Get() =>
            typeof(T).Name switch
            {
                "Int32" => new IntParser() as Parser<T>,
                "Single" => new FloatParser() as Parser<T>,
                "String" => new StringParser() as Parser<T>,
                "Boolean" => new BoolParser() as Parser<T>,
                _ => throw new ArgumentOutOfRangeException()
            };
    }

    public class IntParser : Parser<int>
    {
        public override int LoadFromCache(string key, int defaultValue) => GetInt(key, defaultValue);
        public override void SaveToCache(string key, int value) => SetInt(key, value);
    }

    public class FloatParser : Parser<float>
    {
        public override float LoadFromCache(string key, float defaultValue) => GetFloat(key, defaultValue);
        public override void SaveToCache(string key, float value) => SetFloat(key, value);
    }

    public class StringParser : Parser<string>
    {
        public override string LoadFromCache(string key, string defaultValue) => GetString(key, defaultValue);
        public override void SaveToCache(string key, string value) => SetString(key, value);
    }

    public class BoolParser : Parser<bool>
    {
        public override bool LoadFromCache(string key, bool defaultValue) => GetInt(key, defaultValue ? 1 : 0) == 1;
        public override void SaveToCache(string key, bool value) => SetInt(key, value ? 1 : 0);
    }
}