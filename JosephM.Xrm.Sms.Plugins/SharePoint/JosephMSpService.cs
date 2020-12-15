using System.Collections.Generic;
using JosephM.Xrm.Sms.Plugins.Xrm;

namespace JosephM.Xrm.Sms.Plugins.SharePoint
{
    public class JosephMSharepointService : SharePointService
    {
        public JosephMSharepointService(XrmService xrmService, ISharePointSettings sharepointSettings)
            : base(sharepointSettings, xrmService)
        {
        }

        public override IEnumerable<SharepointFolderConfig> SharepointFolderConfigs
        {
            get
            {

                return new SharepointFolderConfig[]
                {
                };
            }
        }
    }
}
