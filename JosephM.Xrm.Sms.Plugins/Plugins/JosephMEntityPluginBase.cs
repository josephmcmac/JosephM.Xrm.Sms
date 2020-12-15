using JosephM.Xrm.Sms.Plugins.Services;
using JosephM.Xrm.Sms.Plugins.Rollups;
using JosephM.Xrm.Sms.Plugins.Xrm;
using JosephM.Xrm.Sms.Plugins.SharePoint;
using JosephM.Xrm.Sms.Plugins.Localisation;

namespace JosephM.Xrm.Sms.Plugins.Plugins
{
    /// <summary>
    /// class for shared services or settings objects for plugins
    /// </summary>
    public abstract class JosephMEntityPluginBase : XrmEntityPlugin
    {
        private JosephMSmsSettings _settings;
        public JosephMSmsSettings JosephMSmsSettings
        {
            get
            {
                if (_settings == null)
                    _settings = new JosephMSmsSettings(XrmService);
                return _settings;
            }
        }

        private JosephMSmsService _service;
        public JosephMSmsService JosephMSmsService
        {
            get
            {
                if (_service == null)
                    _service = new JosephMSmsService(XrmService, JosephMSmsSettings);
                return _service;
            }
        }

        private JosephMRollupService _RollupService;
        public JosephMRollupService JosephMRollupService
        {
            get
            {
                if (_RollupService == null)
                    _RollupService = new JosephMRollupService(XrmService);
                return _RollupService;
            }
        }

        private JosephMSharepointService _sharePointService;
        public JosephMSharepointService JosephMSharepointService
        {
            get
            {
                if (_sharePointService == null)
                    _sharePointService = new JosephMSharepointService(XrmService, new JosephMSharePointSettings(XrmService));
                return _sharePointService;
            }
        }

        private LocalisationService _localisationService;
        public LocalisationService LocalisationService
        {
            get
            {
                if (_localisationService == null)
                    _localisationService = new LocalisationService(new UserLocalisationSettings(XrmService, Context.InitiatingUserId));
                return _localisationService;
            }
        }
    }
}
