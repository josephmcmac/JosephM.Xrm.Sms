namespace JosephM.Xrm.Sms.Plugins.Services
{
    interface ISmsService
    {
        void SendSms(string mobileNumber, string message, string source);
    }
}
