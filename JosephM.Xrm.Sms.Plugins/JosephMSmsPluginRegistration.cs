using JosephM.Xrm.Sms.Plugins.Plugins;
using JosephM.Xrm.Sms.Plugins.Xrm;
using Schema;

namespace JosephM.Xrm.Sms.Plugins
{
    /// <summary>
    /// This is the class for registering plugins in CRM
    /// Each entity plugin type needs to be instantiated in the CreateEntityPlugin method
    /// </summary>
    public class JosephMSmsPluginRegistration : XrmPluginRegistration
    {
        public override XrmPlugin CreateEntityPlugin(string entityType, bool isRelationship)
        {
            switch (entityType)
            {
                case Entities.jmcg_sms: return new SmsPlugin();
                case Entities.jmcg_bulksms: return new BulkSmsPlugin();
            }
            return null;
        }
    }
}
