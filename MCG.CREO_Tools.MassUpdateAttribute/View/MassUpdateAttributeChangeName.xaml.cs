using Fluent;
using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;

namespace MCG.CREO_Tools.MassUpdateAttribute.View
{
    public partial class MassUpdateAttributeChangeName : RibbonWindow
    {
        public MassUpdateAttributeChangeName()
        {
            InitializeComponent();
        }

        public void SetDataContext(MassUpdateAttributeViewModel dataContext)
        {
            DataContext = dataContext;
        }
    }
}
