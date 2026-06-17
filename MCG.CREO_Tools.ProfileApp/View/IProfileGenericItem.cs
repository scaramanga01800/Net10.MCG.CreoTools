using MCG.CREO_Tools.ProfileApp.ViewModel;

namespace MCG.CREO_Tools.ProfileApp.View
{
    public interface IProfileGenericItem
    {
        string Description { get; set; }
        double? Width { get; set; }
        double? Height { get; set; }
        double? Thickness { get; set; }
        string StandardType { get; set; }


        string PartNumber { get; set; }
        string IdType { get; set; }
        string ProfileGeneric { get; set; }
        string DrwNumberCompleteView { get; set; }
        string DrwNumberBrokenView { get; set; }
        string Material { get; set; }


        ProfileGenericItem UpdatedProfile { get; set; }

    }
}
