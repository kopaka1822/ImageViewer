using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Utility;
using ImageViewer.Models;
using SharpDX.Direct2D1;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.ViewModels.Dialog
{
    public class ImportNpyViewModel : INotifyPropertyChanged
    {
        public static readonly int SelectedTextureType2D = 0;
        public static readonly int SelectedTextureType3D = 1;

        private readonly ModelsEx models;
        private int[] shape = null;

        private Size3? requiredSize;

        public struct InitStatus
        {
            public bool IsCompatible; // true if the shape can be adjusted for import
            public bool IsConfigurable; // false, if there is nothing left to configure (dialog is not required)
        }

        public ImportNpyViewModel(ModelsEx models)
        {
            this.models = models;
        }

        public InitStatus Init(string filename, int[] shape)
        {
            InitStatus status;
            status.IsCompatible = true;
            status.IsConfigurable = true;

            Filename = filename;
            this.shape = shape;
            // make comma separated string from shape
            Shape = "(" + string.Join(", ", shape) + ")";

            // check if there are size requirements
            if (models.Images.NumImages == 0)
            {
                // no size requirements
                requiredSize = null;
                SelectedTextureTypeEnabled = true; // can set type
                LastLayer = Int32.MaxValue; // set to max value, will be clamped by UpdateLayerIndices() later
                if(shape.Last() <= 4)
                {
                    UseRGBAEnabled = true; // let the user decide if he wants to import as RGB or grayscale
                    UseRGBA = true;
                }
                else
                {
                    UseRGBAEnabled = false;
                    UseRGBA = false;
                }
            }
            else
            {
                requiredSize = models.Images.GetSize(0);
                if (models.Images.NumLayers > 1)
                    requiredSize = new Size3(requiredSize.Value.X, requiredSize.Value.Y, models.Images.NumLayers);

                // select current texture type
                SelectedTextureTypeEnabled = false; // type is already set
                SelectedTextureType = models.Images.ImageType == typeof(Texture3D) ? SelectedTextureType3D : SelectedTextureType2D;

                // see if color channels can be used
                var shapeRGBA = CalcShape(true);
                var shapeGray = CalcShape(false);
                var rgbaCompatible = IsShapeCompatible(requiredSize, shapeRGBA);
                var grayCompatible = IsShapeCompatible(requiredSize, shapeGray);

                if (!rgbaCompatible && !grayCompatible)
                {
                    status.IsCompatible = false;
                    return status;
                }

                if(rgbaCompatible && grayCompatible)
                {
                    UseRGBAEnabled = true;
                    // it is configurable, since there is a choice
                    status.IsConfigurable = true;
                }
                else
                {
                    UseRGBAEnabled = false;
                    UseRGBA = rgbaCompatible;

                    // test if at least the layers need to be configured
                    status.IsConfigurable = CalcShape(UseRGBA).Z > requiredSize.Value.Z;
                }
            }

            UpdateLayerIndices();
            SetMaxCompatibleLayer();
            UpdatePreviewText();

            return status;
        }

        // applies settings to the image loader dll
        public void ApplySettings()
        {
            IO.SetGlobalParameter("npy is3D", SelectedTextureType == SelectedTextureType3D ? 1 : 0);
            IO.SetGlobalParameter("npy useChannel", UseRGBA ? 1 : 0);
            IO.SetGlobalParameter("npy firstLayer", FirstLayer);
            IO.SetGlobalParameter("npy lastLayer", LastLayer);
        }

        void UpdateLayerIndices()
        {
            var shape = CalcShape(UseRGBA);
            MaxLayerIndex = shape.Z - 1;
            OnPropertyChanged(nameof(MaxLayerIndex));

            FirstLayer = Utility.Clamp(FirstLayer, 0, MaxLayerIndex);
            LastLayer = Utility.Clamp(LastLayer, FirstLayer, MaxLayerIndex);
        }

        // returns current shape reduced to Size3 (width, height, depth/layers)
        private Size3 CurrentShape
        {
            get
            {
                var shape = CalcShape(UseRGBA);
                var z = Math.Max(1, LastLayer - FirstLayer + 1);
                return new Size3(shape.X, shape.Y, z);
            }
        }

        private Size3 CalcShape(bool useRGBChannels)
        {
            int startIdx = shape.Length - 1; // start from last element
            if (useRGBChannels) startIdx -= 1;

            int width = 1;
            int height = 1;
            int depth = 1;
            if (startIdx >= 0)
                width = shape[startIdx];
            if (startIdx >= 1)
                height = shape[startIdx - 1];
            for (int i = 0; i <= startIdx - 2; ++i)
                depth *= shape[i];

            return new Size3(width, height, depth);
        }

        // returns true if X and Y are precisely the same, and Z can be adjusted
        private bool IsShapeCompatible(Size3? requiredShape, Size3 currentShape)
        {
            if (!requiredShape.HasValue) return true;
            var rs = requiredShape.Value;
            if (rs.X != currentShape.X || rs.Y != currentShape.Y) return false;
            if (rs.Z > currentShape.Z) return false;
            return true;
        }

        private bool IsShapeCorrect(Size3? requiredShape, Size3 currentShape)
        {
            if (!requiredShape.HasValue) return true;
            var rs = requiredShape.Value;
            return rs == currentShape;
        }

        void SetMaxCompatibleLayer()
        {
            if(requiredSize.HasValue)
            {
                var rs = requiredSize.Value;
                LastLayer = FirstLayer + rs.Z - 1; // could be clamped
                FirstLayer = LastLayer - rs.Z + 1; // could be clamped
            }
            else
            {
                FirstLayer = 0;
                LastLayer = MaxLayerIndex;
            }
        }

        private int CurrentChannelCount
        {
            get
            {
                if(UseRGBA && shape.Length >= 1) return shape.Last();
                return 1;
            }
        }

        private void UpdatePreviewText()
        {
            var channelCount = CurrentChannelCount;
            var channelsText = "Grayscale";
            if (channelCount == 2) channelsText = "RG";
            else if (channelCount == 3) channelsText = "RGB";
            else if (channelCount == 4) channelsText = "RGBA";
            else if(channelCount != 1) Debug.Assert(false);

            var shape = CurrentShape;
            var layer = SelectedTextureType == SelectedTextureType2D ? "Layer" : "Depth";

            var text = $@"Preview:
Channels: {channelsText}
Width: {shape.X}
Height: {shape.Y}
{layer}: {shape.Z}
Number of available {layer} slices: {MaxLayerIndex + 1}";

            PreviewText = text;
            OnPropertyChanged(nameof(PreviewText));

            var warnings = "";
            if(shape.X > Device.MAX_TEXTURE_2D_DIMENSION)
            {
                warnings += $"The width exceeds the DirectX max required texture size of {Device.MAX_TEXTURE_2D_DIMENSION}.\n";
            }
            if(shape.Y > Device.MAX_TEXTURE_2D_DIMENSION)
            {
                warnings += $"The height exceeds the DirectX max required texture size of {Device.MAX_TEXTURE_2D_DIMENSION}.\n";
            }
            Debug.Assert(Device.MAX_TEXTURE_3D_DIMENSION == Device.MAX_TEXTURE_2D_ARRAY_DIMENSION);
            if(shape.Z > Device.MAX_TEXTURE_2D_ARRAY_DIMENSION)
            {
                warnings += $"The {layer} count exceeds the DirectX max required {layer} count of {Device.MAX_TEXTURE_2D_ARRAY_DIMENSION}.\n";
            }
            if (warnings.Length > 0)
                warnings += "This may prevent the creation of the texture resource!";

            if (!IsValid && requiredSize.HasValue)
                warnings += $"The Layer/Z count needs to match {requiredSize.Value.Z}! Current Value: {LastLayer - FirstLayer + 1}.";

            ExtraText = warnings;
            OnPropertyChanged(nameof(ExtraText));
        }

        public string Filename { get; set; }

        public string Shape { get; set; }

        public string ExtraText { get; private set; } = "";

        public string PreviewText { get; set; } = "Preview:";

        private bool useRGBA = true;
        public bool UseRGBA
        {
            get => useRGBA;
            set
            {
                if (value == useRGBA) return;
                useRGBA = value;
                OnPropertyChanged(nameof(UseRGBA));
                UpdateLayerIndices();
                SetMaxCompatibleLayer();
                UpdatePreviewText();
            }
        }

        public bool UseRGBAEnabled { get; set; } = true;

        private int selectedTextureType = SelectedTextureType2D;
        public int SelectedTextureType
        {
            get => selectedTextureType;
            set
            {
                if (selectedTextureType == value) return;
                selectedTextureType = value;
                OnPropertyChanged(nameof(SelectedTextureType));
                UpdatePreviewText();
            }
        }

        public bool SelectedTextureTypeEnabled { get; set; } = true;

        private int firstLayer = 0;
        public int FirstLayer
        {
            get => firstLayer;
            set
            {
                firstLayer = Utility.Clamp(value, 0, MaxLayerIndex);
                OnPropertyChanged(nameof(FirstLayer));
                OnPropertyChanged(nameof(IsValid));
                UpdatePreviewText();
            }
        }

        private int lastLayer = 0;
        public int LastLayer
        {
            get => lastLayer;
            set
            {
                lastLayer = Utility.Clamp(value, 0, MaxLayerIndex);
                OnPropertyChanged(nameof(LastLayer));
                OnPropertyChanged(nameof(IsValid));
                UpdatePreviewText();
            }
        }
        public int MaxLayerIndex { get; set; } = 0;

        public bool IsValid => IsShapeCorrect(requiredSize, CurrentShape) && LastLayer >= FirstLayer;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
