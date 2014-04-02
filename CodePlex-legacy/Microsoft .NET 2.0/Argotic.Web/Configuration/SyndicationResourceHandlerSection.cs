/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/25/2008	brian.kuhn	Created SyndicationResourceHandlerSection Class
****************************************************************************/
using System;
using System.ComponentModel;
using System.Configuration;

using Argotic.Common;

namespace Argotic.Configuration
{
    /// <summary>
    /// Represents the configuration section used to declarativly configure the <see cref="Argotic.Web.SyndicationResourceHandler"/> class. This class cannot be inheritied.
    /// </summary>
    public sealed class SyndicationResourceHandlerSection : ConfigurationSection
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the handler syndication format configuration property for the section.
        /// </summary>
        private static readonly ConfigurationProperty configurationSectionFormatProperty            = new ConfigurationProperty("format", typeof(SyndicationContentFormat), SyndicationContentFormat.Rss, new EnumConverter(typeof(SyndicationContentFormat)), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold the updateable within timespan configuration property for the section.
        /// </summary>
        private static readonly ConfigurationProperty configurationSectionUpdatableWithinProperty   = new ConfigurationProperty("updatableWithin", typeof(System.TimeSpan), TimeSpan.FromMinutes(15), new TimeSpanConverter(), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold the valid for timespan configuration property for the section.
        /// </summary>
        private static readonly ConfigurationProperty configurationSectionValidForProperty          = new ConfigurationProperty("validFor", typeof(System.TimeSpan), TimeSpan.FromMinutes(60), new TimeSpanConverter(), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold the enable caching configuration property for the section.
        /// </summary>
        private static readonly ConfigurationProperty configurationSectionEnableCachingProperty     = new ConfigurationProperty("enableCaching", typeof(System.Boolean), true, new BooleanConverter(), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold the unique identifier configuration property for the section.
        /// </summary>
        private static readonly ConfigurationProperty configurationSectionIdentifierProperty        = new ConfigurationProperty("id", typeof(System.String), String.Empty, new StringConverter(), null, ConfigurationPropertyOptions.None);
        /// <summary>
        /// Private member to hold a collection of configuration properties for the section.
        /// </summary>
        private static ConfigurationPropertyCollection configurationSectionProperties               = new ConfigurationPropertyCollection();
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region SyndicationResourceHandlerSection()
        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationResourceHandlerSection"/> class.
        /// </summary>
        public SyndicationResourceHandlerSection()
        {
            //------------------------------------------------------------
            //	Initialize configuration section properties
            //------------------------------------------------------------
            configurationSectionProperties.Add(configurationSectionFormatProperty);
            configurationSectionProperties.Add(configurationSectionUpdatableWithinProperty);
            configurationSectionProperties.Add(configurationSectionValidForProperty);
            configurationSectionProperties.Add(configurationSectionEnableCachingProperty);
            configurationSectionProperties.Add(configurationSectionIdentifierProperty);
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region EnableCaching
        /// <summary>
        /// Gets or sets a value indicating if the syndication resource handler utilizes cache-specific HTTP headers and the output cache.
        /// </summary>
        /// <value><b>true</b> if handler caching is enabled, otherwise returns <b>false</b>. The default value is <b>true</b>.</value>
        [ConfigurationProperty("enableCaching", DefaultValue = "true", Options = ConfigurationPropertyOptions.None)]
        [TypeConverter(typeof(System.Boolean))]
        public bool EnableCaching
        {
            get
            {
                return (bool)base[configurationSectionEnableCachingProperty];
            }

            set
            {
                base[configurationSectionEnableCachingProperty] = value;
            }
        }
        #endregion

        #region Format
        /// <summary>
        /// Gets or sets the default syndication content format processed by the syndication resource handler.
        /// </summary>
        /// <value>
        ///     A <see cref="SyndicationContentFormat"/> enumeration value that indcates the default syndication content format processed by the syndication resource handler. 
        ///     The default value is <see cref="SyndicationContentFormat.Rss"/>, which indicates that by default RSS feed information is retrieved for the application.
        /// </value>
        [ConfigurationProperty("format", DefaultValue = "Rss", Options = ConfigurationPropertyOptions.None)]
        [TypeConverter(typeof(SyndicationContentFormat))]
        public SyndicationContentFormat Format
        {
            get
            {
                return (SyndicationContentFormat)base[configurationSectionFormatProperty];
            }

            set
            {
                base[configurationSectionFormatProperty] = value;
            }
        }
        #endregion

        #region Id
        /// <summary>
        /// Gets or sets the unique identifier of the content that the syndication resource handler should generate.
        /// </summary>
        /// <value>
        ///     An object that represents the unique identifier of the syndication resource served by the syndication resource handler. 
        ///     The default value is <see cref="String.Empty"/>, which indicates that no unique identifier was specified.
        /// </value>
        [ConfigurationProperty("id", DefaultValue = "", Options = ConfigurationPropertyOptions.None)]
        public string Id
        {
            get
            {
                return (string)base[configurationSectionIdentifierProperty];
            }
            set
            {
                base[configurationSectionIdentifierProperty] = value;
            }
        }
        #endregion

        #region UpdatableWithin
        /// <summary>
        /// Gets or sets the period of time after which clients may perform a background check and optional fetch of new content.
        /// </summary>
        /// <value>
        ///     A <see cref="TimeSpan"/> that indicates to the browser that if the content is modified by the server 
        ///     and the resource is requested by the user within the specified time span to <see cref="ValidFor"/> period, 
        ///     the browser displays the information found in the local cache but also performs a background check 
        ///     and optional fetch of the new content on the server. The default value is 15 minutes.
        /// </value>
        [ConfigurationProperty("updatableWithin", DefaultValue = "0:15:0.0", Options = ConfigurationPropertyOptions.None)]
        [TypeConverter(typeof(System.TimeSpan))]
        public TimeSpan UpdatableWithin
        {
            get
            {
                return (TimeSpan)base[configurationSectionUpdatableWithinProperty];
            }

            set
            {
                base[configurationSectionUpdatableWithinProperty] = value;
            }
        }
        #endregion

        #region ValidFor
        /// <summary>
        /// Gets or sets the period of time in which the handler content can be expected to not change.
        /// </summary>
        /// <value>
        ///     A <see cref="TimeSpan"/> that indicates to the browser that the content will not change for the specified time span 
        ///     and instructs it to retrieve the content directly from the local cache. The default value is 60 minutes.
        /// </value>
        [ConfigurationProperty("validFor", DefaultValue = "0:59:0.0", Options = ConfigurationPropertyOptions.None)]
        [TypeConverter(typeof(System.TimeSpan))]
        public TimeSpan ValidFor
        {
            get
            {
                return (TimeSpan)base[configurationSectionValidForProperty];
            }

            set
            {
                base[configurationSectionValidForProperty] = value;
            }
        }
        #endregion

        //============================================================
        //	PROTECTED PROPERTIES
        //============================================================
        #region Properties
        /// <summary>
        /// Gets the configuration properties for this section.
        /// </summary>
        /// <value>A <see cref="ConfigurationPropertyCollection"/> object that represents the configuration properties for this section.</value>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return configurationSectionProperties;
            }
        }
        #endregion
    }
}
