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

namespace ImageViewer.ViewModels.Dialog
{
    public class Tex3DToArrayViewModel : INotifyPropertyChanged
    {
        private readonly Size3 dim;

        public Tex3DToArrayViewModel(Size3 dim)
        {
            this.dim = dim;
            selectedAxis = AxisList[0];
            lastSlice = NumSlices - 1;
        }

        public List<ListItemViewModel<int>> AxisList { get; } = new List<ListItemViewModel<int>>
        {
            new ListItemViewModel<int>
            {
                Name = "XY Plane",
                Cargo = 2
            },
            new ListItemViewModel<int>
            {
                Name = "XZ Plane",
                Cargo = 1
            },
            new ListItemViewModel<int>
            {
                Name = "YZ Plane",
                Cargo = 0
            },
        };

        private ListItemViewModel<int> selectedAxis;
        public ListItemViewModel<int> SelectedAxis
        {
            get => selectedAxis;
            set
            {
                if (value == null || ReferenceEquals(value, selectedAxis)) return;
                bool lastWasMax = (LastSlice + 1 == NumSlices);
                selectedAxis = value;
                OnPropertyChanged(nameof(SelectedAxis));
                OnPropertyChanged(nameof(NumSlices));

                FirstSlice = Math.Min(FirstSlice, NumSlices - 1);
                if (lastWasMax)
                    LastSlice = NumSlices - 1;
                else
                    LastSlice = Math.Min(LastSlice, NumSlices - 1);
                OnPropertyChanged(nameof(IsValid));
            }
        }

        // helper
        public int FixedAxisSlice => selectedAxis.Cargo;

        public int FreeAxis1 => FixedAxisSlice == 0 ? 1 : 0;
        public int FreeAxis2 => FixedAxisSlice == 2 ? 1 : 2;

        public int NumSlices => dim[FixedAxisSlice];

        private int firstSlice = 0;

        public int FirstSlice
        {
            get => firstSlice;
            set
            {
                if(value == firstSlice) return;
                Debug.Assert(value < NumSlices);
                firstSlice = value;
                OnPropertyChanged(nameof(FirstSlice));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private int lastSlice = 0;
        public int LastSlice
        {
            get => lastSlice;
            set
            {
                if (value == lastSlice) return;
                Debug.Assert(value < NumSlices);
                lastSlice = value;
                OnPropertyChanged(nameof(LastSlice));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid => FirstSlice >= 0 && FirstSlice < NumSlices && LastSlice >= 0 && LastSlice < NumSlices &&
                               FirstSlice <= LastSlice;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
