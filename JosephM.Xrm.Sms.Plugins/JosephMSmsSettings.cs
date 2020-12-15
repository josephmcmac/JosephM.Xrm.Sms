using Didasko.Xrm.Plugins.Services.GlobalSms;
using JosephM.Xrm.Sms.Plugins.Services;
using JosephM.Xrm.Sms.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using Schema;
using System;

namespace JosephM.Xrm.Sms.Plugins
{
    /// <summary>
    /// A settings object which loads the first record of a given type for accessing its fields/properties
    /// </summary>
    public class JosephMSmsSettings : IGlobalSmsSettings
    {
        private XrmService XrmService { get; set; }

        public JosephMSmsSettings(XrmService xrmService)
        {
            XrmService = xrmService;
        }

        private const string EntityType = Entities.jmcg_smssettings;

        private Entity _settings;

        public Entity Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = XrmService.GetFirst(EntityType);
                    if (_settings == null)
                        throw new NullReferenceException($"Error getting the {XrmService.GetEntityDisplayName(EntityType)} record. It does not exist or you do not have permissions to view it");
                }
                return _settings;
            }
            set { _settings = value; }
        }

        public bool ActuallySendSms
        {
            get
            {
                return Settings.GetBoolean(Fields.jmcg_smssettings_.jmcg_actuallysendsms);
            }
        }

        public string SmsSource
        {
            get
            {
                return Settings.GetStringField(Fields.jmcg_smssettings_.jmcg_smssource);
            }
        }

        public int SmsProvider
        {
            get
            {
                return Settings.GetOptionSetValue(Fields.jmcg_smssettings_.jmcg_smsprovider);
            }
        }

        public string ApiSecret
        {
            get
            {
                return Settings.GetStringField(Fields.jmcg_smssettings_.jmcg_apisecret);
            }
        }

        public string ApiKey
        {
            get
            {
                return Settings.GetStringField(Fields.jmcg_smssettings_.jmcg_apikey);
            }
        }

        string IGlobalSmsSettings.key => ApiKey;

        string IGlobalSmsSettings.secret => ApiSecret;

        public Guid BulkSmsSendMessagesWorkflowId { get { return new Guid("B295F7D0-E410-49EF-8232-A023BB942CBF"); } }
    }
}