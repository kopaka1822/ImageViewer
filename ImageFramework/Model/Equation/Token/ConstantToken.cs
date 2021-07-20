using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Equation.Token
{
    public class ConstantToken : ValueToken
    {
        private readonly float value;

        public ConstantToken(float value)
        {
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
            throw new NotImplementedException("ConstantToken::ToFloat");
        }
    }
}
