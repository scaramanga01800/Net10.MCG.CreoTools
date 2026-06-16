using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.WpfComponent.ViewModel;

namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    public class MassUpdateAttributeValue : ObservableObject
    {
        private string _ParentPartNumber;
        public string ParentPartNumber
        {
            get { return this._ParentPartNumber; }
            set
            {
                if (this._ParentPartNumber != value)
                {
                    this._ParentPartNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _OldValue = "";
        public string OldValue
        {
            get { return this._OldValue; }
            set
            {
                if (this._OldValue != value)
                {
                    this._OldValue = value;
                    NewValue = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _NewValue = "";
        public string NewValue
        {
            get { return this._NewValue; }
            set
            {
                if (this._NewValue != value)
                {
                    if (ParentAttribute != null && ParentAttribute.MaxCharacters > 0 && value.Length > ParentAttribute.MaxCharacters)
                        this._NewValue = value.Substring(0, ParentAttribute.MaxCharacters);
                    else
                        this._NewValue = value;
                    OnPropertyChanged();
                }
                IsUpdated = !(NewValue == OldValue);
            }
        }

        private bool _IsUpdated = false;
        public bool IsUpdated
        {
            get { return this._IsUpdated; }
            set
            {
                if (this._IsUpdated != value)
                {
                    this._IsUpdated = value;
                    OnPropertyChanged();
                }
            }
        }

        public McgAttributeColumnHeaderInfo ParentAttribute { get; set; }

        public MassUpdateAttributeValue(string AttributeValue)
        {
            OldValue = AttributeValue;
        }

        public MassUpdateAttributeValue(string AttributeValue, McgAttributeColumnHeaderInfo pParentAttribute)
        {
            OldValue = AttributeValue;
            ParentAttribute = pParentAttribute;
        }

        public MassUpdateAttributeValue(string AttributeValue, McgAttributeColumnHeaderInfo pParentAttribute, string pParentNumber)
        {
            OldValue = AttributeValue;
            ParentAttribute = pParentAttribute;
            ParentPartNumber = pParentNumber;
        }

        public override string ToString()
        {
            return NewValue;
        }
    }
}