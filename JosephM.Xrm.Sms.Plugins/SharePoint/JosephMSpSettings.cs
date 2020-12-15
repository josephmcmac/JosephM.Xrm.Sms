using System;
using JosephM.Xrm.Sms.Plugins.Xrm;

namespace JosephM.Xrm.Sms.Plugins.SharePoint
{
    public class JosephMSharePointSettings : ISharePointSettings
    {
        public JosephMSharePointSettings(XrmService xrmService)
        {
            XrmService = xrmService;
        }

        private string _username;
        public string UserName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private string _password;
        public string Password
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private XrmService XrmService { get; }
    }
}
