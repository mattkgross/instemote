using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Insteon
{
    [DataContract]
    public class Device : INotifyPropertyChanged
    {
        [DataMember]
        public string ID;
        [DataMember]
        public string GroupID;
        [DataMember]
        public string InsteonID;

        private double _Percent = 0;
        public double Percent
        {
            get { return _Percent; }
            set
            {
                _Percent = value;
                NotifyPropertyChanged("Percent");
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                NotifyPropertyChanged("Name");
            }
        }

        //public string GetTwoPlacesFormattedGroupID()
        //{
        //    if (GroupID.Length == 1)
        //    {
        //        return "0" + GroupID;
        //    }
        //    return GroupID;
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
