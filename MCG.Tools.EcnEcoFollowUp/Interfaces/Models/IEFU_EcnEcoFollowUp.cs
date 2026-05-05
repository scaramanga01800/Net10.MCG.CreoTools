namespace MCG.Tools.EcnEcoFollowUp.Interfaces.Models
{
    public interface IEFU_EcnEcoFollowUp
    {
        string Ecn_Number { get; set; }
        string Ecn_Name { get; set; }
        string Ecn_State { get; set; }
        string Pdm_Product { get; set; }
        string Ecn_Creator_Name { get; set; }

        string Eco_Status { get; set; }
        string Eco_Urgence { get; set; }
        string Eco_Project { get; set; }
        string Eco_Categ { get; set; }
        string Eco_Sub_Line { get; set; }
        string Eco_Next_Step { get; set; }
        bool Eco_IsCreated { get; set; }

        int Nb_Part { get; set; }
        int Nb_Drw { get; set; }
        int Nb_Epm_Doc { get; set; }
        int Nb_Wt_Doc { get; set; }

        int Eco_Status_Order { get; set; }

        DateTime? Ecn_Created_On { get; set; }
        DateTime? Designer_Start_App_Date { get; set; }
        DateTime? First_Approval_Date { get; set; }
        DateTime? Qual_Check_Approval_Date { get; set; }
        DateTime? CAIII_Approval_Date { get; set; }

        DateTime? Eco_Created_On { get; set; }
        DateTime? Eco_Wf_Started_On { get; set; }
        DateTime? Eco_Effectivity_Date { get; set; }
        DateTime? Eco_Closed_On { get; set; }
        DateTime? Eco_Last_Wi_Created_On { get; set; }

        string MainPlantDescription { get; set; }
        string MainPlant { get; set; }
        string Eco_Next_Step_Secondary { get; set; }
        DateTime? Eco_Last_Wi_Created_On_Secondary { get; set; }
        string Eco_Wi_Secondary_Plants { get; set; }

    }
}
