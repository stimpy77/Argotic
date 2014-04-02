/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/25/2008	brian.kuhn	Created PrivilegedConfigurationManager Class
****************************************************************************/
using System;
using System.Configuration;
using System.Security.Permissions;
using System.Threading;

using Argotic.Common;

namespace Argotic.Configuration
{
    /// <summary>
    /// Provides privileged access to configuration files for web applications. This class cannot be inherited.
    /// </summary>
    [ConfigurationPermission(SecurityAction.Assert, Unrestricted = true)]
    internal static class PrivilegedConfigurationManager
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS       
        /// <summary>
        /// Private member to hold object used to synchronize locks when reading syndication handler configuration information.
        /// </summary>
        private static object configurationManagerSyndicationResourceHandlerSyncObject;
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region HandlerSyncObject
        /// <summary>
        /// Gets the <see cref="Object"/> used when locking acess to the syndication handler configuration file section being managed.
        /// </summary>
        /// <value>The <see cref="Object"/> used when locking acess to the syndication handler configuration file section being managed.</value>
        internal static object HandlerSyncObject
        {
            get
            {
                if (configurationManagerSyndicationResourceHandlerSyncObject == null)
                {
                    Interlocked.CompareExchange(ref configurationManagerSyndicationResourceHandlerSyncObject, new object(), null);
                }
                return configurationManagerSyndicationResourceHandlerSyncObject;
            }
        }
        #endregion

        //============================================================
        //	GENERIC METHODS
        //============================================================
        #region GetSection(string sectionName)
        /// <summary>
        /// Retrieves a specified configuration section for the current application's default configuration.
        /// </summary>
        /// <param name="sectionName">The configuration section path and name.</param>
        /// <returns>The specified ConfigurationSection object, or a null reference (Nothing in Visual Basic) if the section does not exist.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="sectionName"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="sectionName"/> is an empty string.</exception>
        /// <exception cref="ConfigurationErrorsException">A configuration file could not be loaded.</exception>
        internal static object GetSection(string sectionName)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(sectionName, "sectionName");

            return ConfigurationManager.GetSection(sectionName);
        }
        #endregion

        //============================================================
        //	HANDLER METHODS
        //============================================================
        #region GetSyndicationHandlerSection()
        /// <summary>
        /// Returns the syndication handler configuration section information.
        /// </summary>
        /// <returns>
        ///     A <see cref="SyndicationResourceHandlerSection"/> object that represents the syndication handler configuration information. 
        ///     If no configuration section is defined for syndication handlers, returns a <b>null</b> reference.
        /// </returns>
        [SecurityPermission(SecurityAction.Demand)]
        internal static SyndicationResourceHandlerSection GetSyndicationHandlerSection()
        {
            string sectionPath  = "argotic.web.httpHandler";

            lock (PrivilegedConfigurationManager.HandlerSyncObject)
            {
                SyndicationResourceHandlerSection section   = PrivilegedConfigurationManager.GetSection(sectionPath) as SyndicationResourceHandlerSection;
                if (section == null)
                {
                    return null;
                }
                return section;
            }
        }
        #endregion
    }
}
