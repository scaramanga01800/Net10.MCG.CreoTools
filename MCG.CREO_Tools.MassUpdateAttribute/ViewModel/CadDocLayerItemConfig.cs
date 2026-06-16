using MCG.CREO_Tools.MassUpdateAttribute.Exceptions;

namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    public class CadDocLayerItemConfig
    {
        public string Name { get; set; }

        public bool IsDisplayed { get; set; }

        public bool ToBeCreatedIfMissing { get; set; } = false;

        public string RefType { get; set; }

        public CadDocLayerItem GetCadDocLayerItem()
        {
            try
            {
                return new CadDocLayerItem()
                {
                    Name = Name,
                    IsDisplayed = IsDisplayed,
                    ToBeCreatedIfMissing = ToBeCreatedIfMissing,
                    RefType = RefType
                };
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }
    }
}
