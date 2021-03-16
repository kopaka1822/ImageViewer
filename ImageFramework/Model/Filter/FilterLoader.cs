using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ImageFramework.Model.Filter.Parameter;
using SharpDX.DXGI;

namespace ImageFramework.Model.Filter
{
    public class FilterLoader
    {
        public enum Type
        {
            Tex2D, // 2d kernel
            Tex3D, // 3d kernel
            Color, // single color kernel
            Dynamic // 2d and 3d kernel
        }

        public enum TargetType
        {
            Tex2D, // targets 2d textures
            Tex3D // targets 3d textures
        }

        public string ShaderSource { get; private set; } = "";
        public List<IFilterParameter> Parameters { get; } = new List<IFilterParameter>();
        public TargetType Target { get; }
        public Type KernelType { get; private set; } = Type.Tex2D;
        public bool IsSepa { get; private set; } = false;
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Filename { get; }

        // work group size per axis => 32 x 32 local threads (maximum)
        private int groupSize = Int32.MaxValue;
        public int GroupSize
        {
            get
            {
                if (groupSize != Int32.MaxValue) return groupSize;
                if (Target == TargetType.Tex2D) return 32;
                return 8;
            }
            private set
            {
                if (value < 1)
                    throw new Exception($"setting groupsize must be at least 1");
                if (Target == TargetType.Tex2D)
                {
                    if (GroupSize > 32)
                        throw new Exception($"setting groupsize cannot exceed 32 for 2D textures");
                }
                else
                {
                    if (GroupSize > 8)
                        throw new Exception($"setting groupsize cannot exceed 8 for 3D textures");
                }

                groupSize = value;
            }
        }

        public List<TextureFilterParameterModel> TextureParameters { get; } = new List<TextureFilterParameterModel>();

        private static readonly Regex variableRegex = new Regex(@"[a-zA-Z]([a-zA-Z0-9]*)");

        public FilterLoader(string filename, TargetType target)
        {
            Filename = filename;
            Target = target;
            Name = filename;
            Description = "";
            int lineNumber = 1;

            // Read the file and display it line by line.
            System.IO.StreamReader file =
                new System.IO.StreamReader(filename);

            try
            {
                bool insidePre = false; // inside preprocessor block
                bool maskOut = false; // mask out current preprocessor block

                string line;
                while ((line = file.ReadLine()) != null)
                {
                    // preprocessing with #if2D #if3D
                    if (line.StartsWith("#if2D"))
                    {
                        if(insidePre) throw new Exception("recursive #if2D/#if3D are not supported");
                        if (Target != TargetType.Tex2D) maskOut = true;
                        insidePre = true;
                        continue;
                    }
                    if (line.StartsWith("#if3D"))
                    {
                        if (insidePre) throw new Exception("recursive #if2D/#if3D are not supported");
                        if (Target != TargetType.Tex3D) maskOut = true;
                        insidePre = true;
                        continue;
                    }

                    if (insidePre && line.StartsWith("#else"))
                    {
                        maskOut = !maskOut;
                        continue;
                    }

                    if (insidePre && line.StartsWith("#endif"))
                    {
                        maskOut = false;
                        insidePre = false;
                        continue;
                    }

                    if(maskOut) continue;

                    // other preprocessing arguments
                    if (line.StartsWith("#paramprop"))
                    {
                        HandleParamprop(GetParameters(line.Substring("#paramprop".Length)));
                        ShaderSource += "\n"; // remember line for error information
                    }
                    else if (line.StartsWith("#param"))
                    {
                        HandleParam(GetParameters(line.Substring("#param".Length)));
                        ShaderSource += "\n"; // remember line for error information
                    }
                    else if (line.StartsWith("#texture"))
                    {
                        HandleTexture(GetParameters(line.Substring("#texture".Length)));
                        ShaderSource += "\n"; // remember line for error information
                    }
                    else if (line.StartsWith("#setting"))
                    {
                        var whole = line.Substring("#setting".Length);
                        var parameters = GetParameters(whole);
                        // get the second parameter as one string (without , seperation)
                        try
                        {
                            var idx = whole.IndexOf(",", StringComparison.Ordinal);
                            whole = whole.Substring(idx + 1);
                        }
                        catch (Exception)
                        {
                            // no second parameter available
                            whole = "";
                        }

                        whole = whole.Trim();

                        HandleSetting(parameters, whole);
                        ShaderSource += "\n"; // remember line for error information
                    }
                    else if (line.StartsWith("#keybinding"))
                    {
                        HandleKeybinding(GetParameters(line.Substring("#keybinding".Length)));
                        ShaderSource += "\n"; // remember line for error information
                    }
                    else
                    {
                        ShaderSource += line + "\n";
                    }
                    ++lineNumber;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + " at line " + lineNumber);
            }
            finally
            {
                file.Close();
            }

            // detect if correct type is targeted
            if(KernelType == Type.Tex3D && Target != TargetType.Tex3D)
                throw new Exception("This filter can only be used with 3D textures");
            if(KernelType == Type.Tex2D && Target != TargetType.Tex2D)
                throw new Exception("This filter can only be used with 2D textures");
        }

