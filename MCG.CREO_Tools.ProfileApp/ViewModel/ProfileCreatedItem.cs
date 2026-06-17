using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.WebtermLib.Models;
using MCG.CommonLib.WebtermLib.Services;
using MCG.CREO_Tools.ProfileApp.Exceptions;
using pfcls;

namespace MCG.CREO_Tools.ProfileApp.ViewModel
{
    public class ProfileCreatedItem
    {

        public ProfileGenericItem GenericItem { get; set; }

        public string PartNumber { get; set; }

        public double Length { get; set; }

        public double ScaleDrawing { get; set; } = 1;

        public string IsoViewName { get; set; } = "ISOVIEW";

        public double IsoViewScale { get; set; } = 1;

        public string Material { get; set; }

        public bool IsDrwBrokenView { get; set; } = false;

        public string GroupCreator { get; set; }

        public string CreatedBy { get; set; }

        public ProfileDrwLocation CurrentLang { get; set; }

        public bool UseWindchillTemplate { get; set; } = false;

        public string LocalTemplateDir { get; set; } = @"D:\Programme\MCG_Tools\Resources\AppProfileTemplate\";

        //public CREOModelStatus CreateProfile(CREOConnection pCREOConnection)
        //{
        //    try
        //    {
        //        if (UseWindchillTemplate)
        //            LocalTemplateDir = "";

        //        //Retrieve drawing template
        //        IpfcModel DrwTemplagteModel;
        //        try
        //        {
        //            if (IsDrwBrokenView)
        //                DrwTemplagteModel = CREOConnection.retrieveModel(pCREOConnection.Session,
        //                    $"{LocalTemplateDir}{ GenericItem.OrigProfileGeneric.Drwnumberbrokenview}_{CurrentLang.DrwSuffix}",
        //                    EpfcModelType.EpfcMDL_DRAWING);
        //            else
        //                DrwTemplagteModel = CREOConnection.retrieveModel(pCREOConnection.Session,
        //                    $"{LocalTemplateDir}{ GenericItem.OrigProfileGeneric.Drwnumbercompleteview}_{CurrentLang.DrwSuffix}",
        //                    EpfcModelType.EpfcMDL_DRAWING);

        //            if (DrwTemplagteModel == null)
        //                return CREOModelStatus.RETRIEVEISSUE;
        //        }
        //        catch (Exception)
        //        {
        //            return CREOModelStatus.RETRIEVEISSUE;
        //        }

        //        //Copy drawing template with new number: use rename
        //        try
        //        {
        //            DrwTemplagteModel.Rename(PartNumber, true);
        //        }
        //        catch (Exception)
        //        {
        //            return CREOModelStatus.RENAMEISSUE;
        //        }

        //        //replace drawing model with the right one
        //        IpfcModel Generic3DModel;
        //        IpfcModel Instance3DModel;
        //        try
        //        {
        //            Generic3DModel = CREOConnection.retrieveModel(pCREOConnection.Session, $"{LocalTemplateDir}{GenericItem.OrigProfileGeneric.Profilegeneric1}", EpfcModelType.EpfcMDL_PART);
        //            if (UseWindchillTemplate)
        //                Instance3DModel = CREOConnection.retrieveModel(pCREOConnection.Session, GenericItem.OrigProfileGeneric.Partnumber, EpfcModelType.EpfcMDL_PART);
        //            else
        //                Instance3DModel = CREOConnection.retrieveModel(pCREOConnection.Session, $"{LocalTemplateDir}{GenericItem.OrigProfileGeneric.Partnumber}<{GenericItem.OrigProfileGeneric.Profilegeneric1}>.PRT", EpfcModelType.EpfcMDL_PART);
        //            CREOConnection.ReplaceModelDrw(DrwTemplagteModel, Generic3DModel, Instance3DModel);
        //        }
        //        catch (Exception)
        //        {
        //            return CREOModelStatus.REPLACEMODELDRWISSUE;
        //        }

        //        //remove instance from family to cut link
        //        IpfcFamilyMember GenericFamily;
        //        try
        //        {
        //            GenericFamily = (IpfcFamilyMember)Generic3DModel;
        //            IpfcFamilyTableRows AllRows = GenericFamily.ListRows();
        //            string instanceName = GenericItem.OrigProfileGeneric.Partnumber.Split('.').First();
        //            for (int index = 0; index < AllRows.Count; index++)
        //            {
        //                if (AllRows[index].InstanceName.ToUpper() == instanceName.ToUpper())
        //                {
        //                    GenericFamily.RemoveRow(AllRows[index]);
        //                    index = AllRows.Count;
        //                }
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            return CREOModelStatus.SESSIONISSUE;
        //        }

        //        //remove generics from the session
        //        try
        //        {
        //            Generic3DModel.Erase();
        //        }
        //        catch (Exception)
        //        {
        //            return CREOModelStatus.SESSIONISSUE;
        //        }

        //        //rename instance with new number
        //        try
        //        {
        //            Instance3DModel.Rename(PartNumber, true);
        //        }
        //        catch (Exception)
        //        {
        //            return CREOModelStatus.SESSIONISSUE;
        //        }

