using Fluent;
using MCG.CREO_Tools.CutLengthApp.Exceptions;
using MCG.CREO_Tools.CutLengthApp.ViewModel;

namespace MCG.CREO_Tools.CutLengthApp.View
{
    public partial class CutLengthCutUpdatePartView : RibbonWindow
    {
        private CutLengthCutUpdatePartViewModel CurrentDataContext {  get; set; }

        public CutLengthCutUpdatePartView(CutLengthCutUpdatePartViewModel currentViewModel)
        {
            try
            {
                InitializeComponent();
                CurrentDataContext = currentViewModel;
                DataContext = currentViewModel; 
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void SetCurrentPart(CutLengthCutPart CurrentPart)
        {
            try
            {
                CurrentDataContext.PartItem = CurrentPart;
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
