using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageConsole
{
    /// <summary>
    /// helper class to extract values from a parameter list
    /// </summary>
    public class ParameterReader
    {
        private readonly List<string> args;
        private int curArg = 0;

        public ParameterReader(List<string> args)
        {
            this.args = args;
        }

        public string ReadString(string description, string defaultValue = null)
        {
            if (defaultValue != null && !HasMoreArgs()) return defaultValue;

            return ReadArg(description);
        }

        public int ReadInt(string description, int? defaultValue = null)
        {
            if (defaultValue != null & !HasMoreArgs()) return defaultValue.Value;

            var str = ReadArg(description);
            if (!int.TryParse(str, out var res))
            {
                throw new Exception($"could not convert \"{str}\" at index {curArg - 1} to integer");
            }

            return res;
        }

        public float ReadFloat(string description, float? defaultValue = null)
        {
            if (defaultValue != null && !HasMoreArgs()) return defaultValue.Value;

            var str = ReadArg(description);
            if (!float.TryParse(str, NumberStyles.Float, Models.Culture, out var res))
            {
                throw new Exception($"could not convert \"{str}\" at index {curArg - 1} to float");
            }

            return res;
        }

        public bool ReadBool(string description, bool? defaultValue = null)
        {
            if (defaultValue != null && !HasMoreArgs()) return defaultValue.Value;

            var str = ReadArg(description);
            if (str == "true") return true;
            if (str == "false") return false;
            throw new Exception($"could not convert \"{str}\" at index {curArg - 1} to bool");
        }

        public T ReadEnum<T>(string description, T? defaultValue = null) where T : struct, Enum
        {
            if (defaultValue != null && !HasMoreArgs()) return defaultValue.Value;

            var str = ReadArg(description);
            if (!Enum.TryParse<T>(str, true, out var res))
            {
                throw new Exception($"could not convert \"{str}\" at index {curArg - 1} to {typeof(T).Name}");
            }

            return res;
        }

        public bool HasMoreArgs()
        {
            return curArg < args.Count;
        }

        public void ExpectNoMoreArgs()
        {
            if (curArg > args.Count)
                throw new Exception($"too many arguments. Expected {curArg} but got {args.Count}");
        }

        private string ReadArg(string description)
        {
            if(!HasMoreArgs())
                throw new Exception($"too few arguments. Expected {description} at index {curArg}");
            return args[curArg++];
        }
    }
}
