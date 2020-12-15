using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Xrm.Sms.Plugins.Test
{
    //this class just for general debug purposes
    [TestClass]
    public class DebugTests : JosephMXrmTest
    {
        [TestMethod]
        public void Debug()
        {
            var me = XrmService.WhoAmI();
        }
    }
}