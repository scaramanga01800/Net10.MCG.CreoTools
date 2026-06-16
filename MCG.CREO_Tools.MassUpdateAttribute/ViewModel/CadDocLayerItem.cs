using MCG.CommonLib.Models.Enums;
using pfcls;

namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    public class CadDocLayerItem: CadDocLayerItemConfig
    {

        public ObjectState State { get; set; } = ObjectState.UNKNOWN;

        public IpfcLayer LayerItem { get; set; } = null;

        public EpfcDisplayStatus DisplayStatus { get; set; } = EpfcDisplayStatus.EpfcDisplayStatus_nil;

        public List<IpfcModelItem> ListModelItems { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }


}

