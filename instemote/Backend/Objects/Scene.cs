using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;

[DataContract]
public class Scene : INotifyPropertyChanged
{
    [DataMember] 
    public string ID;
    [DataMember] 
    public string GroupID;
    [DataMember] 
    private string _Name;

    public string Name
    {
        get { return _Name.Trim(); ; }
        set
        {
            _Name = value;
            NotifyPropertyChanged("Name");
        }
    }

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