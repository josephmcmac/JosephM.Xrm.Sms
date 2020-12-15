using JosephM.Xrm.Sms.Plugins.Services;
using JosephM.Xrm.Sms.Plugins.Xrm;

namespace JosephM.Xrm.Sms.Plugins.Workflows
{
    /// <summary>
    /// class for shared services or settings objects for workflow activities
    /// </summary>
    public abstract class JosephMSmsWorkflowActivity<T> : XrmWorkflowActivityInstance<T>
        where T : XrmWorkflowActivityRegistration
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
    }
}
