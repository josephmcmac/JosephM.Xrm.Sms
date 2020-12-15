using Didasko.Xrm.Plugins.Services.GlobalSms;
using JosephM.Xrm.Sms.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Xrm.Sms.Plugins.Core;

namespace JosephM.Xrm.Sms.Plugins.Services
{
    /// <summary>
    /// A service class for performing logic
    /// </summary>
    public class JosephMSmsService
    {
        private XrmService XrmService { get; set; }
        private JosephMSmsSettings JosephMSmsSettings { get; set; }

        public JosephMSmsService(XrmService xrmService, JosephMSmsSettings settings)
        {
            XrmService = xrmService;
            JosephMSmsSettings = settings;
        }

        private ISmsService GetSmsService()
        {
            var smsProvider = JosephMSmsSettings.SmsProvider;
            switch(smsProvider)
            {
                case OptionSets.SMSSettings.SMSProvider.GlobalSMS: return new GlobalSmsService(JosephMSmsSettings);
            }
            throw new InvalidPluginExecutionException($"No SMS Service is implemented for the {XrmService.GetFieldLabel(Fields.jmcg_smssettings_.jmcg_smsprovider, Entities.jmcg_smssettings)} option value of {smsProvider} in the {XrmService.GetEntityDisplayName(Entities.jmcg_smssettings)} record");
        }

        internal void SendSms(string cleanMobileNumber, string smsContent, string smsSource)
        {
            GetSmsService().SendSms(cleanMobileNumber, smsContent, smsSource);
        }

