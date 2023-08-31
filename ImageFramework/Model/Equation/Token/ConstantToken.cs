using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Equation.Token
{
    public class ConstantToken : ValueToken
    {
        private readonly string name;
        private readonly float value;

        public ConstantToken(string name, float value)
        {
            this.name = name;
            this.value = value;
        }

        public override string ToHlsl()
        {
            if (!float.IsInfinity(value) && !float.IsNaN(value)) 
                return $"f4({value.ToString(Models.Culture)})";
            
            // stuff into uint to keep representation
            uint res;
            float f = value;
            unsafe
            {
                float* pf = &f;
                res = *((uint*)pf);
            }
            // for reference:
            // NaN: asfloat( 4290772992u )
            // infinity: asfloat( 2139095040u )

            return $"f4(asfloat({res}u))";
        }

        public override float ToFloat()
        {
            return value;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
