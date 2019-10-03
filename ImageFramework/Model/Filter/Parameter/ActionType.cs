using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Filter.Parameter
{
    /// <summary>
    /// action that is performed by the numeric up and down box
    /// </summary>
    public enum ActionType
    {
        OnAdd, // number box up button
        OnSubtract, // number box down button
        Unknown
    }
}
