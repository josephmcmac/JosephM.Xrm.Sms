using JosephM.Xrm.Sms.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using Schema;
using System;

namespace JosephM.Xrm.Sms.Plugins.Plugins
{
    public class BulkSmsPlugin : JosephMEntityPluginBase
    {
        public override void GoExtention()
        {
            ProcessSendSmsMessages();
        }

        private void ProcessSendSmsMessages()
        {
            if (IsMessage(PluginMessage.Create, PluginMessage.Update) && IsStage(PluginStage.PreOperationEvent))
            {
                if (BooleanChangingToTrue(Fields.jmcg_bulksms_.jmcg_sendmessages))
                {
                    var smsContent = GetStringField(Fields.jmcg_bulksms_.jmcg_smscontent);
                    if (string.IsNullOrWhiteSpace(smsContent))
                    {
                        throw new InvalidPluginExecutionException($"{GetFieldLabel(Fields.jmcg_bulksms_.jmcg_smscontent)} Is Required");
                    }
                    var marketingListId = GetLookupGuid(Fields.jmcg_bulksms_.jmcg_marketinglist);
                    if (!marketingListId.HasValue)
                    {
                        throw new InvalidPluginExecutionException($"{GetFieldLabel(Fields.jmcg_bulksms_.jmcg_marketinglist)} Is Required");
                    }
                    var marketingList = XrmService.Retrieve(Entities.list, marketingListId.Value, new[] { Fields.list_.membertype });
                    if (marketingList.GetInt(Fields.list_.membertype) != OptionSets.MarketingList.MarketingListMemberType.Contact)
                    {
                        throw new InvalidPluginExecutionException($"{GetFieldLabel(Fields.jmcg_bulksms_.jmcg_marketinglist)} {XrmService.GetFieldLabel(Fields.list_.membertype, Entities.list)} Is Required To Be {XrmService.GetOptionLabel(OptionSets.MarketingList.MarketingListMemberType.Contact, Fields.list_.createdfromcode, Entities.list)}");
                    }
                    SetField(Fields.jmcg_bulksms_.jmcg_lastsendprocessid, Guid.NewGuid().ToString());
                    SetField(Fields.jmcg_bulksms_.jmcg_sendingmessages, true);
                    SetField(Fields.jmcg_bulksms_.jmcg_sendmessages, false);
                    SetField(Fields.jmcg_bulksms_.jmcg_fatalerror, null);
                }
            }
            if (IsMessage(PluginMessage.Create, PluginMessage.Update) && IsStage(PluginStage.PostEvent) && IsMode(PluginMode.Synchronous))
            {
                if (BooleanChangingToTrue(Fields.jmcg_bulksms_.jmcg_sendingmessages))
                {
                    XrmService.StartWorkflow(JosephMSmsSettings.BulkSmsSendMessagesWorkflowId, TargetId);
                }
            }
        }
    }
}