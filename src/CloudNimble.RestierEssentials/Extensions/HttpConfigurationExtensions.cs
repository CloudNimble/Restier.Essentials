using System;
using System.Web.Http;

namespace CloudNimble.Restier.Essentials
{

    /// <summary>
    /// Extension methods for configuring the behavior of the RestierExtensions.
    /// </summary>
    public static class HttpConfigurationExtensions
    {

        /// <summary>
        /// When used in conjunction with the SoftDeleteSubmitExecutor, sets the name of the boolean property specifying whether or not the item is considered deleted.
        /// </summary>
        /// <param name="configuration">The instance of the HttpConfiguration that is being modified.</param>
        /// <param name="propertyName">The name of the property on each Entity that specifies whether or not the Entity is considered deleted.</param>
        /// <param name="adminRole">An optional string specifying the Administrative role name that can perform a hard-delete.</param>
        /// <returns></returns>
        public static HttpConfiguration CheckSoftDeleteProperty(this HttpConfiguration configuration, string propertyName, string adminRole = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException(propertyName);
            }

            configuration.Properties[SoftDeleteSubmitExecutor.SoftDeletePropertyNameKey] = propertyName;

            if (!string.IsNullOrWhiteSpace(adminRole))
            {
                configuration.Properties[SoftDeleteSubmitExecutor.SoftDeleteAdminRoleKey] = adminRole;
            }
            return configuration;
        }

    }

}