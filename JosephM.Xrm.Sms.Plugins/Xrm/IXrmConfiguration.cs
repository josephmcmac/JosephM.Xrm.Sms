﻿using Microsoft.Xrm.Sdk.Client;

namespace JosephM.Xrm.Sms.Plugins.Xrm
{
    public interface IXrmConfiguration
    {
        string Name { get; }
        bool UseXrmToolingConnector { get; }
        string ToolingConnectionId { get; }
        AuthenticationProviderType AuthenticationProviderType { get; }
        string DiscoveryServiceAddress { get; }
        string OrganizationUniqueName { get; }
        string Domain { get; }
        string Username { get; }
        string Password { get; }
    }
}