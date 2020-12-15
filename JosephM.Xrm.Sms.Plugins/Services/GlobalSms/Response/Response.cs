﻿using System.Web.Script.Serialization;

/// <summary>
/// The response namespace.
/// </summary>
namespace SMSGlobal.SMS.Response
{
    /// <summary>
    /// The response class.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Converts the response to a string representation
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }
}
