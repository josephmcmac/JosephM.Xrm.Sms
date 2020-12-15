using Microsoft.Xrm.Sdk;
using JosephM.Xrm.Sms.Plugins.Xrm;
using Schema;
using System;
using System.Collections.Generic;

namespace JosephM.Xrm.Sms.Plugins.Rollups
{
    public class JosephMRollupService : RollupService
    {
        public JosephMRollupService(XrmService xrmService)
            : base(xrmService)
        {
        }

        private IEnumerable<LookupRollup> _Rollups = new LookupRollup[]
        {
        };

        public override IEnumerable<LookupRollup> AllRollups => _Rollups;
    }
}