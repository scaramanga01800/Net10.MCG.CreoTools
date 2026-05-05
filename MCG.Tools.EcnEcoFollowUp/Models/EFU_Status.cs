namespace MCG.Tools.EcnEcoFollowUp.Models
{
    public enum EFU_Status
    {
        OK,
        UNKNOWN,
        ECN_NOT_IN_ECN_PDM,
        ECN_NOT_IN_ECNECOSTATUS,
        ECN_NOT_IN_ECO_SAP_EXPORT,
        ECN_NOT_IN_PDM,
        ECNINPROGRESS,
        ECNINPROGRESS90,
        ECOTOBECREATED,
        ECOSTATUS99,
        ECOSTATUS01,
        ECOSTATUS01_6MONTHS,
        ECOSTATUS02,
        ECOSTATUS03,
        ECNCANCELED,
        WFTASKCOMPLETED,
        WFTASKREWORKED,
        WFTASKINPROGRESS,
        ECNUNDERREVIEW
    }
}