        public bool ProcessBulkSmsSending(Guid bulkSmsId)
        {
            var startedAt = DateTime.UtcNow;
            var escapeSandboxIsolation = true;
            var sandboxIsolationSeconds = 120;
            var sandboxLeeway = 20;
            var bulkSms = XrmService.Retrieve(Entities.jmcg_bulksms, bulkSmsId);

            //get all the marketing list members
            try
            {
                var marketingListId = bulkSms.GetLookupGuid(Fields.jmcg_bulksms_.jmcg_marketinglist);
                if (!marketingListId.HasValue)
                {
                    throw new InvalidPluginExecutionException($"{XrmService.GetFieldLabel(Fields.jmcg_bulksms_.jmcg_marketinglist, Entities.jmcg_bulksms)} Is Empty");
                }
                var thisProcessId = bulkSms.GetStringField(Fields.jmcg_bulksms_.jmcg_lastsendprocessid);

                //deactivate any errors not for this send process
                var errorsToDeactivate = XrmService.RetrieveAllAndConditions(Entities.jmcg_bulksmserror, new[]
                {
                    new ConditionExpression(Fields.jmcg_bulksmserror_.statecode, ConditionOperator.Equal, OptionSets.BulkSMSError.Status.Active),
                    new ConditionExpression(Fields.jmcg_bulksmserror_.jmcg_bulksms, ConditionOperator.Equal, bulkSms.Id),
                    new ConditionExpression(Fields.jmcg_bulksmserror_.jmcg_sendprocessid, ConditionOperator.NotEqual, thisProcessId)
                }, new string[0]);
                foreach (var errorToDeactivate in errorsToDeactivate)
                {
                    XrmService.SetState(errorToDeactivate.LogicalName, errorToDeactivate.Id, OptionSets.BulkSMSError.Status.Inactive);
                    if (escapeSandboxIsolation && DateTime.UtcNow - startedAt > new TimeSpan(0, 0, sandboxIsolationSeconds - sandboxLeeway))
                        return false;
                }
                //get the sms messages already created/sent
                var existingSmsMessages = XrmService.RetrieveAllAndConditions(Entities.jmcg_sms, new[]
                {
                    new ConditionExpression(Fields.jmcg_sms_.jmcg_bulksms, ConditionOperator.Equal, bulkSms.Id)
                }, new[] { Fields.jmcg_sms_.to });
                var indexSmsByContact = new Dictionary<Guid, Entity>();
                foreach (var sms in existingSmsMessages)
                {
                    var to = sms.GetActivityParties(Fields.jmcg_sms_.to);
                    if (to.Any())
                    {
                        var partyId = to.First().GetLookupGuid(Fields.activityparty_.partyid);
                        if (partyId.HasValue && !indexSmsByContact.ContainsKey(partyId.Value))
                        {
                            indexSmsByContact.Add(partyId.Value, sms);
                        }
                    }
                }

                if (escapeSandboxIsolation && DateTime.UtcNow - startedAt > new TimeSpan(0, 0, sandboxIsolationSeconds - sandboxLeeway))
                    return false;

                var existingErrors = XrmService.RetrieveAllAndConditions(Entities.jmcg_bulksmserror, new[]
                {
                    new ConditionExpression(Fields.jmcg_bulksmserror_.statecode, ConditionOperator.Equal, OptionSets.BulkSMSError.Status.Active),
                    new ConditionExpression(Fields.jmcg_bulksmserror_.jmcg_bulksms, ConditionOperator.Equal, bulkSms.Id),
                    new ConditionExpression(Fields.jmcg_bulksmserror_.jmcg_sendprocessid, ConditionOperator.Equal, thisProcessId)

                }, new[] { Fields.jmcg_bulksmserror_.jmcg_contact });
                var indexErrorByContact = new Dictionary<Guid, Entity>();
                foreach (var error in existingErrors)
                {
                    var contactId = error.GetLookupGuid(Fields.jmcg_bulksmserror_.jmcg_contact);
                    if (contactId.HasValue && !indexErrorByContact.ContainsKey(contactId.Value))
                    {
                        indexErrorByContact.Add(contactId.Value, error);
                    }
                }

                if (escapeSandboxIsolation && DateTime.UtcNow - startedAt > new TimeSpan(0, 0, sandboxIsolationSeconds - sandboxLeeway))
                    return false;

                //get contacts where
                //no sms already sent
                //and no error this run
                var excludeContacts = indexErrorByContact.Keys
                    .Union(indexSmsByContact.Keys)
                    .ToArray();
                var contactsToSms = XrmService
                    .GetMarketingListMembers(marketingListId.Value, new[] { Fields.contact_.contactid, Fields.contact_.mobilephone })
                    .Where(c => !excludeContacts.Contains(c.Id))
                    .ToArray();

                if (escapeSandboxIsolation && DateTime.UtcNow - startedAt > new TimeSpan(0, 0, sandboxIsolationSeconds - sandboxLeeway))
                    return false;

                foreach (var contact in contactsToSms)
                {
                    try
                    {
                        var sms = new Entity(Entities.jmcg_sms);
                        sms.SetField(Fields.jmcg_sms_.subject, bulkSms.GetField(Fields.jmcg_bulksms_.jmcg_name));
                        sms.SetField(Fields.jmcg_sms_.description, bulkSms.GetStringField(Fields.jmcg_bulksms_.jmcg_smscontent));
                        sms.AddToParty(contact.LogicalName, contact.Id);
                        sms.SetLookupField(Fields.jmcg_sms_.jmcg_bulksms, bulkSms);
                        sms.SetField(Fields.jmcg_sms_.jmcg_sendandcomplete, true);
                        sms.Id = XrmService.Create(sms);
                    }
                    catch (Exception ex)
                    {
                        var smsError = new Entity(Entities.jmcg_bulksmserror);
                        smsError.SetField(Fields.jmcg_bulksmserror_.jmcg_name, ex.Message.Left(XrmService.GetMaxLength(Fields.jmcg_bulksmserror_.jmcg_name, Entities.jmcg_bulksmserror)));
                        smsError.SetField(Fields.jmcg_bulksmserror_.jmcg_errordetails, ex.XrmDisplayString().Left(XrmService.GetMaxLength(Fields.jmcg_bulksmserror_.jmcg_errordetails, Entities.jmcg_bulksmserror)));
                        smsError.SetField(Fields.jmcg_bulksmserror_.jmcg_sendprocessid, thisProcessId);
                        smsError.SetLookupField(Fields.jmcg_bulksmserror_.jmcg_bulksms, bulkSms);
                        smsError.SetLookupField(Fields.jmcg_bulksmserror_.jmcg_contact, contact);
                        smsError.Id = XrmService.Create(smsError);
                    }

                    if (escapeSandboxIsolation && DateTime.UtcNow - startedAt > new TimeSpan(0, 0, sandboxIsolationSeconds - sandboxLeeway))
                        return false;
                }
            }
            catch (Exception ex)
            {
                bulkSms.SetField(Fields.jmcg_bulksms_.jmcg_fatalerror, ex.XrmDisplayString().Left(XrmService.GetMaxLength(Fields.jmcg_bulksms_.jmcg_fatalerror, Entities.jmcg_bulksms)));
                XrmService.Update(bulkSms, new[] { Fields.jmcg_bulksms_.jmcg_fatalerror });
            }
            bulkSms.SetField(Fields.jmcg_bulksms_.jmcg_sendingmessages, false);
            XrmService.Update(bulkSms, new[] { Fields.jmcg_bulksms_.jmcg_sendingmessages });

            return true;
        }
    }
}
