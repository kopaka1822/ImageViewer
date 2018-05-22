namespace TextureViewer.Views
{
    public class ComboBoxItem<T>
    {
        private readonly string name;
        public T Cargo { get; }

        public ComboBoxItem(string name, T cargo)
        {
            this.name = name;
            Cargo = cargo;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
