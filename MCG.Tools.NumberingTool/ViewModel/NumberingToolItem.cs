using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.NumberingTool.Exceptions;
using MCG.Tools.NumberingTool.View;

namespace MCG.Tools.NumberingTool.ViewModel
{
    public class NumberingToolItem : ObservableObject, INumberingToolItem
    {
        #region [REGION] Properties from Interface
        private string _Number = string.Empty;
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

        private string _CreatedBy = string.Empty;
        public string CreatedBy
        {
            get { return _CreatedBy; }
            set
            {
                if (this._CreatedBy != value)
                {
                    this._CreatedBy = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Description = string.Empty;
        public string Description
        {
            get { return _Description; }
            set
            {
                if (this._Description != value)
                {
                    this._Description = value;
                    OnPropertyChanged();
                    IsUpdated=true;
                }

            }
        }

        private string _Product = string.Empty;
        public string Product
        {
            get { return _Product; }
            set
            {
                if (this._Product != value)
                {
                    this._Product = value;
                    OnPropertyChanged();
                    IsUpdated=true;
                }

            }
        }

        private string _Format = string.Empty;
        public string Format
        {
            get { return _Format; }
            set
            {
                if (this._Format != value)
                {
                    this._Format = value;
                    OnPropertyChanged();
                    IsUpdated=true;
                }

            }
        }

        private DateTime _CreatedOn;
        public DateTime CreatedOn
        {
            get { return _CreatedOn; }
            set
            {
                if (this._CreatedOn != value)
                {
                    this._CreatedOn = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        private string _CreatedById = string.Empty;
        public string CreatedById
        {
            get { return _CreatedById; }
            set
            {
                if (this._CreatedById != value)
                {
                    this._CreatedById = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsUpdated =false;
        public bool IsUpdated
        {
            get { return _IsUpdated; }
            set
            {
                if (this._IsUpdated != value)
                {
                    this._IsUpdated = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Misc
        public NumberingItem GetNumberingItem(int CurrentId=0)
        {
            try
            {
                return new NumberingItem()
                {
                    Createdbyfullname = CreatedBy,
                    Createdbyid = CreatedById,
                    Createdon = DateOnly.FromDateTime(CreatedOn),
                    Description = Description,
                    Format = Format,
                    Number = Number,
                    Product = Product,
                    Id = CurrentId
                };
            }
            catch (Exception ex)
            {
                throw new NumberingToolException(this.GetType().Name, ex);
            }
        }

        public static NumberingToolItem GetNumberingToolItem(NumberingItem CurrentItem)
        {
            try
            {
                if (CurrentItem != null)
                {
                    if (CurrentItem.Createdon == null) CurrentItem.Createdon = DateOnly.MinValue;

                    return new NumberingToolItem()
                    {
                        CreatedBy = CurrentItem.Createdbyfullname ?? string.Empty,
                        CreatedById = CurrentItem.Createdbyid ?? string.Empty,
                        CreatedOn = CurrentItem.Createdon.Value.ToDateTime(TimeOnly.MinValue),
                        Description = CurrentItem.Description ?? string.Empty,
                        Format = CurrentItem.Format ?? string.Empty,
                        Number = CurrentItem.Number ?? string.Empty,
                        Product = CurrentItem.Product ?? string.Empty
                    };
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw new NumberingToolException("NumberingToolItem", ex);
            }
        }
        #endregion
    }
}
