using JosephM.Xrm.Sms.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using Schema;
using System;

namespace JosephM.Xrm.Sms.Plugins.Localisation
{
    public class UserLocalisationSettings : ILocalisationSettings
    {
        public UserLocalisationSettings(XrmService xrmService, Guid userId)
        {
            XrmService = xrmService;
            CurrentUserId = userId;
        }

        private int? _userTimeZoneCode;
        private int UserTimeZoneCode
        {
            get
            {
                if (!_userTimeZoneCode.HasValue)
                {
                    var userSettings = XrmService.GetFirst(Entities.usersettings, Fields.usersettings_.systemuserid, CurrentUserId, new[] { Fields.usersettings_.timezonecode });
                    if (userSettings == null)
                        throw new NullReferenceException(string.Format("Error getting {0} for user ", XrmService.GetEntityDisplayName(Entities.usersettings)));
                    if (userSettings.GetField(Fields.usersettings_.timezonecode) == null)
                        throw new NullReferenceException(string.Format("Error {0} is empty in the {1} record", XrmService.GetFieldLabel(Fields.usersettings_.timezonecode, Entities.usersettings), XrmService.GetEntityDisplayName(Entities.usersettings)));


                    _userTimeZoneCode = userSettings.GetInt(Fields.usersettings_.timezonecode);
                }
                return _userTimeZoneCode.Value;
            }
        }

        private Entity _timeZone;
        private Entity TimeZone
        {
            get
            {
                if (_timeZone == null)
                {
                    _timeZone = XrmService.GetFirst(Entities.timezonedefinition, Fields.timezonedefinition_.timezonecode, UserTimeZoneCode, new[] { Fields.timezonedefinition_.standardname });
                }
                return _timeZone;
            }
        }

        public XrmService XrmService { get; }
        public Guid CurrentUserId { get; }

        public string TargetTimeZoneId => TimeZone.GetStringField(Fields.timezonedefinition_.standardname);
    }
}