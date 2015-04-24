using Core.Common.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (propertyChangedEvent != null)
            {
                propertyChangedEvent(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            string propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
            OnPropertyChanged(propertyName);
        }
    }
}