        private void HandleTexture(string[] pars)
        {
            if (pars.Length < 2)
                throw new Exception("not enough arguments for #texture provided");
            var name = pars[0];
            if (!variableRegex.IsMatch(pars[1]))
                throw new Exception("invalid function name (only alphanumeric characters allowed)");

            TextureParameters.Add(new TextureFilterParameterModel(name, pars[1]));
        }

        private void HandleParamprop(string[] pars)
        {
            if (pars.Length < 2)
                throw new Exception("not enough arguments for #paramprops provided");
            var matchingParam = FindMatchingParameter(pars[0]);

            if (!Enum.TryParse(pars[1], true, out ActionType atype))
                throw new Exception("unknown paramprops action " + pars[1]);
            switch (atype)
            {
                case ActionType.OnAdd:
                case ActionType.OnSubtract:
                    {
                        if (pars.Length < 4)
                            throw new Exception("not enough arguments for #paramprops provided");

                        var modType = GetModificationType(pars[3]);

                        switch (matchingParam.GetParamterType())
                        {
                            case ParameterType.Float:
                                AddParameterAction(matchingParam.GetFloatModel(), pars[2], modType, atype);
                                break;
                            case ParameterType.Int:
                            case ParameterType.Enum:
                                AddParameterAction(matchingParam.GetIntModel(), pars[2], modType, atype);
                                break;
                            case ParameterType.Bool:
                                AddParameterAction(matchingParam.GetBoolModel(), pars[2], modType, atype);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    break;
                default:
                    throw new Exception("unknown paramprops action " + pars[1]);
            }
        }

        private void AddParameterAction(IntFilterParameterModel model, string value, ModificationType modType, ActionType actionType)
        {
            var val = GetIntValue(value);
            var action = new IntFilterParameterModel.IntParameterAction(val, modType);
            model.Actions[actionType] = action;
        }

        private void AddParameterAction(FloatFilterParameterModel model, string value, ModificationType modType, ActionType actionType)
        {
            var val = GetFloatValue(value);
            var action = new FloatFilterParameterModel.FloatParameterAction(val, modType);
            model.Actions[actionType] = action;
        }

        private void AddParameterAction(BoolFilterParameterModel model, string value, ModificationType modType, ActionType actionType)
        {
            var val = GetBoolValue(value);
            var action = new BoolFilterParameterModel.BoolParameterAction(val, modType);
            model.Actions[actionType] = action;
        }

        private ModificationType GetModificationType(string type)
        {
            switch (type)
            {
                case "add":
                    return ModificationType.Add;
                case "multiply":
                    return ModificationType.Multiply;
                case "set":
                    return ModificationType.Set;
                default:
                    throw new Exception("invalid keybinding operation");
            }
        }

        private void HandleKeybinding(string[] pars)
        {
            if (pars.Length < 4)
                throw new Exception("not enough arguments for #keybinding provided");

            var matchingParam = FindMatchingParameter(pars[0]);

            var modType = GetModificationType(pars[3]);

            // try to parse key
            if (!Enum.TryParse<System.Windows.Input.Key>(pars[1], out var key))
                throw new Exception("could not match key in keybinding");

            switch (matchingParam.GetParamterType())
            {
                case ParameterType.Float:
                    AddKeybinding(matchingParam.GetFloatModel(), pars[2], modType, key);
                    break;
                case ParameterType.Int:
                case ParameterType.Enum:
                    AddKeybinding(matchingParam.GetIntModel(), pars[2], modType, key);
                    break;
                case ParameterType.Bool:
                    AddKeybinding(matchingParam.GetBoolModel(), pars[2], modType, key);
                    break;
            }
        }

        private void AddKeybinding(IntFilterParameterModel model, string value, ModificationType modType, Key key)
        {
            var val = GetIntValue(value);
            var binding = new IntFilterParameterModel.IntParameterAction(val, modType);
            model.Keybindings[key] = binding;
        }

        private void AddKeybinding(FloatFilterParameterModel model, string value, ModificationType modType, Key key)
        {
            var val = GetFloatValue(value);
            var binding = new FloatFilterParameterModel.FloatParameterAction(val, modType);
            model.Keybindings[key] = binding;
        }

        private void AddKeybinding(BoolFilterParameterModel model, string value, ModificationType modType, Key key)
        {
            var val = GetBoolValue(value);
            var binding = new BoolFilterParameterModel.BoolParameterAction(val, modType);
            model.Keybindings[key] = binding;
        }

        private void HandleParam(string[] pars)
        {
            if (pars.Length < 3)
                throw new Exception("not enough arguments for #param provided");

            if (!variableRegex.IsMatch(pars[1]))
                throw new Exception("invalid variable name (only alphanumeric characters allowed)");

            ParameterType type;
            if (pars[2].ToLower().Equals("float"))
                type = ParameterType.Float;
            else if (pars[2].ToLower().Equals("int"))
                type = ParameterType.Int;
            else if (pars[2].ToLower().Equals("bool"))
                type = ParameterType.Bool;
            else if (pars[2].ToLower().StartsWith("enum"))
                type = ParameterType.Enum;
            else throw new Exception("unknown parameter type " + pars[2]);

            switch (type)
            {
                case ParameterType.Float:
                    AddFloatParam(pars);
                    break;
                case ParameterType.Int:
                    AddIntParam(pars);
                    break;
                case ParameterType.Bool:
                    AddBoolParam(pars);
                    break;
                case ParameterType.Enum:
                    AddEnumParam(pars);
                    break;
            }
        }

        private void AddIntParam(string[] pars)
        {
            var def = pars.Length >= 4 ? GetIntValue(pars[3]) : 0;

            var min = pars.Length >= 5 ? GetIntValue(pars[4]) : Int32.MinValue;

            var max = pars.Length >= 6 ? GetIntValue(pars[5]) : Int32.MaxValue;

            Parameters.Add(new IntFilterParameterModel(pars[0], pars[1], min, max, def));
        }

        private void AddFloatParam(string[] pars)
        {
            var def = pars.Length >= 4 ? GetFloatValue(pars[3]) : 0.0f;

            var min = pars.Length >= 5 ? GetFloatValue(pars[4]) : Single.MinValue;

            var max = pars.Length >= 6 ? GetFloatValue(pars[5]) : Single.MaxValue;

            Parameters.Add(new FloatFilterParameterModel(pars[0], pars[1], min, max, def));
        }

        private void AddBoolParam(string[] pars)
        {
            var def = pars.Length >= 4 && GetBoolValue(pars[3]);

            Parameters.Add(new BoolFilterParameterModel(pars[0], pars[1], false, true, def));
        }

        private void AddEnumParam(string[] pars)
        {
            // check valid format: enum { A ; B ; C }
            var enumDef = pars[2].Substring("enum".Length);
            enumDef = enumDef.Trim();
            if(!enumDef.StartsWith("{") || !enumDef.EndsWith("}")) 
                throw new Exception("invalid enum syntax: " + pars[2]);
            var enumValues = new List<string>(GetParameters(enumDef.Substring(1, enumDef.Length - 2), ';'));

            var defIdx = 0;
            if (pars.Length >= 4)
            {
                var def = pars[3].Trim();
                defIdx = enumValues.IndexOf(def);
                if (defIdx < 0) throw new Exception($"enum default value '{def}' does not match with any values of {pars[2]}");
            }

            Parameters.Add(new EnumFilterParameterModel(pars[0], pars[1], enumValues, defIdx));
        }

        private bool GetBoolValue(string argument)
        {
            switch (argument.ToLower())
            {
                case "true":
                    return true;
                case "false":
                    return false;
            }
            throw new Exception("cannot convert argument to bool. expected either true or false");
        }

        private int GetIntValue(string argument)
        {
            if (!Int32.TryParse(argument, NumberStyles.Integer, Models.Culture, out var res))
                throw new Exception("cannot convert argument to int");
            return res;
        }

        private float GetFloatValue(string argument)
        {
            if (!Single.TryParse(argument, NumberStyles.Float, Models.Culture, out var res))
                throw new Exception("cannot convert argument to int");
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p">komma seperated parameters</param>
        /// <param name="wholeString">the second parameter wihtout seperation of kommas (for description)</param>
        private void HandleSetting(string[] p, string wholeString)
        {
            if (p.Length < 2)
                throw new Exception("not enough arguments for #setting provided");
            switch (p[0].ToLower())
            {
                case "sepa":
                    IsSepa = p[1].ToLower().Equals("true");
                    break;
                case "title":
                    Name = wholeString;
                    break;
                case "description":
                    Description = wholeString;
                    break;
                case "groupsize":
                    GroupSize = int.Parse(wholeString);
                    break;
                case "type":
                    switch (wholeString.ToUpper())
                    {
                        case "TEX2D":
                            KernelType = Type.Tex2D;
                            break;
                        case "TEX3D":
                            KernelType = Type.Tex3D;
                            break;
                        case "COLOR":
                            KernelType = Type.Color;
                            break;
                        case "DYNAMIC":
                            KernelType = Type.Dynamic;
                            break;
                        default:
                            throw new Exception("unknown type for settings");
                    }
                    break;
                default:
                    throw new Exception("unknown setting " + p[0]);
            }
        }

        private static string[] GetParameters(string s, char split = ',')
        {
            string[] pars = s.Split(split);
            // remove some white spaces
            for (int i = 0; i < pars.Length; ++i)
            {
                pars[i] = pars[i].Trim();
            }

            return pars;
        }

        private IFilterParameter FindMatchingParameter(string name)
        {
            foreach (var parameter in Parameters)
            {
                if (parameter.GetBase().Name == name)
                {
                    return parameter;
                }
            }
            throw new Exception("could not match keybinding with name: " + name + " to any parameter");
        }
    }
}