        //        //Change Dimensions and attributes
        //        try
        //        {
        //            //pCREOConnection.session.SetConfigOption("regen_failure_handling", "resolve_mode");
        //            CREOConnection.SetDimensionValuesNotByRef(Instance3DModel, "Length", Length);

        //            //Update Length
        //            IpfcSolid Model3D = (IpfcSolid)Instance3DModel;

        //            //Update Material
        //            CREOConnection.AssignMaterial(Instance3DModel, Material);

        //            //Update Param Description 1&2
        //            string DescLocal = WebtermTools.GetTerm(GenericItem.TemplateProfileType.OrigProfileType.Paramdescen, WebtermLanguage.ENGLISH, WebtermTools.GetWebtermLanguage(CurrentLang.WebtermLang));
        //            string DescDetailEn = GenericItem.TemplateProfileType.OrigProfileType.Paramdescdetailen.
        //                        Replace("LENGTH", Length.ToString()).
        //                        Replace("WIDTH", GenericItem.OrigProfileGeneric.Width.ToString()).
        //                        Replace("HEIGHT", GenericItem.OrigProfileGeneric.Height.ToString()).
        //                        Replace("THICKNESS", GenericItem.OrigProfileGeneric.Thickness.ToString());

        //            string DescDetailLocal = GenericItem.TemplateProfileType.OrigProfileType.Paramdescdetaillocal.
        //                        Replace("LENGTH", Length.ToString()).
        //                        Replace("WIDTH", GenericItem.OrigProfileGeneric.Width.ToString()).
        //                        Replace("HEIGHT", GenericItem.OrigProfileGeneric.Height.ToString()).
        //                        Replace("THICKNESS", GenericItem.OrigProfileGeneric.Thickness.ToString());

        //            CREOConnection.SetParamValueString(Instance3DModel, "DESCRIPTION_2", DescDetailEn, true);
        //            CREOConnection.SetParamValueString(Instance3DModel, "DESCRIPTION2_1", DescLocal, true);
        //            CREOConnection.SetParamValueString(Instance3DModel, "DESCRIPTION2_2", DescDetailLocal, true);
        //            CREOConnection.SetParamValueString(Instance3DModel, "GROUP_CREATOR", GroupCreator, true);
        //            CREOConnection.SetParamValueString(Instance3DModel, "MODIFIED_BY", CreatedBy, true);

        //            Model3D.Regenerate(null);
        //            //pCREOConnection.session.SetConfigOption("regen_failure_handling", "no_resolve_mode");
        //        }
        //        catch (Exception)
        //        {
        //            return CREOModelStatus.UPDATE3DISSUE;
        //        }

        //        //update scale of the drawings
        //        IpfcWindow currentIpfcWindow = null;
        //        IpfcWindows allIpfcWindows = null;
        //        try
        //        {
        //            allIpfcWindows = pCREOConnection.Session.ListWindows();
        //            if (allIpfcWindows.Count<18)
        //            {
        //                currentIpfcWindow = pCREOConnection.Session.CreateModelWindow(DrwTemplagteModel);
        //                DrwTemplagteModel.Display();
        //            }
        //            else
        //            {
        //                DrwTemplagteModel.Display();
        //                for (int index = 0; index < allIpfcWindows.Count; index++)
        //                {
        //                    currentIpfcWindow = allIpfcWindows[index];
        //                    if (currentIpfcWindow.Model.FileName == DrwTemplagteModel.FileName)
        //                        currentIpfcWindow.Activate();
        //                }
        //            }
        //        }
        //        catch (Exception)
        //        {
        //        }

        //        if (currentIpfcWindow != null)
        //            currentIpfcWindow.Activate();

        //        IpfcSheetOwner DrwSheetOwner;
        //        try
        //        {
        //            //Main scale
        //            DrwSheetOwner = (IpfcSheetOwner)DrwTemplagteModel;
        //            DrwSheetOwner.SetSheetScale(1, ScaleDrawing, null);

        //            //ISO View Scale
        //            IpfcView2D IsoView  = ((IpfcModel2D)DrwTemplagteModel).GetViewByName(IsoViewName);
        //            if (IsoView != null)
        //                IsoView.Scale = IsoViewScale;
        //               // IsoView.Scale = 0.3;
                   
        //        }
        //        catch (Exception)
        //        {
        //            return CREOModelStatus.UPDATEDRWISSUE;
        //        }

        //        //Show 3D model
        //        try
        //        {
        //            allIpfcWindows = pCREOConnection.Session.ListWindows();
        //            if (allIpfcWindows.Count < 18)
        //            {
        //                currentIpfcWindow = pCREOConnection.Session.CreateModelWindow(Instance3DModel);
        //                Instance3DModel.Display();
        //            }
        //            else
        //            {
        //                Instance3DModel.Display();
        //                for (int index = 0; index < allIpfcWindows.Count; index++)
        //                {
        //                    currentIpfcWindow = allIpfcWindows[index];
        //                    if (currentIpfcWindow.Model.FileName == Instance3DModel.FileName)
        //                        currentIpfcWindow.Activate();
        //                }
        //            }
        //        }
        //        catch (Exception)
        //        {
        //        }

        //        if (currentIpfcWindow != null)
        //            currentIpfcWindow.Activate();

        //        return CREOModelStatus.OK;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ProfileException(this.GetType().Name, ex);
        //    }
        //}
    }
}
