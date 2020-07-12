using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Utility;
using ImageViewer.Models;

namespace ImageViewer.UtilityEx
{
    public class CropManager : INotifyPropertyChanged
    {
        private Float3 cropStartf = Float3.One;

        /// <summary>
        /// crop start in relative coordinates [0, 1]
        /// CropStart.ToPixel is the first included pixel
        /// </summary>
        public Float3 CropStart
        {
            get => cropStartf;
            set
            {
                Debug.Assert((value >= Float3.Zero).AllTrue());
                Debug.Assert((value <= Float3.One).AllTrue());
                if (value == cropStartf) return;
                cropStartf = value;
                OnPropertyChanged(nameof(CropStart));

                if ((cropStartf > cropEndf).AnyTrue())
                {
                    CropEnd = new Float3(
                        Math.Max(cropStartf.X, cropEndf.X),
                        Math.Max(cropStartf.Y, cropEndf.Y),
                        Math.Max(cropStartf.Z, cropEndf.Z)
                    );
                }
            }
        }

        private Float3 cropEndf = Float3.One;

        /// <summary>
        /// crop end in relative coordinates [0, 1]
        /// CropEnd.ToPixel is the last included pixel.
        /// CropStart == CropEnd => exactly one pixel will be exported
        /// </summary>
        public Float3 CropEnd
        {
            get => cropEndf;
            set
            {
                Debug.Assert((value >= Float3.Zero).AllTrue());
                Debug.Assert((value <= Float3.One).AllTrue());
                if (value == cropEndf) return;
                cropEndf = value;
                OnPropertyChanged(nameof(CropEnd));

                if ((cropEndf < cropStartf).AnyTrue())
                {
                    CropStart = new Float3(
                        Math.Min(cropStartf.X, cropEndf.X),
                        Math.Min(cropStartf.Y, cropEndf.Y),
                        Math.Min(cropStartf.Z, cropEndf.Z)
                    );
                }
            }
        }

        public void SetMaxCropping()
        {
            CropStart = Float3.Zero;
            CropEnd = Float3.One;
        }

        public ViewModel GetViewModel(ModelsEx models)
        {
            return new ViewModel(models, this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public class ViewModel : INotifyPropertyChanged, IDisposable
        {
            private readonly ModelsEx models;
            private readonly CropManager parent;
            private Size3 size;
            private int mipmap = 0;

            internal ViewModel(ModelsEx models, CropManager parent)
            {
                this.parent = parent;
                this.size = models.Images.Size;
                this.models = models;
                parent.PropertyChanged += ParentOnPropertyChanged;
                models.Settings.PropertyChanged += SettingsOnPropertyChanged;
            }

            private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(SettingsModel.FlipYAxis):
                        OnPropertyChanged(nameof(CropStartY));
                        OnPropertyChanged(nameof(CropEndY));
                        break;
                }
            }

            public void Dispose()
            {
                parent.PropertyChanged -= ParentOnPropertyChanged;
                models.Settings.PropertyChanged -= SettingsOnPropertyChanged;
            }

            private void ParentOnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(CropManager.CropStart):
                    case nameof(CropManager.CropEnd):
                        OnPropertyChanged(nameof(CropStartX));
                        OnPropertyChanged(nameof(CropStartY));
                        OnPropertyChanged(nameof(CropStartZ));
                        OnPropertyChanged(nameof(CropEndX));
                        OnPropertyChanged(nameof(CropEndY));
                        OnPropertyChanged(nameof(CropEndZ));
                        break;
                }
            }

            public int Mipmap
            {
                get => mipmap;
                set
                {
                    Debug.Assert(mipmap >= -1);
                    if (mipmap == value) return;
                    mipmap = value;
                    
                    OnPropertyChanged(nameof(CropMaxX));
                    OnPropertyChanged(nameof(CropMaxY));
                    OnPropertyChanged(nameof(CropMaxZ));

                    OnPropertyChanged(nameof(CropStartX));
                    OnPropertyChanged(nameof(CropStartY));
                    OnPropertyChanged(nameof(CropStartZ));
                    OnPropertyChanged(nameof(CropEndX));
                    OnPropertyChanged(nameof(CropEndY));
                    OnPropertyChanged(nameof(CropEndZ));
                }
            }

