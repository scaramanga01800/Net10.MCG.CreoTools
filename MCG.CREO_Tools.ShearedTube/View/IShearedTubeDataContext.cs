using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.ShearedTube.View
{
    interface IShearedTubeDataContext
    {
        bool IsCreoEnable { get; set; }
        bool IsHoleSelected { get; set; }
        string HoleDiameter { get; set; }
        string HoleLength { get; set; }
        string ExtremityAngle { get; set; }

        ObservableCollection<double> ListThickness { get; set; }
        ObservableCollection<double> ListDiameter { get; set; }
        double SelectedThickness { get; set; }
        double SelectedDiameter { get; set; }

        string LeftAngle { get; set; }
        string RightAngle { get; set; }

        string TotalLength { get; set; }

        string Number { get; set; }
        string PtcCommonName { get; set; }
        string Description_2 { get; set; }
        string Description2_1 { get; set; }
        string Description2_2 { get; set; }

        ObservableCollection<string> ListGroupCreator { get; set; }
        ObservableCollection<string> ListQualInspGroup { get; set; }
        string SelectedGroupCreator { get; set; }
        string SelectedQualInspGroup { get; set; }
    }
}
