using JosephM.Xrm.Sms.Plugins.Xrm;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Sms.Plugins.Test
{
    [TestClass]
    public class BulkSmsTests : JosephMXrmTest
    {
        //[TestMethod]
        //public void BulkSmsSendDebugTests()
        //{
        //    var id = new Guid("4455107c-6d3e-eb11-8158-000c290a70aa");
        //    JosephMSmsService.ProcessBulkSmsSending(id);
        //}

        /// <summary>
        /// Scripts through a bulk sms message
        /// with initial error, fix then retry
        /// </summary>
        [TestMethod]
        public void BulkSmsSendStaticTests()
        {
            var bulkSms = CreateTestRecord(Entities.jmcg_bulksms);
            //validate sms content populated when send
            try
            {
                bulkSms.SetField(Fields.jmcg_bulksms_.jmcg_sendmessages, true);
                bulkSms = UpdateFieldsAndRetreive(bulkSms, Fields.jmcg_bulksms_.jmcg_sendmessages);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsFalse(ex is AssertFailedException);
            }
            bulkSms.SetField(Fields.jmcg_bulksms_.jmcg_smscontent, "Test Script Bulk SMS Content");
            bulkSms = UpdateFieldsAndRetreive(bulkSms, Fields.jmcg_bulksms_.jmcg_smscontent);
            //validate marketing list populated when send
            try
            {
                bulkSms.SetField(Fields.jmcg_bulksms_.jmcg_sendmessages, true);
                bulkSms = UpdateFieldsAndRetreive(bulkSms, Fields.jmcg_bulksms_.jmcg_sendmessages);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsFalse(ex is AssertFailedException);
            }
            bulkSms.SetLookupField(Fields.jmcg_bulksms_.jmcg_marketinglist, TestAccountMarketingList);
            bulkSms = UpdateFieldsAndRetreive(bulkSms, Fields.jmcg_bulksms_.jmcg_marketinglist);
            //validate marketing list for contacts when send
            try
            {
                bulkSms.SetField(Fields.jmcg_bulksms_.jmcg_sendmessages, true);
                bulkSms = UpdateFieldsAndRetreive(bulkSms, Fields.jmcg_bulksms_.jmcg_sendmessages);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsFalse(ex is AssertFailedException);
            }
            //okay now set to a contact marketing list
            //should be valid for send
            //lets add a contact with a mobile number
            //and one without a mobile number
            bulkSms.SetLookupField(Fields.jmcg_bulksms_.jmcg_marketinglist, TestContactMarketingList);
            bulkSms = UpdateFieldsAndRetreive(bulkSms, Fields.jmcg_bulksms_.jmcg_marketinglist);
            //clear the contact list
            var marketingListMembers = XrmService.GetMarketingListMemberIds(TestContactMarketingList.Id);
            foreach (var memberId in marketingListMembers)
            {
                RemoveListMember(memberId, TestContactMarketingList.Id);
            }
            //add test contact with mobile number
            if (string.IsNullOrWhiteSpace(TestContact.GetStringField(Fields.contact_.mobilephone)))
            {
                TestContact.SetField(Fields.contact_.mobilephone, "0438570301");
                TestContact = UpdateFieldsAndRetreive(TestContact, Fields.contact_.mobilephone);
            }
            AddListMember(TestContact.Id, TestContactMarketingList.Id);
            //add test contact2 without mobile number
            if (!string.IsNullOrWhiteSpace(TestContact2.GetStringField(Fields.contact_.mobilephone)))
            {
                TestContact2.SetField(Fields.contact_.mobilephone, null);
                TestContact2 = UpdateFieldsAndRetreive(TestContact2, Fields.contact_.mobilephone);
            }
            AddListMember(TestContact2.Id, TestContactMarketingList.Id);
            //send sms including error
            bulkSms.SetField(Fields.jmcg_bulksms_.jmcg_sendmessages, true);
            bulkSms = UpdateFieldsAndRetreive(bulkSms, Fields.jmcg_bulksms_.jmcg_sendmessages);
            Assert.IsTrue(bulkSms.GetBoolean(Fields.jmcg_bulksms_.jmcg_sendingmessages));
            Assert.IsFalse(bulkSms.GetBoolean(Fields.jmcg_bulksms_.jmcg_sendmessages));
            WaitTillTrue(() => !Refresh(bulkSms).GetBoolean(Fields.jmcg_bulksms_.jmcg_sendingmessages), 60);

            var smsMessages = GetSmsMessagesSent(bulkSms);
            var smsErrors = GetSmsErrorsActive(bulkSms);
            Assert.AreEqual(1, smsMessages.Count());
            Assert.AreEqual(TestContact.Id, smsMessages.First().GetActivityPartyReferences(Fields.jmcg_sms_.to).First().Id);
            Assert.AreEqual(1, smsErrors.Count());
            Assert.AreEqual(TestContact2.Id, smsErrors.First().GetLookupGuid(Fields.jmcg_bulksmserror_.jmcg_contact));
            
            //fix error and reprocess

            //populate mobile number
            TestContact2.SetField(Fields.contact_.mobilephone, "0438570301");
            TestContact2 = UpdateFieldsAndRetreive(TestContact2, Fields.contact_.mobilephone);
            //resend
            bulkSms.SetField(Fields.jmcg_bulksms_.jmcg_sendmessages, true);
            bulkSms = UpdateFieldsAndRetreive(bulkSms, Fields.jmcg_bulksms_.jmcg_sendmessages);
            Assert.IsTrue(bulkSms.GetBoolean(Fields.jmcg_bulksms_.jmcg_sendingmessages));
            Assert.IsFalse(bulkSms.GetBoolean(Fields.jmcg_bulksms_.jmcg_sendmessages));
            WaitTillTrue(() => !Refresh(bulkSms).GetBoolean(Fields.jmcg_bulksms_.jmcg_sendingmessages), 60);

            //verify now 2 sms messages
            //and no active errors
            smsMessages = GetSmsMessagesSent(bulkSms);
            smsErrors = GetSmsErrorsActive(bulkSms);
            Assert.AreEqual(2, smsMessages.Count());
            Assert.AreEqual(1, smsMessages.Count(e => e.GetActivityPartyReferences(Fields.jmcg_sms_.to).First().Id == TestContact.Id));
            Assert.AreEqual(1, smsMessages.Count(e => e.GetActivityPartyReferences(Fields.jmcg_sms_.to).First().Id == TestContact2.Id));
            Assert.AreEqual(0, smsErrors.Count());

            DeleteMyToday();
        }

        /// <summary>
        /// Scripts through a bulk sms message
        /// with initial error, fix then retry
        /// </summary>
        [TestMethod]
        public void BulkSmsSendDynamicTests()
        {
            if (string.IsNullOrWhiteSpace(TestContact.GetStringField(Fields.contact_.mobilephone)))
            {
                TestContact.SetField(Fields.contact_.mobilephone, "0438570301");
                TestContact = UpdateFieldsAndRetreive(TestContact, Fields.contact_.mobilephone);
            }
            if (!string.IsNullOrWhiteSpace(TestContact2.GetStringField(Fields.contact_.mobilephone)))
            {
                TestContact2.SetField(Fields.contact_.mobilephone, null);
                TestContact2 = UpdateFieldsAndRetreive(TestContact2, Fields.contact_.mobilephone);
            }

            var bulkSms = CreateTestRecord(Entities.jmcg_bulksms, new Dictionary<string, object>
            {
                {  Fields.jmcg_bulksms_.jmcg_name, "Test Script Bulk SMS Content"},
                {  Fields.jmcg_bulksms_.jmcg_smscontent, "Test Script Bulk SMS Content"},
                {  Fields.jmcg_bulksms_.jmcg_marketinglist, TestDynamicContactMarketingList.ToEntityReference()},
                {  Fields.jmcg_bulksms_.jmcg_sendmessages, true},
            });
            Assert.IsTrue(bulkSms.GetBoolean(Fields.jmcg_bulksms_.jmcg_sendingmessages));
            Assert.IsFalse(bulkSms.GetBoolean(Fields.jmcg_bulksms_.jmcg_sendmessages));
            WaitTillTrue(() => !Refresh(bulkSms).GetBoolean(Fields.jmcg_bulksms_.jmcg_sendingmessages), 60);

            var smsMessages = GetSmsMessagesSent(bulkSms);
            var smsErrors = GetSmsErrorsActive(bulkSms);
            Assert.AreEqual(1, smsMessages.Count());
            Assert.AreEqual(TestContact.Id, smsMessages.First().GetActivityPartyReferences(Fields.jmcg_sms_.to).First().Id);
            Assert.AreEqual(1, smsErrors.Count());
            Assert.AreEqual(TestContact2.Id, smsErrors.First().GetLookupGuid(Fields.jmcg_bulksmserror_.jmcg_contact));

            //fix error and reprocess

            //populate mobile number
            TestContact2.SetField(Fields.contact_.mobilephone, "0438570301");
            TestContact2 = UpdateFieldsAndRetreive(TestContact2, Fields.contact_.mobilephone);
            //resend
            bulkSms.SetField(Fields.jmcg_bulksms_.jmcg_sendmessages, true);
            bulkSms = UpdateFieldsAndRetreive(bulkSms, Fields.jmcg_bulksms_.jmcg_sendmessages);
            Assert.IsTrue(bulkSms.GetBoolean(Fields.jmcg_bulksms_.jmcg_sendingmessages));
            Assert.IsFalse(bulkSms.GetBoolean(Fields.jmcg_bulksms_.jmcg_sendmessages));
            WaitTillTrue(() => !Refresh(bulkSms).GetBoolean(Fields.jmcg_bulksms_.jmcg_sendingmessages), 60);

            //verify now 2 sms messages
            //and no active errors
            smsMessages = GetSmsMessagesSent(bulkSms);
            smsErrors = GetSmsErrorsActive(bulkSms);
            Assert.AreEqual(2, smsMessages.Count());
            Assert.AreEqual(1, smsMessages.Count(e => e.GetActivityPartyReferences(Fields.jmcg_sms_.to).First().Id == TestContact.Id));
            Assert.AreEqual(1, smsMessages.Count(e => e.GetActivityPartyReferences(Fields.jmcg_sms_.to).First().Id == TestContact2.Id));
            Assert.AreEqual(0, smsErrors.Count());

            DeleteMyToday();
        }

        private Entity _testContactMarketingList;
        public Entity TestContactMarketingList
        {
            get
            {
                if (_testContactMarketingList == null)
                {
                    _testContactMarketingList = XrmService.GetFirst(Entities.list, Fields.list_.listname, "Test Contact Marketing List");
                    if (_testContactMarketingList == null)
                    {
                        _testContactMarketingList = CreateTestRecord(Entities.list, new Dictionary<string, object>
                        {
                            { Fields.list_.listname, "Test Contact Marketing List" },
                            { Fields.list_.membertype, OptionSets.MarketingList.MarketingListMemberType.Contact },
                            { Fields.list_.type, false }, //static
                            { Fields.list_.createdfromcode, new OptionSetValue(OptionSets.MarketingList.MarketingListMemberType.Contact) }
                        });
                    }
                }
                return _testContactMarketingList;
            }
        }

        private Entity _testAccountMarketingList;
        public Entity TestAccountMarketingList
        {
            get
            {
                if (_testAccountMarketingList == null)
                {
                    _testAccountMarketingList = XrmService.GetFirst(Entities.list, Fields.list_.listname, "Test Account Marketing List");
                    if (_testAccountMarketingList == null)
                    {
                        _testAccountMarketingList = CreateTestRecord(Entities.list, new Dictionary<string, object>
                        {
                            { Fields.list_.listname, "Test Account Marketing List" },
                            { Fields.list_.membertype, OptionSets.MarketingList.MarketingListMemberType.Account },
                            { Fields.list_.type, false }, //static
                            { Fields.list_.createdfromcode, new OptionSetValue(OptionSets.MarketingList.MarketingListMemberType.Account) }
                        });
                    }
                }
                return _testAccountMarketingList;
            }
        }

        private Entity _testDynamicContactMarketingList;
        public Entity TestDynamicContactMarketingList
        {
            get
            {
                if (_testDynamicContactMarketingList == null)
                {
                    _testDynamicContactMarketingList = XrmService.GetFirst(Entities.list, Fields.list_.listname, "Test Dynamic Contact Marketing List");
                    if (_testDynamicContactMarketingList == null)
                    {
                        _testDynamicContactMarketingList = CreateTestRecord(Entities.list, new Dictionary<string, object>
                        {
                            { Fields.list_.listname, "Test Dynamic Contact Marketing List" },
                            { Fields.list_.membertype, OptionSets.MarketingList.MarketingListMemberType.Contact },
                            { Fields.list_.type, true }, //dynamic
                            { Fields.list_.createdfromcode, new OptionSetValue(OptionSets.MarketingList.MarketingListMemberType.Contact) },
                            { Fields.list_.query, "<fetch version=\"1.0\" output-format=\"xml-platform\" mapping=\"logical\" distinct=\"false\"><entity name=\"contact\"><attribute name=\"fullname\" /><attribute name=\"telephone1\" /><attribute name=\"contactid\" /><order attribute=\"fullname\" descending=\"false\" /><filter type=\"and\"><condition attribute=\"contactid\" operator=\"in\"><value uiname=\"TEST SCRIPT CONTACT\" uitype=\"contact\">" + TestContact.Id + "</value><value uiname=\"TEST SCRIPT CONTACT 2\" uitype=\"contact\">" + TestContact2.Id + "</value></condition></filter></entity></fetch>" }
                        });
                    }
                }
                return _testDynamicContactMarketingList;
            }
        }

        //

        private IEnumerable<Entity> GetSmsErrorsActive(Entity bulkSms)
        {
            return XrmService.RetrieveAllAndConditions(Entities.jmcg_bulksmserror, new[]
            {
                new ConditionExpression(Fields.jmcg_bulksmserror_.jmcg_bulksms, ConditionOperator.Equal, bulkSms.Id),
                new ConditionExpression(Fields.jmcg_bulksmserror_.statecode, ConditionOperator.Equal, OptionSets.BulkSMSError.Status.Active)
            });
        }

        private IEnumerable<Entity> GetSmsMessagesSent(Entity bulkSms)
        {
            return XrmService.RetrieveAllAndConditions(Entities.jmcg_sms, new[]
            {
                new ConditionExpression(Fields.jmcg_sms_.jmcg_bulksms, ConditionOperator.Equal, bulkSms.Id),
                new ConditionExpression(Fields.jmcg_sms_.statecode, ConditionOperator.Equal, OptionSets.SMS.ActivityStatus.Completed)
            });
        }

        private void AddListMember(Guid memberId, Guid listId)
        {
            var remove = new AddMemberListRequest
            {
                EntityId = memberId,
                ListId = listId
            };
            XrmService.Execute(remove);
        }

        private void RemoveListMember(Guid memberId, Guid listId)
        {
            var remove = new RemoveMemberListRequest
            {
                EntityId = memberId,
                ListId = listId
            };
            XrmService.Execute(remove);
        }
    }
}