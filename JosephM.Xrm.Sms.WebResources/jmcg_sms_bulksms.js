jmcg_sms_bulkemail = new function () {
    var that = this;

    this.RunOnLoad = function () {
        smsPageUtility.CommonForm(jmcg_sms_bulkemail.RunOnChange,  jmcg_sms_bulkemail.RunOnSave);
        that.WaitCompletedSending();
        that.RefreshVisibilities();
    };

    this.RunOnChange = function (fieldName) {
        switch (fieldName) {
            case "jmcg_sendmessages":
                that.TriggerSavedSend();
                break;
            case "jmcg_sendingmessages":
                that.WaitCompletedSending();
                break;
            case "jmcg_fatalerror":
                that.RefreshVisibilities();
                break;
        }
    };

    this.RunOnSave = function () {
    };

    this.TriggerCreateActivityEscape = false;
    this.TriggerSavedSend = function () {
        if (smsPageUtility.GetFieldValue("jmcg_sendmessages") == true)
            smsPageUtility.SaveRecord();
    };

    this.WaitCompletedSending = function () {
        var isSending = smsPageUtility.GetFieldValue("jmcg_sendingmessages");
        if (isSending) {
            smsPageUtility.DisplayLoading("Please Wait While Sending Messages");
            setTimeout(function () { Xrm.Page.data.refresh().then(that.WaitCompletedSending, that.WaitCompletedSending) }, 5000);
        }
        else {
            smsPageUtility.CloseLoading();
        }
    };

    this.RefreshVisibilities = function () {
        smsPageUtility.SetFieldVisibility("jmcg_fatalerror", smsPageUtility.GetFieldValue("jmcg_fatalerror") != null);
    };
}();