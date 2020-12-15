using JosephM.Xrm.Sms.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Sms.Plugins.Plugins
{
    public class SmsPlugin : JosephMEntityPluginBase
    {
        public override void GoExtention()
        {
            SendAndComplete();
            //call both these because the update one wasn't always getting called
            SetActivityCompleteTriggerForSetState();
            SetActivityCompleteTriggerForUpdate();
        }

        private void SendAndComplete()
        {
            //if send and complete set the set state to complete
            if (IsMessage(PluginMessage.Create, PluginMessage.Update) && IsStage(PluginStage.PostEvent) && IsMode(PluginMode.Synchronous))
            {
                if (BooleanChangingToTrue(Fields.jmcg_sms_.jmcg_sendandcomplete))
                {
                    XrmService.SetState(TargetType, TargetId, OptionSets.SMS.ActivityStatus.Completed);
                }
            }
            //if send and complete set true and setting state to complete
            //then send the sms
            if (IsMessage(PluginMessage.SetStateDynamicEntity)
                && IsStage(PluginStage.PostEvent) 
                && IsMode(PluginMode.Synchronous)
                && SetStateState == OptionSets.SMS.ActivityStatus.Completed)
            {
                var sms = XrmService.Retrieve(Entities.jmcg_sms, TargetId, new[] { Fields.jmcg_sms_.jmcg_sendandcomplete, Fields.jmcg_sms_.to, Fields.jmcg_sms_.description });
                if (sms.GetBoolean(Fields.jmcg_sms_.jmcg_sendandcomplete))
                {
                    var to = sms.GetActivityParties(Fields.jmcg_sms_.to);
                    if(Context.ParentContext != null
                        && Context.ParentContext.InputParameters.Contains("Target")
                        && Context.ParentContext.InputParameters["Target"] is Entity parentContextTarget
                        && parentContextTarget.Contains(Fields.jmcg_sms_.to))
                    {
                        to = parentContextTarget.GetActivityParties(Fields.jmcg_sms_.to);
                    }
                    if (to.Count() == 0)
                    {
                        throw new InvalidPluginExecutionException($"The {GetFieldLabel(Fields.jmcg_sms_.to)} Recipient Is Required");
                    }
                    if (to.Count() != 1)
                    {
                        throw new InvalidPluginExecutionException($"Only 1 Recipient Is Implemented. Ensure Only 1 Contact Is Entered In The {GetFieldLabel(Fields.jmcg_sms_.to)} Field");
                    }
                    var recipientParty = to.First();
                    var recipient = recipientParty.GetField(Fields.activityparty_.partyid) as EntityReference;
                    if (recipient == null || recipient.LogicalName != Entities.contact)
                    {
                        throw new InvalidPluginExecutionException($"The {GetFieldLabel(Fields.jmcg_sms_.to)} Recipient Must Be A {XrmService.GetEntityDisplayName(Entities.contact)}");
                    }
                    var mobileNumber = (string)XrmService.LookupField(recipient.LogicalName, recipient.Id, Fields.contact_.mobilephone);
                    if (string.IsNullOrWhiteSpace(mobileNumber))
                    {
                        throw new InvalidPluginExecutionException($"The Recipients {XrmService.GetFieldLabel(Fields.contact_.mobilephone, Entities.contact)} Is Empty");
                    }
                    var smsContent = sms.GetStringField(Fields.jmcg_sms_.description);
                    if (string.IsNullOrWhiteSpace(smsContent))
                    {
                        throw new InvalidPluginExecutionException($"{GetFieldLabel(Fields.jmcg_sms_.description)} Is Empty");
                    }

                    var cleanMobileNumber = string.Empty;
                    foreach (var character in mobileNumber)
                    {
                        if (char.IsNumber(character) || character == '+')
                        {
                            cleanMobileNumber = cleanMobileNumber + character;
                        }
                    }

                    if (JosephMSmsSettings.ActuallySendSms)
                    {
                       JosephMSmsService.SendSms(cleanMobileNumber, smsContent, JosephMSmsSettings.SmsSource);
                    }
                }
            }
        }

        /// <summary>
        /// when various system generated activities are completed the system needs to update a field
        /// in the regarding record to specify completed
        /// </summary>
        public void SetActivityCompleteTriggerForUpdate()
        {
            if (IsMessage(PluginMessage.Update) && IsStage(PluginStage.PostEvent) && IsMode(PluginMode.Synchronous))
            {
                if (OptionSetChangedTo(Fields.activitypointer_.statecode, OptionSets.Activity.ActivityStatus.Completed))
                {
                    SetActivityCompleteTrigger(GetField);
                }
            }
        }

        /// <summary>
        /// when various system generated activities are completed the system needs to update a field
        /// in the regarding record to specify completed
        /// </summary>
        public void SetActivityCompleteTriggerForSetState()
        {
            if (IsMessage(PluginMessage.SetStateDynamicEntity)
                            && IsStage(PluginStage.PostEvent)
                            && IsMode(PluginMode.Synchronous)
                            && SetStateState == OptionSets.SMS.ActivityStatus.Completed)
            {
                SetActivityCompleteTrigger(XrmService.Retrieve(TargetType, TargetId).GetField);
            }
        }

        private void SetActivityCompleteTrigger(Func<string, object> getField)
        {
            if (!string.IsNullOrWhiteSpace((string)getField(Fields.jmcg_sms_.jmcg_setsentonregardingfield)))
            {
                var regardingType = XrmEntity.GetLookupType(getField(Fields.activitypointer_.regardingobjectid));
                var regardingId = XrmEntity.GetLookupGuid(getField(Fields.activitypointer_.regardingobjectid));
                if (regardingId.HasValue)
                {
                    var fieldToSet = (string)getField(Fields.jmcg_sms_.jmcg_setsentonregardingfield);
                    var fieldType = XrmService.GetFieldType(fieldToSet, regardingType);
                    var completionTarget = XrmService.Retrieve(regardingType, regardingId.Value, new[] { fieldToSet });
                    if (fieldType == AttributeTypeCode.Boolean)
                    {
                        if (!completionTarget.GetBoolean(fieldToSet))
                            XrmService.SetField(regardingType, regardingId.Value, fieldToSet, true);
                    }
                    else if (fieldType == AttributeTypeCode.DateTime)
                    {
                        if (!XrmEntity.FieldsEqual(completionTarget.GetField(fieldToSet), LocalisationService.TodayUnspecifiedType))
                            XrmService.SetField(regardingType, regardingId.Value, fieldToSet, LocalisationService.TodayUnspecifiedType);
                    }
                    else
                        throw new NotImplementedException(string.Format("Setting the field type {0} of the field {1} on {2} type is not implemented", fieldType, fieldToSet, regardingType));
                }
            }
        }
    }
}