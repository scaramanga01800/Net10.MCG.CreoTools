using MCG.CommonLib.CreoInteractionTools.Models;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MCG.CREO_Tools.QuickSearch.ViewModel
{
    public class QuickSearchExtraCompMenu : MenuItem
    {
        public EPMDocument ExtraEpmDoc { get; set; }

        public BitmapImage ExtraCompImage { get; set; }

        public string  Description { get; set; }
    }
}
