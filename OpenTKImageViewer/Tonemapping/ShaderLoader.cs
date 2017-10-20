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

        public class Parameter
        {
            public string Name { get; set; }
            public int Location { get; set; }
            public ParameterType Type { get; set; }
            public decimal Min { get; set; }
            public decimal Max { get; set; }
            public decimal Default { get; set; }

            private decimal currentValue = 0;
            public decimal CurrentValue
            {
                get { return currentValue; }
                set
                {
                    var val = Math.Min(Max, Math.Max(Min, value));
                    if (currentValue != val)
                    {

                        currentValue = val;
                        OnValueChanged();
                    }
                    else if(value != val)
                        // in order to clamp the numeric up down values correctly
                        OnValueChanged();
                }
            }

            public event ChangedValueHandler ValueChanged;
                
            /// <summary>
            /// deep copy of parameter
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
                    currentValue = currentValue
                };
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
                        HandleSetting(GetParameters(line.Substring("#setting".Length)));
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
            if(!Decimal.TryParse(argument, NumberStyles.Any, new CultureInfo("en-US"), out val))
                throw new Exception("cannot convert arument to decimal");
            return val;
        }

        private void HandleSetting(string[] p)
        {
            if(p.Length < 2)
                throw new Exception("not enough arguments for #setting provided");
            switch (p[0].ToLower())
            {
                case "sepa":
                    IsSepa = p[1].ToLower().Equals("true");
                    break;
                case "title":
                    Name = p[1];
                    break;
                case "description":
                    Description = p[1];
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
