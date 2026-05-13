using MCG.Tools.EcnDataCheck.Exceptions;

namespace MCG.Tools.EcnDataCheck.Models
{
    public class DataCheckRule
    {
        public string Name { get; set; }

        public DataCheckOption RuleOption { get; set; }

        public string Document { get; set; }

        public DataCheckStatus GetDataCheckStatus()
        {
            try
            {
                switch (RuleOption)
                {
                    case DataCheckOption.E:
                        return DataCheckStatus.ISSUE;
                    case DataCheckOption.N:
                        return DataCheckStatus.NONE;
                    case DataCheckOption.W:
                        return DataCheckStatus.WARNING;
                    default:
                        return DataCheckStatus.ISSUE;
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
    }
}
