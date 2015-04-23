using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Core
{
    public class NotificationObject : INotifyPropertyChanged
    {
        private event PropertyChangedEventHandler propertyChangedEvent;

        protected List<PropertyChangedEventHandler> propertyChangedSubcribers = new List<PropertyChangedEventHandler>();

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (!propertyChangedSubcribers.Contains(value))
                {
                    propertyChangedEvent += value;
                    propertyChangedSubcribers.Add(value);
                }
            }
            remove
            {
                propertyChangedEvent -= value;
                propertyChangedSubcribers.Remove(value);
            }
        }
    }
}
