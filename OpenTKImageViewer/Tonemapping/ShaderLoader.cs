using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Tonemapping
{
    public delegate void ChangedValueHandler(object sender, EventArgs e);

    public class ShaderLoader
    {
        public enum ParameterType
        {
            Float,
            Int,
            Bool
        }

        public enum ModificationType
        {
            Add,
            Multiply,
            Set
        }

        /// <summary>
        /// tricky method to remove trailing 0's in a decimal
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static decimal Normalize(decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }

        public class Keybinding
        {
            public Keybinding(decimal value, ModificationType modType, System.Windows.Input.Key key)
            {
                this.Value = value;
                this.ModType = modType;
                this.Key = key;
            }

            /// <summary>
            /// applies the event bound to the keybinding on the parameter
            /// </summary>
            /// <parameter></parameter>
            /// <returns>true if the corresponding parameter was changed</returns>
            public bool Invoke(Parameter parameter)
            {
                var oldValue = parameter.CurrentValue;
                switch (ModType)
                {
                    case ModificationType.Add:
                        parameter.CurrentValue += Value;
                        break;
                    case ModificationType.Multiply:
                        parameter.CurrentValue *= Value;
                        break;
                    case ModificationType.Set:
                        parameter.CurrentValue = Value;
                        break;
                }
                parameter.CurrentValue = Normalize(parameter.CurrentValue);
                return oldValue != parameter.CurrentValue;
            }

            public decimal Value { get; }
            public ModificationType ModType { get; }
            public System.Windows.Input.Key Key { get; }
        }

        public class Parameter
        {
            public string Name { get; set; }
            public int Location { get; set; }
            public ParameterType Type { get; set; }
            public decimal Min { get; set; }
            public decimal Max { get; set; }
            public decimal Default { get; set; }
            public List<Keybinding> Keybindings { get; set; } = new List<Keybinding>();

            private decimal currentValue = 0;
            public decimal CurrentValue
            {
                get { return currentValue; }
                set
                {
                    var val = Math.Min(Max, Math.Max(Min, value));
                    var prevVal = currentValue;
                    currentValue = val;
                    if (prevVal != val)
                    {
                        OnValueChanged();
                    }
                    else if(value != prevVal)
                        // in order to clamp the numeric up down values correctly
                        OnValueChanged();
                }
            }

            public event ChangedValueHandler ValueChanged;
                
            /// <summary>
            /// deep copy of parameter (except the keybindings)
            /// </summary>
            /// <returns>deep copy</returns>
            public Parameter Clone()
            {
                return new Parameter
                {
                    Name = Name,
                    Location = Location,
                    Type = Type,
                    Min = Min,
                    Max = Max,
                    Default = Default,
                    currentValue = currentValue,
                    Keybindings = Keybindings
                };
            }

            public bool InvokeKey(System.Windows.Input.Key key)
            {
                bool changed = false;
                foreach (var binding in Keybindings)
                    if (binding.Key == key && binding.Invoke(this))
                        changed = true;
                return changed;
            }

            protected virtual void OnValueChanged()
            {
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string ShaderSource { get; private set; } = "";
        public List<Parameter> Parameters { get; private set; } = new List<Parameter>();
        public bool IsSepa { get; private set; }= false;
        public bool IsSingleInvocation { get; private set; } = true;
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Filename { get; }

        public ShaderLoader(string filename)
        {
            this.Filename = filename;
            Name = filename;
            Description = "";

            int lineNumber = 1;

            // Read the file and display it line by line.
            System.IO.StreamReader file =
                new System.IO.StreamReader(filename);

            try
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.StartsWith("#param"))
                    {
                        HandleParam(GetParameters(line.Substring("#param".Length)));
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
                        
                        whole = whole.TrimStart(' ');
                        whole = whole.TrimEnd(' ');

                        HandleSetting(parameters, whole);
                        ShaderSource += "\n"; // remember line for error information
                    }
                    else if(line.StartsWith("#keybinding"))
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
        }

        private void HandleKeybinding(string[] pars)
        {
            if (pars.Length < 4)
                throw new Exception("not enough arguments for #keybinding provided");

            var name = pars[0];
            Parameter matchingParam = null;
            // find parameter with the same name
            foreach (var parameter in Parameters)
            {
                if (parameter.Name == name)
                {
                    matchingParam = parameter;
                    break;
                }
            }
            if (matchingParam == null)
                throw new Exception("could not match keybinding with name: " + name + " to any parameter");

            decimal value = GetDecimalValue(pars[2], matchingParam.Type);

            ModificationType modType;
            switch(pars[3].ToLower())
            {
                case "add":
                    modType = ModificationType.Add;
                    break;
                case "multiply":
                    modType = ModificationType.Multiply;
                    break;
                case "set":
                    modType = ModificationType.Set;
                    break;
                default:
                    throw new Exception("invalid keybinding operation");
            }

            // try to parse key
            System.Windows.Input.Key key;
            if (!Enum.TryParse<System.Windows.Input.Key>(pars[1], out key))
                throw new Exception("could not match key in keybinding");

            // create new keybinding
            var binding = new Keybinding(value, modType, key);
            matchingParam.Keybindings.Add(binding);
        }

        private void HandleParam(string[] pars)
        {
            if(pars.Length < 3)
                throw new Exception("not enough arguments for #param provided");
            var p = new Parameter {Name = pars[0]};

            int location;
            if(!Int32.TryParse(pars[1], out location))
                throw new Exception("location must be a number");
            p.Location = location;

            if(pars[2].ToLower().Equals("float"))
                p.Type = ParameterType.Float;
            else if(pars[2].ToLower().Equals("int"))
                p.Type = ParameterType.Int;
            else if(pars[2].ToLower().Equals("bool"))
                p.Type = ParameterType.Bool;
            else throw new Exception("unknown parameter type " + pars[2]);

            p.Default = pars.Length >= 4 ? GetDecimalValue(pars[3], p.Type) : 0;

            p.Min = pars.Length >= 5 ? GetDecimalValue(pars[4], p.Type) : Decimal.MinValue;

            p.Max = pars.Length >= 6 ? GetDecimalValue(pars[5], p.Type) : Decimal.MaxValue;

            p.CurrentValue = p.Default;
            Parameters.Add(p);
        }

        private decimal GetDecimalValue(string argument, ParameterType type)
        {
            if (type == ParameterType.Bool)
                return argument.ToLower().Equals("true") ? 1 : 0;
            decimal val;
            if(!Decimal.TryParse(argument, NumberStyles.Any, App.GetCulture(), out val))
                throw new Exception("cannot convert arument to decimal");
            return val;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p">komma seperated parameters</param>
        /// <param name="wholeString">the second parameter wihtout seperation of kommas (for description)</param>
        private void HandleSetting(string[] p, string wholeString)
        {
            if(p.Length < 2)
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
                case "singleinvocation":
                    IsSingleInvocation = p[1].ToLower().Equals("true");
                    break;
                default:
                    throw new Exception("unknown setting " + p[0]);
            }
        }

        private static string[] GetParameters(string s)
        {
            string[] pars = s.Split(',');
            // remove some white spaces
            for (int i = 0; i < pars.Length; ++i)
            {
                pars[i] = pars[i].TrimStart(' ');
                pars[i] = pars[i].TrimEnd(' ');
            }

            return pars;
        }
    }
}
