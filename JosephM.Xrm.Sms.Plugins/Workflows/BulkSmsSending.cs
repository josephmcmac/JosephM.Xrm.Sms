using JosephM.Xrm.Sms.Plugins.Xrm;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;

namespace JosephM.Xrm.Sms.Plugins.Workflows
{
    /// <summary>
    /// This class is for the static type required for registration of the custom workflow activity in CRM
    /// </summary>
    public class BulkSmsSending : XrmWorkflowActivityRegistration
    {
        [Output("Is Completed")]
        public OutArgument<bool> IsCompleted { get; set; }
        protected override XrmWorkflowActivityInstanceBase CreateInstance()
        {
            return new BulkSmsSendingInstance();
        }
    }

    /// <summary>
    /// This class is instantiated per execution
    /// </summary>
    public class BulkSmsSendingInstance
        : JosephMSmsWorkflowActivity<BulkSmsSending>
    {
        protected override void Execute()
        {
            var isCompleted = JosephMSmsService.ProcessBulkSmsSending(TargetId);
            ActivityThisType.IsCompleted.Set(ExecutionContext, isCompleted);
        }
    }
}