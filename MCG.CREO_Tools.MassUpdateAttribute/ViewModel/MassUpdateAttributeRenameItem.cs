using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MassUpdateAttribute.View;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;

namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    public class MassUpdateAttributeRenameItem : ObservableObject, IMassUpdateAttributeRenameItem
    {
        private string _Number;
        public string Number
        {
            get { return _Number; }
            set
            {
                if (this._Number != value)
                {
                    this._Number = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _OldName;
        public string OldName
        {
            get { return _OldName; }
            set
            {
                if (this._OldName != value)
                {
                    this._OldName = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _NewName;
        public string NewName
        {
            get { return _NewName; }
            set
            {
                if (this._NewName != value)
                {
                    this._NewName = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _ObjectId;
        public string ObjectId
        {
            get { return _ObjectId; }
            set
            {
                if (this._ObjectId != value)
                {
                    this._ObjectId = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _State = "Unknown";
        public string State
        {
            get { return _State; }
            set
            {
                if (this._State != value)
                {
                    this._State = value;
                    OnPropertyChanged();
                }

            }
        }

        private WindchillObjectType _ObjectType;
        public WindchillObjectType ObjectType
        {
            get { return _ObjectType; }
            set
            {
                if (this._ObjectType != value)
                {
                    this._ObjectType = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ToBeRenamed = false;
        public bool ToBeRenamed
        {
            get { return _ToBeRenamed; }
            set
            {
                if (this._ToBeRenamed != value)
                {
                    this._ToBeRenamed = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsSelected = false;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (this._IsSelected != value)
                {
                    this._IsSelected = value;
                    OnPropertyChanged();
                }

            }
        }

        public RestOdataWtObject OdataObject { get; set; }

        public bool IsReadOnly { get; set; } = false;
    }
}
