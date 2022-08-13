using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;

namespace Server.DTOs
{
    public class InformationCardReadDto :INotifyPropertyChanged
    {
        public int Index { get; set; }
        public Brush Image { get; set; }
        public string Name { get; set; }




		private bool _IsSelected;
        public bool IsSelected { get { return _IsSelected; } set { _IsSelected = value; OnChanged("IsSelected"); } }



        public event PropertyChangedEventHandler PropertyChanged;
        private void OnChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

    }
}
