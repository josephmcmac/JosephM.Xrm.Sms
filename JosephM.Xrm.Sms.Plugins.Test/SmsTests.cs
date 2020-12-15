using JosephM.Xrm.Sms.Plugins.Xrm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Schema;
using System;
using System.Collections.Generic;

namespace JosephM.Xrm.Sms.Plugins.Test
{
    [TestClass]
    public class SmsTests : JosephMXrmTest
    {
        /// <summary>
        /// Scripts through sending of an sms
        /// </summary>
        [TestMethod]
        public void SmsSendAndCompleteTest()
        {
            if (JosephMSmsSettings.ActuallySendSms)
                Assert.Inconclusive("Block Test Execution If Sending Of SMS Is On");

            //sms will send when send and complete field set true
            //go through the variuous validations performed prior to send
            //then verify the sms is set completed by the send and complete field
            //when validation for sending an sms passes
            var sms = CreateTestRecord(Entities.jmcg_sms, new Dictionary<string, object>
            {
                { Fields.jmcg_sms_.subject, "Test Scipt Sms" },
            });

            sms.SetField(Fields.jmcg_sms_.jmcg_sendandcomplete, true);
            try
            {
                sms = UpdateFieldsAndRetreive(sms, Fields.jmcg_sms_.to, Fields.jmcg_sms_.jmcg_sendandcomplete);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsFalse(ex is AssertFailedException);
            }
            sms.AddActivityParty(Fields.jmcg_sms_.to, TestContactAccount.LogicalName, TestContactAccount.Id);
            sms = UpdateFieldsAndRetreive(sms, Fields.jmcg_sms_.to);
            sms.SetField(Fields.jmcg_sms_.jmcg_sendandcomplete, true);
            try
            {
                sms = UpdateFieldsAndRetreive(sms, Fields.jmcg_sms_.to, Fields.jmcg_sms_.jmcg_sendandcomplete);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsFalse(ex is AssertFailedException);
            }
            TestContact.SetField(Fields.contact_.mobilephone, null);
            TestContact = UpdateFieldsAndRetreive(TestContact, Fields.contact_.mobilephone);

            sms.SetField(Fields.jmcg_sms_.to, null);
            sms.AddActivityParty(Fields.jmcg_sms_.to, TestContact.LogicalName, TestContact.Id);
            sms = UpdateFieldsAndRetreive(sms, Fields.jmcg_sms_.to);
            sms.SetField(Fields.jmcg_sms_.jmcg_sendandcomplete, true);
            try
            {
                sms = UpdateFieldsAndRetreive(sms, Fields.jmcg_sms_.to, Fields.jmcg_sms_.jmcg_sendandcomplete);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsFalse(ex is AssertFailedException);
            }
            TestContact.SetField(Fields.contact_.mobilephone, "61438570301");
            TestContact = UpdateFieldsAndRetreive(TestContact, Fields.contact_.mobilephone);
            sms.SetField(Fields.jmcg_sms_.jmcg_sendandcomplete, true);
            try
            {
                sms = UpdateFieldsAndRetreive(sms, Fields.jmcg_sms_.to, Fields.jmcg_sms_.jmcg_sendandcomplete);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsFalse(ex is AssertFailedException);
            }
            sms.SetField(Fields.jmcg_sms_.description, "SMS Content");
            sms = UpdateFieldsAndRetreive(sms, Fields.jmcg_sms_.description);

            sms.SetField(Fields.jmcg_sms_.jmcg_sendandcomplete, true);
            sms = UpdateFieldsAndRetreive(sms, Fields.jmcg_sms_.to, Fields.jmcg_sms_.jmcg_sendandcomplete);
            Assert.AreEqual(OptionSets.SMS.ActivityStatus.Completed, sms.GetOptionSetValue(Fields.jmcg_sms_.statecode));

            //sendimmediatelly
            sms = new Entity(Entities.jmcg_sms);
            sms.SetField(Fields.jmcg_sms_.subject, "Test Scipt Sms On Create");
            sms.SetField(Fields.jmcg_sms_.jmcg_sendandcomplete, true);
            sms.SetField(Fields.jmcg_sms_.description, "SMS Content");
            sms.AddToParty(TestContact.LogicalName, TestContact.Id);
            sms = CreateAndRetrieve(sms);
            Assert.AreEqual(OptionSets.SMS.ActivityStatus.Completed, sms.GetOptionSetValue(Fields.jmcg_sms_.statecode));

            DeleteMyToday();
        }

        [TestMethod]
        public void SmsSetSentFieldOnRegardingTest()
        {
            if (JosephMSmsSettings.ActuallySendSms)
                Assert.Inconclusive("Block Test Execution If Sending Of SMS Is On");

            if (TestContact.GetBoolean(Fields.contact_.isbackofficecustomer))
            {
                TestContact.SetField(Fields.contact_.isbackofficecustomer, false);
                TestContact = UpdateFieldsAndRetreive(TestContact, Fields.contact_.isbackofficecustomer);
            }

            var sms = new Entity(Entities.jmcg_sms);
            sms.SetField(Fields.jmcg_sms_.subject, "Test Scipt Sms On Create");
            sms.SetField(Fields.jmcg_sms_.jmcg_sendandcomplete, true);
            sms.SetField(Fields.jmcg_sms_.description, "SMS Content");
            sms.SetLookupField(Fields.jmcg_sms_.regardingobjectid, TestContact);
            sms.SetField(Fields.jmcg_sms_.jmcg_setsentonregardingfield, Fields.contact_.isbackofficecustomer);
            sms.AddToParty(TestContact.LogicalName, TestContact.Id);
            sms = CreateAndRetrieve(sms);
            Assert.AreEqual(OptionSets.SMS.ActivityStatus.Completed, sms.GetOptionSetValue(Fields.jmcg_sms_.statecode));
            TestContact = Refresh(TestContact);
            Assert.IsTrue(TestContact.GetBoolean(Fields.contact_.isbackofficecustomer));

            if (TestContact.GetField(Fields.contact_.lastusedincampaign) != null)
            {
                TestContact.SetField(Fields.contact_.lastusedincampaign, null);
                TestContact = UpdateFieldsAndRetreive(TestContact, Fields.contact_.lastusedincampaign);
            }

            sms = new Entity(Entities.jmcg_sms);
            sms.SetField(Fields.jmcg_sms_.subject, "Test Scipt Sms On Create");
            sms.SetField(Fields.jmcg_sms_.jmcg_sendandcomplete, true);
            sms.SetField(Fields.jmcg_sms_.description, "SMS Content");
            sms.SetLookupField(Fields.jmcg_sms_.regardingobjectid, TestContact);
            sms.SetField(Fields.jmcg_sms_.jmcg_setsentonregardingfield, Fields.contact_.lastusedincampaign);
            sms.AddToParty(TestContact.LogicalName, TestContact.Id);
            sms = CreateAndRetrieve(sms);
            Assert.AreEqual(OptionSets.SMS.ActivityStatus.Completed, sms.GetOptionSetValue(Fields.jmcg_sms_.statecode));
            TestContact = Refresh(TestContact);
            Assert.IsNotNull(TestContact.GetField(Fields.contact_.lastusedincampaign));

            DeleteMyToday();
        }
    }
}