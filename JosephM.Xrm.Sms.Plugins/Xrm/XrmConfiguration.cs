﻿using Microsoft.Xrm.Sdk.Client;

namespace JosephM.Xrm.Sms.Plugins.Xrm
{
    public class XrmConfiguration : IXrmConfiguration
    {
        public string Name { get; set; }
        public bool UseXrmToolingConnector { get; set; }
        public string ToolingConnectionId { get; set; }
        public virtual AuthenticationProviderType AuthenticationProviderType { get; set; }
        public virtual string DiscoveryServiceAddress { get; set; }
        public virtual string OrganizationUniqueName { get; set; }
        public virtual string Domain { get; set; }
        public virtual string Username { get; set; }
        public virtual string Password { get; set; }
    }
}