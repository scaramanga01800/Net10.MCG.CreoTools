using MCG.CREO_Tools.ProfileApp.ViewModel;

namespace MCG.CREO_Tools.ProfileApp.Configuration
{
    public class ProfileConfiguration
    {
        public List<ProfileDrwLocation> ListDrwLocation { get; set; }
        public List<string> ListGrpCreator { get; set; }
        public List<DrwScaleItem> ListDrwScale { get; set; }
        public List<string> ListMaterial { get; set; }
        public List<string> ListGeneric3D { get; set; }
        public List<string> ListGenericDrwComplete { get; set; }
        public List<string> ListGenericDrwBroken { get; set; }
        public List<string> ListStdType { get; set; }

    }
}
