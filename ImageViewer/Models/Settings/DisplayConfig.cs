using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Models.Settings
{
    public class DisplayConfig
    {
        public bool LinearInterpolation { get; set; }
        public bool DisplayNegative { get; set; }
        public SettingsModel.AlphaType AlphaBackground { get; set; }
        public int TexelRadius { get; set; }
        public int TexelDecimalPlaces { get; set; }
        public SettingsModel.TexelDisplayMode TexelDisplayMode { get; set; }

        public static DisplayConfig LoadFromModels(ModelsEx models)
        {
            var res = new DisplayConfig();
            res.LinearInterpolation = models.Display.LinearInterpolation;
            res.DisplayNegative = models.Display.DisplayNegative;
            res.AlphaBackground = models.Settings.AlphaBackground;
            
            // pixel display
            res.TexelRadius = models.Display.TexelRadius;
            res.TexelDecimalPlaces = models.Settings.TexelDecimalPlaces;
            res.TexelDisplayMode = models.Settings.TexelDisplay;

            return res;
        }

        public void ApplyToModels(ModelsEx models)
        {
            models.Display.LinearInterpolation = LinearInterpolation;
            models.Display.DisplayNegative = DisplayNegative;
            models.Settings.AlphaBackground = AlphaBackground;

            models.Display.TexelRadius = TexelRadius;
            models.Settings.TexelDecimalPlaces = TexelDecimalPlaces;
            models.Settings.TexelDisplay = TexelDisplayMode;
        }
    }
}
