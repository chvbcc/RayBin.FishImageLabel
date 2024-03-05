using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace RayBin.FishImageLabel
{
    public class FileSystemItemInfo : INotifyPropertyChanged
    {
        //show folder name
        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        //folder full path
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }


        private ImageSource nodeIcon;
        public ImageSource NodeIcon
        {
            get => nodeIcon;
            set
            {
                nodeIcon = value;
                OnPropertyChanged(nameof(NodeIcon));
            }
        }

        private ObservableCollection<FileSystemItemInfo> children;
        public ObservableCollection<FileSystemItemInfo> Children
        {
            get => children;
            set
            {
                children = value;
                OnPropertyChanged(nameof(Children));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}