            public int CropMinX => 0;
            public int CropMaxX => size.GetMip(Math.Max(Mipmap, 0)).Width - 1;
            public int CropMinY => 0;
            public int CropMaxY => size.GetMip(Math.Max(Mipmap, 0)).Height - 1;

            public int CropMinZ => 0;
            public int CropMaxZ => size.GetMip(Math.Max(Mipmap, 0)).Depth - 1;

            private Size3 curDim => size.GetMip(Math.Max(mipmap, 0));

            public int CropStartX
            {
                get => parent.CropStart.ToPixels(curDim).X;
                set
                {
                    var clamped = Utility.Clamp(value, CropMinX, CropMaxX);
                    SetCropStart(clamped, 0);

                    if (clamped != value) OnPropertyChanged(nameof(CropStartX));
                    CropEndX = CropEndX; // maybe adjust this value
                }
            }

            public int CropEndX
            {
                get => parent.CropEnd.ToPixels(curDim).X;
                set
                {
                    var clamped = Utility.Clamp(value, CropMinX, CropMaxX);
                    SetCropEnd(clamped, 0);

                    if (clamped != value) OnPropertyChanged(nameof(CropEndX));
                }
            }

            public int CropStartY
            {
                get => models.Settings.FlipYAxis ? FlipY(parent.CropEnd.ToPixels(curDim).Y) : parent.CropStart.ToPixels(curDim).Y;
                set
                {
                    var clamped = Utility.Clamp(value, CropMinY, CropMaxY);
                    if (models.Settings.FlipYAxis)
                    {
                        // set crop end y
                        SetCropEnd(FlipY(clamped), 1);
                    }
                    else
                    {
                        // set crop start y
                        SetCropStart(clamped, 1);
                    }

                    if (clamped != value) OnPropertyChanged(nameof(CropStartY));
                }
            }

            public int CropEndY
            {
                get => models.Settings.FlipYAxis ? FlipY(parent.CropStart.ToPixels(curDim).Y) : parent.CropEnd.ToPixels(curDim).Y;
                set
                {
                    var clamped = Utility.Clamp(value, CropMinY, CropMaxY);
                    if (models.Settings.FlipYAxis)
                    {
                        // set crop start y
                        SetCropStart(FlipY(clamped), 1);
                    }
                    else
                    {
                        SetCropEnd(clamped, 1);
                    }

                    if (clamped != value) OnPropertyChanged(nameof(CropEndY));
                }
            }

            public int CropStartZ
            {
                get => parent.CropStart.ToPixels(curDim).Z;
                set
                {
                    var clamped = Utility.Clamp(value, CropMinZ, CropMaxZ);
                    SetCropStart(clamped, 2);

                    if (clamped != value) OnPropertyChanged(nameof(CropStartZ));
                    CropEndZ = CropEndZ; // maybe adjust this value
                }
            }

            public int CropEndZ
            {
                get => parent.CropEnd.ToPixels(curDim).Z;
                set
                {
                    var clamped = Utility.Clamp(value, CropMinZ, CropMaxZ);
                    SetCropEnd(clamped, 2);

                    if (clamped != value) OnPropertyChanged(nameof(CropEndZ));
                }
            }

            private int FlipY(int value)
            {
                return CropMaxY - value;
            }

            private void SetCropStart(int v, int axis)
            {
                var res = parent.CropStart;
                res[axis] = (v + 0.5f) / curDim[axis];
                parent.CropStart = res;
            }

            private void SetCropEnd(int v, int axis)
            {
                var res = parent.CropEnd;
                res[axis] = (v + 0.5f) / curDim[axis];
                parent.CropEnd = res;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
