using Core.Common.Contracts;
using Core.Common.Extensions;
using Core.Common.Utils;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Core
{
    public abstract class ObjectBase : NotificationObject, IDirtyCapable, IExtensibleDataObject, IDataErrorInfo
    {
        private bool isDirty = false;
        private IValidator validator = null;
        private IEnumerable<ValidationFailure> validationErrors = null;

        public ObjectBase()
        {
            validator = GetValidator();
            Validate();
        }

        #region IDirtyCapable Members
        [NotNavigable]
        public bool IsDirty
        {
            get 
            {
                return isDirty;
            }
            protected set
            {
                isDirty = value;
                OnPropertyChanged(() => IsDirty, false);
            }
        }

        public bool IsAnythingDirty()
        {
            bool isDirty = false;

            WalkObjectGraph(
            o =>
            {
                if (o.IsDirty)
                {
                    isDirty = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }, coll => { });

            return isDirty;    
        }

        public List<IDirtyCapable> GetDirtyObjects()
        {
            List<IDirtyCapable> dirtyObjects = new List<IDirtyCapable>();

            WalkObjectGraph(
            o =>
            {
                if (o.IsDirty)
                {
                    dirtyObjects.Add(o);
                }
                return false;
            }, coll => { });

            return dirtyObjects;
        }

        public void CleanAll()
        {
            WalkObjectGraph(
            o =>
            {
                if (o.IsDirty)
                {
                    o.IsDirty = false;
                }
                return false;
            }, coll => { });
        }
        #endregion IDirtyCapable Members

        #region IExtensibleDataObject Members
        public ExtensionDataObject ExtensionData { get; set; }        
        #endregion IExtensibleDataObject Members

        #region IDataErrorInfo Members
        public string Error
        {
            get { return string.Empty; }
        }

        public string this[string columnName]
        {
            get 
            {
                StringBuilder errors = new StringBuilder();
                if (validationErrors != null && validationErrors.Count() > 0)
                {
                    foreach (ValidationFailure validationError in validationErrors)
                    {
                        if (validationError.PropertyName == columnName)
                        {
                            errors.AppendLine(validationError.ErrorMessage);
                        }
                    }
                }
                return errors.ToString();
            }
        }
        #endregion IDataErrorInfo Members

        #region Validation
        protected virtual IValidator GetValidator()
        {
            return null;
        }

        [NotNavigable]
        public IEnumerable<ValidationFailure> ValidationErrors 
        {
            get { return validationErrors; }
            set { } 
        }

        public void Validate()
        {
            if (validator != null)
            {
                ValidationResult result = validator.Validate(this);
                validationErrors = result.Errors;
            }
        }

        [NotNavigable]
        public bool IsValid
        {
            get 
            {
                return validationErrors == null || validationErrors.Count() == 0;
            }
        }
        #endregion Validation

        #region Protected Methods
        protected void WalkObjectGraph(
            Func<ObjectBase, bool> snippetForObject,
            Action<IList> snippetForCollection,
            params string[] exemptProperties)
        {
            List<ObjectBase> visited = new List<ObjectBase>();
            Action<ObjectBase> walk = null;

            List<string> exemptions = new List<string>();
            if (exemptProperties != null)
            {
                exemptions = exemptProperties.ToList();
            }

            walk = (o) =>
            {
                if (o != null && !visited.Contains(o))
                {
                    visited.Add(o);
                    bool exitWalk = snippetForObject.Invoke(o);
                    if (!exitWalk)
                    {
                        PropertyInfo[] properties = o.GetBrowsableProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (!exemptions.Contains(property.Name))
                            {
                                if (property.PropertyType.IsSubclassOf(typeof(ObjectBase)))
                                {
                                    ObjectBase obj = (ObjectBase)(property.GetValue(o, null));
                                    walk(obj);
                                }
                                else
                                {
                                    IList coll = property.GetValue(o, null) as IList;
                                    if (coll != null)
                                    {
                                        snippetForCollection.Invoke(coll);

                                        foreach (object item in coll)
                                        {
                                            if (item is ObjectBase)
                                            {
                                                walk((ObjectBase)item);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
        #endregion Protected Methods

        #region Property change notification
        protected override void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName, true);
        }

        protected void OnPropertyChanged(string propertyName, bool makeDirty)
        {
            base.OnPropertyChanged(propertyName);
            if (makeDirty)
                IsDirty = true;

            Validate();
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression, bool makeDirty)
        {
            string propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
            OnPropertyChanged(propertyName, makeDirty);
        }
        #endregion Property change notification
    }
}
