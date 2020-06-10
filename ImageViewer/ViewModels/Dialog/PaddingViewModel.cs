using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using ImageFramework.Annotations;
using ImageFramework.DirectX;

namespace ImageViewer.ViewModels.Dialog
{
    public class PaddingViewModel : INotifyPropertyChanged
    {
        private readonly ImageFramework.Model.Models models;

        public string Resolution
        {
            get
            {
                var size = models.Images.Size;
                string res = $"{Left + Right + size.Width}x{Top + Bottom + size.Height}";
                if (models.Images.Is3D)
                    res += $"x{Front + Back + size.Depth}";

                return res;
            }
        }

        public bool IsValid
        {
            get
            {
                var size = models.Images.Size;
                var max = models.Images.Is3D ? Device.MAX_TEXTURE_3D_DIMENSION : Device.MAX_TEXTURE_2D_DIMENSION;

                if (Left + Right + size.Width > max) return false;
                if (Top + Bottom + size.Height > max) return false;

                if (Is3D && (Front + Back + size.Depth) > max) return false;

                return true;
            }
        }

        public bool Is3D => models.Images.Is3D;

        private int left = 0;
        public int Left
        {
            get => left;
            set
            {
                if (value == left) return;
                left = value;
                OnPropertyChanged(nameof(Left));
                OnPropertyChanged(nameof(Resolution));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private int right = 0;

        public int Right
        {
            get => right;
            set
            {
                if (value == right) return;
                right = value;
                OnPropertyChanged(nameof(Right));
                OnPropertyChanged(nameof(Resolution));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private int top = 0;

        public int Top
        {
            get => top;
            set
            {
                if(value == top) return;
                top = value;
                OnPropertyChanged(nameof(Top));
                OnPropertyChanged(nameof(Resolution));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private int bottom = 0;

        public int Bottom
        {
            get => bottom;
            set
            {
                if (value == bottom) return;
                bottom = value;
                OnPropertyChanged(nameof(Bottom));
                OnPropertyChanged(nameof(Resolution));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private int front = 0;

        public int Front
        {
            get => front;
            set
            {
                if(value == front) return;
                front = value;
                OnPropertyChanged(nameof(Front));
                OnPropertyChanged(nameof(Resolution));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private int back = 0;

        public int Back
        {
            get => back;
            set
            {
                if(value == back) return;
                back = value;
                OnPropertyChanged(nameof(Back));
                OnPropertyChanged(nameof(Resolution)); 
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public PaddingViewModel(ImageFramework.Model.Models models)
        {
            this.models = models;
            SelectedFill = AvailableFills[0];
        }

        public List<ListItemViewModel<int>> AvailableFills { get; } = new List<ListItemViewModel<int>>
        {
            new ListItemViewModel<int>{Cargo = 0, Name = "Black", ToolTip = ""},
            new ListItemViewModel<int>{Cargo = 1, Name = "White", ToolTip = ""},
            new ListItemViewModel<int>{Cargo = 0, Name = "Transparent", ToolTip = ""},
            new ListItemViewModel<int>{Cargo = 0, Name = "Clamp", ToolTip = "Use nearest pixel color from image"},
        };

        public ListItemViewModel<int> SelectedFill { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
