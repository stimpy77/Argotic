/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/24/2008	brian.kuhn	Created SyndicationResourceHandler Class
****************************************************************************/
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Web;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Configuration;
using Argotic.Syndication;
using Argotic.Syndication.Specialized;

namespace Argotic.Web
{
    /// <summary>
    /// Synchronously processes HTTP Web requests for syndicated content.
    /// </summary>
    /// <seealso cref="SyndicationResourceHandlerFactory"/>
    public class SyndicationResourceHandler : IHttpHandler
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold a value indicating whether another request can use the handler.
        /// </summary>
        private bool handlerIsReusable;
        /// <summary>
        /// Private member to hold the type of syndication format that this syndication resource handler processes.
        /// </summary>
        private SyndicationContentFormat handlerFormat  = SyndicationContentFormat.None;
        /// <summary>
        /// Private member to hold the period of time in which the handler content is expected to not change.
        /// </summary>
        private TimeSpan handlerContentValidFor         = TimeSpan.FromMinutes(60);
        /// <summary>
        /// Private member to hold the period of time after which clients may perform a background check and optional fetch of new content.
        /// </summary>
        private TimeSpan handlerContentUpdatableWithin  = TimeSpan.FromMinutes(15);
        /// <summary>
        /// Private member to hold a value indicating if the handler utilizes the HTTP caching policy.
        /// </summary>
        private bool handlerHasCachingEnabled           = true;
        /// <summary>
        /// Private member to hold the unique identifier of the syndication resource served by the handler.
        /// </summary>
        private object handlerResourceId;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region SyndicationResourceHandler()
        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationResourceHandler"/> class.
        /// </summary>
        public SyndicationResourceHandler()
        {
            //------------------------------------------------------------
            //	Attempt to initialize handler state using configuration settings
            //------------------------------------------------------------
            this.Initialize();
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region ContentUpdatableWithin
        /// <summary>
        /// Gets or sets the period of time after which clients may perform a background check and optional fetch of new content.
        /// </summary>
        /// <value>
        ///     A <see cref="TimeSpan"/> that indicates to the browser that if the content is modified by the server 
        ///     and the resource is requested by the user within the specified time span to <see cref="ContentValidFor"/> period, 
        ///     the browser displays the information found in the local cache but also performs a background check 
        ///     and optional fetch of the new content on the server. The default value is 15 minutes.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         The initialization order of precedence is as follows:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     Application settings: If the <b>httpHandler</b> configuration section defines a <i>updatableWithin</i> attribute, 
        ///                     its value takes precedence.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     Runtime/programmatic: If content update period is not specified via application configuration, the property setter value takes precedence.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        ///     <para>This property corresponds to the value of the <i>post-check</i> cache extension.</para>
        /// </remarks>
        public TimeSpan ContentUpdatableWithin
        {
            get
            {
                return handlerContentUpdatableWithin;
            }

            set
            {
                handlerContentUpdatableWithin = value;
            }
        }
        #endregion

        #region ContentValidFor
        /// <summary>
        /// Gets or sets the period of time in which the handler content can be expected to not change.
        /// </summary>
        /// <value>
        ///     A <see cref="TimeSpan"/> that indicates to the browser that the content will not change for the specified time span 
        ///     and instructs it to retrieve the content directly from the local cache. The default value is 60 minutes.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         The initialization order of precedence is as follows:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     Application settings: If the <b>httpHandler</b> configuration section defines a <i>validFor</i> attribute, 
        ///                     its value takes precedence.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     Runtime/programmatic: If content expiration period is not specified via application configuration, the property setter value takes precedence.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        ///     <para>This property corresponds to the value of the <i>pre-check</i> cache extension.</para>
        /// </remarks>
        public TimeSpan ContentValidFor
        {
            get
            {
                return handlerContentValidFor;
            }

            set
            {
                handlerContentValidFor = value;
            }
        }
        #endregion

        #region EnableCaching
        /// <summary>
        /// Gets or sets a value indicating if the <see cref="SyndicationResourceHandler"/> utilizes cache-specific HTTP headers and the output cache.
        /// </summary>
        /// <value><b>true</b> if handler caching is enabled, otherwise returns <b>false</b>. The default value is <b>true</b>.</value>
        /// <remarks>
        ///     <para>
        ///         The initialization order of precedence is as follows:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     Application settings: If the <b>httpHandler</b> configuration section defines a <i>enableCaching</i> attribute, 
        ///                     its value takes precedence.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     Runtime/programmatic: If enable of caching is not specified via application configuration, the property setter value takes precedence.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        public bool EnableCaching
        {
            get
            {
                return handlerHasCachingEnabled;
            }

            set
            {
                handlerHasCachingEnabled = value;
            }
        }
        #endregion

        #region Format
        /// <summary>
        /// Gets or sets the <see cref="SyndicationContentFormat"/> that this syndication resource handler processes.
        /// </summary>
        /// <value>
        ///     The <see cref="SyndicationContentFormat"/> enumeration value that indicates the type of syndication format that this syndication resource handler processes. 
        ///     The default value is <see cref="SyndicationContentFormat.None"/>, whcih indicates that no content format was specified or configured.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         The <see cref="Format"/> property determines the syndication content format that this <see cref="SyndicationResourceHandler"/> will attempt to retrieve. 
        ///         The handler format can be specified via the query string or declaratively configured in the application configuration settings.
        ///     </para>
        ///     <para>
        ///         The initialization order of precedence is as follows:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     Request query string parameter: If a <i>format</i> parameter exists, its value takes precedence.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     Application settings: If format was not specified via the query string, 
        ///                     and the <b>httpHandler</b> configuration section defines a <i>format</i> attribute, the configuration value takes precedence.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     Runtime/programmatic: If format was not specified via the query string or via application configuration, 
        ///                     the property setter value takes precedence.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        public SyndicationContentFormat Format
        {
            get
            {
                return handlerFormat;
            }

            set
            {
                handlerFormat = value;
            }
        }
        #endregion

        #region Id
        /// <summary>
        /// Gets or sets the unique identifier of the content that this HTTP handler should generate.
        /// </summary>
        /// <value>An object that represents the unique identifier of the syndication resource served by this HTTP handler.</value>
        /// <remarks>
        ///     <para>
        ///         The initialization order of precedence is as follows:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>
        ///                     Request query string parameter: If an <i>id</i> parameter exists, its value takes precedence.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                    Application settings: If resource identifier was not specified via the query string, 
        ///                     and the <b>httpHandler</b> configuration section defines a <i>id</i> attribute, the configuration value takes precedence.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     Runtime/programmatic: If content identifier was not specified via the query string or via application configuration, 
        ///                     the property setter value takes precedence.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/>is a null reference (Nothing in Visual Basic).</exception>
        public object Id
        {
            get
            {
                return handlerResourceId;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                handlerResourceId = value;
            }
        }
        #endregion

        #region IsReusable
        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="SyndicationResourceHandler"/> instance.
        /// </summary>
        /// <value><b>true</b> if the <see cref="SyndicationResourceHandler"/> instance is reusable; otherwise, <b>false</b>. The default value is <b>false</b>.</value>
        public bool IsReusable
        {
            get
            {
                return handlerIsReusable;
            }

            protected set
            {
                handlerIsReusable = value;
            }
        }
        #endregion

        //============================================================
        //	STATIC METHODS
        //============================================================
        #region ContentFormatAsMimeType(SyndicationContentFormat format)
        /// <summary>
        /// Returns the MIME content type for the supplied <see cref="SyndicationContentFormat"/>.
        /// </summary>
        /// <param name="format">The <see cref="SyndicationContentFormat"/> to get the MIME content type for.</param>
        /// <returns>The MIME content type for the supplied <paramref name="format"/>, otherwise returns <b>text/xml</b>.</returns>
        public static string ContentFormatAsMimeType(SyndicationContentFormat format)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string contentType  = "text/xml";

            //------------------------------------------------------------
            //	Return MIME type based on supplied format
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(SyndicationContentFormat).GetFields())
            {
                if (fieldInfo.FieldType == typeof(SyndicationContentFormat))
                {
                    SyndicationContentFormat contentFormat  = (SyndicationContentFormat)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);

                    if (contentFormat == format)
                    {
                        object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(MimeMediaTypeAttribute), false);

                        if (customAttributes != null && customAttributes.Length > 0)
                        {
                            MimeMediaTypeAttribute mediaType    = customAttributes[0] as MimeMediaTypeAttribute;
                            string mimeType                     = String.Format(null, "{0}/{1}", mediaType.Name, mediaType.SubName);

                            contentType                         = mimeType;
                            break;
                        }
                    }
                }
            }

            return contentType;
        }
        #endregion

        #region ContentFormatByMimeType(string contentType)
        /// <summary>
        /// Returns the <see cref="SyndicationContentFormat"/> enumeration value that corresponds to the specified MIME content type.
        /// </summary>
        /// <param name="contentType">The MIME type of the syndication format.</param>
        /// <returns>A <see cref="SyndicationContentFormat"/> enumeration value that corresponds to the specified string, otherwise returns <b>SyndicationContentFormat.None</b>.</returns>
        /// <remarks>This method disregards case of specified MIME content type.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="contentType"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="contentType"/> is an empty string.</exception>
        public static SyndicationContentFormat ContentFormatByMimeType(string contentType)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            SyndicationContentFormat contentFormat  = SyndicationContentFormat.None;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNullOrEmptyString(contentType, "contentType");

            //------------------------------------------------------------
            //	Determine syndication content format based on supplied name
            //------------------------------------------------------------
            foreach (System.Reflection.FieldInfo fieldInfo in typeof(SyndicationContentFormat).GetFields())
            {
                if (fieldInfo.FieldType == typeof(SyndicationContentFormat))
                {
                    SyndicationContentFormat format = (SyndicationContentFormat)Enum.Parse(fieldInfo.FieldType, fieldInfo.Name);
                    object[] customAttributes       = fieldInfo.GetCustomAttributes(typeof(MimeMediaTypeAttribute), false);

                    if (customAttributes != null && customAttributes.Length > 0)
                    {
                        MimeMediaTypeAttribute mediaType    = customAttributes[0] as MimeMediaTypeAttribute;
                        string mimeType                     = String.Format(null, "{0}/{1}", mediaType.Name, mediaType.SubName);

                        if (String.Compare(contentType, mimeType, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            contentFormat   = format;
                            break;
                        }
                    }
                }
            }

            return contentFormat;
        }
        #endregion

        #region GetContentDisposition(SyndicationContentFormat format)
        /// <summary>
        /// Returns the content disposition header value for the supplied syndication format.
        /// </summary>
        /// <param name="format">The <see cref="SyndicationContentFormat"/> enumeration value to determine the content disposition for.</param>
        /// <returns>
        ///     The content disposition header value that is mapped to the supplied <see cref="SyndicationContentFormat"/>. 
        ///     If no content disposition is mapped to the supplied <paramref name="format"/>, returns an empty string.
        /// </returns>
        private static string GetContentDisposition(SyndicationContentFormat format)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string contentDisposition   = String.Empty;

            //------------------------------------------------------------
            //	Map content disposition to supplied format
            //------------------------------------------------------------
            switch (format)
            {
                case SyndicationContentFormat.Apml:
                    contentDisposition  = "inline; filename=apml.xml";
                    break;

                case SyndicationContentFormat.Atom:
                    contentDisposition  = "inline; filename=atom.xml";
                    break;

                case SyndicationContentFormat.BlogML:
                    contentDisposition  = "inline; filename=blogML.xml";
                    break;

                case SyndicationContentFormat.MicroSummaryGenerator:
                    contentDisposition  = "inline; filename=microSummary.xml";
                    break;

                case SyndicationContentFormat.NewsML:
                    contentDisposition  = "inline; filename=newsML.xml";
                    break;

                case SyndicationContentFormat.OpenSearchDescription:
                    contentDisposition  = "inline; filename=openSearch.xml";
                    break;

                case SyndicationContentFormat.Opml:
                    contentDisposition  = "inline; filename=opml.xml";
                    break;

                case SyndicationContentFormat.Rsd:
                    contentDisposition  = "inline; filename=rsd.xml";
                    break;

                case SyndicationContentFormat.Rss:
                    contentDisposition  = "inline; filename=rss.xml";
                    break;

                default:
                    contentDisposition  = String.Empty;
                    break;
            }

            return contentDisposition;
        }
        #endregion

        #region GetLastModificationDate(ISyndicationResource resource)
        /// <summary>
        /// Returns a date-time indicating the most recent instant in time when the resource was last modified.
        /// </summary>
        /// <param name="resource">The <see cref="ISyndicationResource"/> to determine the modification date for.</param>
        /// <returns>
        ///     A <see cref="DateTime"/> indicating the most recent instant in time when the <see cref="ISyndicationResource"/> was last modified. 
        ///     If the <see cref="ISyndicationResource"/> conforms to a format that does not support modification tracking, or if otherwise unable 
        ///     to determine the modification date, returns <see cref="DateTime.MinValue"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        private static DateTime GetLastModificationDate(ISyndicationResource resource)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            DateTime lastModifiedOn = DateTime.MinValue;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(resource, "resource");

            //------------------------------------------------------------
            //	Determine modification date based on format specific details
            //------------------------------------------------------------
            switch(resource.Format)
            {
                case SyndicationContentFormat.Apml:
                    ApmlDocument apmlDocument   = resource as ApmlDocument;
                    if (apmlDocument != null && apmlDocument.Head != null)
                    {
                        lastModifiedOn  = apmlDocument.Head.CreatedOn;
                    }
                    break;

                case SyndicationContentFormat.Atom:
                    XPathNavigator rootNavigator    = resource.CreateNavigator();
                    if(String.Compare(rootNavigator.LocalName, "entry", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        AtomEntry entry = resource as AtomEntry;
                        if (entry != null)
                        {
                            lastModifiedOn  = entry.UpdatedOn;
                        }
                    }
                    else if (String.Compare(rootNavigator.LocalName, "feed", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        AtomFeed atomFeed   = resource as AtomFeed;
                        if (atomFeed != null)
                        {
                            lastModifiedOn  = atomFeed.UpdatedOn;
                        }
                    }
                    break;

                case SyndicationContentFormat.BlogML:
                    BlogMLDocument blogDocument = resource as BlogMLDocument;
                    if (blogDocument != null)
                    {
                        lastModifiedOn  = blogDocument.GeneratedOn;
                    }
                    break;

                case SyndicationContentFormat.Opml:
                    OpmlDocument opmlDocument = resource as OpmlDocument;
                    if (opmlDocument != null && opmlDocument.Head != null)
                    {
                        lastModifiedOn  = opmlDocument.Head.ModifiedOn;
                    }
                    break;

                case SyndicationContentFormat.Rsd:
                    //  RSD 1.0 does not provide last modified information
                    lastModifiedOn      = DateTime.MinValue;
                    break;

                case SyndicationContentFormat.Rss:
                    RssFeed rssFeed = resource as RssFeed;
                    if (rssFeed != null && rssFeed.Channel != null)
                    {
                        lastModifiedOn  = rssFeed.Channel.LastBuildDate;
                    }
                    break;

                default:
                    lastModifiedOn      = DateTime.MinValue;
                    break;
            }

            return lastModifiedOn;
        }
        #endregion

        #region ProcessConditionalGetInformation(HttpContext context, ISyndicationResource resource)
        /// <summary>
        /// Processes conditional GET header information for the supplied <see cref="ISyndicationResource"/>.
        /// </summary>
        /// <param name="context">
        ///     An <see cref="HttpContext"/> object that provides references to the intrinsic server objects 
        ///     (for example, <b>Request</b>, <b>Response</b>, <b>Session</b>, and <b>Server</b>) used to service HTTP requests.
        /// </param>
        /// <param name="resource">The <see cref="ISyndicationResource"/> that is used when determining if conditional GET operation requirements were met.</param>
        /// <remarks>
        ///     <para>
        ///         This  method will retrieve conditional GET information from the request headers, and if present, will set the appropriate 
        ///         response header information and status for the supplied <paramref name="resource"/>. If a modification date is available for  
        ///         the supplied <paramref name="resource"/>, the conditional GET header information will be set so that clients may optionally 
        ///         cache this infomration when making future requests.
        ///     </para>
        ///     <para>If a conditional GET operation was fulflled, no additional processing or modification of the response should be performed.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="context"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        private static void ProcessConditionalGetInformation(HttpContext context, ISyndicationResource resource)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string eTag                 = String.Empty;
            string ifModifiedSince      = String.Empty;
            bool resourceIsUnmodified   = false;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(context, "context");
            Guard.ArgumentNotNull(resource, "resource");

            //------------------------------------------------------------
            //	Perform conditional GET processing
            //------------------------------------------------------------
            eTag            = context.Request.Headers["If-None-Match"];
            ifModifiedSince = context.Request.Headers["if-modified-since"];

            if (!String.IsNullOrEmpty(eTag) || !String.IsNullOrEmpty(ifModifiedSince))
            {
                DateTime lastModified   = SyndicationResourceHandler.GetLastModificationDate(resource);

                if (lastModified == DateTime.MinValue)
                {
                    return;
                }

                if (!String.IsNullOrEmpty(eTag))
                {
                    //------------------------------------------------------------
                    //	Determine if not modified by comparing last modified date-time
                    //------------------------------------------------------------
                    resourceIsUnmodified    = eTag.Equals(lastModified.Ticks.ToString(CultureInfo.InvariantCulture));
                }
                else if (!String.IsNullOrEmpty(ifModifiedSince))
                {
                    //------------------------------------------------------------
                    //	Determine if not modified by comparing if modified since date-time
                    //------------------------------------------------------------
                    if (ifModifiedSince.IndexOf(";", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        ifModifiedSince = ifModifiedSince.Split(';').GetValue(0).ToString().Trim();    // Wed, 29 Dec 2004 18:34:27 GMT; length=126275
                    }

                    DateTime ifModifiedDate;
                    if (DateTime.TryParse(ifModifiedSince, out ifModifiedDate))
                    {
                        resourceIsUnmodified    = (lastModified <= ifModifiedDate);
                    }
                }

                //------------------------------------------------------------
                //	Set conditional GET response information
                //------------------------------------------------------------
                if (resourceIsUnmodified)
                {
                    context.Response.StatusCode         = 304;
                    context.Response.SuppressContent    = true;
                    context.Response.End();     // HTTP handler execution stops.
                }
                else
                {
                    if (lastModified <= DateTime.Now)
                    {
                        context.Response.Cache.SetLastModified(lastModified);
                    }
                    else
                    {
                        context.Response.Cache.SetLastModified(context.Timestamp);
                    }
                    context.Response.Cache.SetETag(lastModified.Ticks.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region ProcessRequest(HttpContext context)
        /// <summary>
        /// Enables processing of HTTP Web requests for syndicated content.
        /// </summary>
        /// <param name="context">
        ///     An <see cref="HttpContext"/> object that provides references to the intrinsic server objects 
        ///     (for example, <b>Request</b>, <b>Response</b>, <b>Session</b>, and <b>Server</b>) used to service HTTP requests.
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="context"/> is a null reference (Nothing in Visual Basic).</exception>
        public void ProcessRequest(HttpContext context)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            ISyndicationResource syndicationResource    = null;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(context, "context");

            //------------------------------------------------------------
            //	Initialize handler state using query string parameters
            //------------------------------------------------------------
            if (context.Request.QueryString["format"] != null && !String.IsNullOrEmpty(context.Request.QueryString["format"]))
            {
                SyndicationContentFormat format = SyndicationDiscoveryUtility.SyndicationContentFormatByName(context.Request.QueryString["format"]);
                if (format != SyndicationContentFormat.None)
                {
                    this.Format = format;
                }
            }
            if (context.Request.QueryString["id"] != null && !String.IsNullOrEmpty(context.Request.QueryString["id"]))
            {
                this.Id = context.Request.QueryString["id"];
            }

            //------------------------------------------------------------
            //	Get syndication resource using provider model
            //------------------------------------------------------------
            if(this.Id != null)
            {
                syndicationResource    = SyndicationManager.GetResource(this.Id);
            }
            else if(this.Format != SyndicationContentFormat.None)
            {
                Collection<ISyndicationResource> resources  = SyndicationManager.GetResources(this.Format);
                if (resources != null && resources.Count > 0)
                {
                    syndicationResource    = resources[0];
                }
            }

            //------------------------------------------------------------
            //	Write syndication resource data and header details
            //------------------------------------------------------------
            if (syndicationResource != null)
            {
                this.WriteResource(context, syndicationResource);
            }
        }
        #endregion

        //============================================================
        //	PRIVATE METHODS
        //============================================================
        #region Initialize()
        /// <summary>
        /// Initializes the current instance using the application configuration settings.
        /// </summary>
        /// <seealso cref="SyndicationResourceHandlerSection"/>
        private void Initialize()
        {
            SyndicationResourceHandlerSection handlerConfiguration  = PrivilegedConfigurationManager.GetSyndicationHandlerSection();

            if (handlerConfiguration != null)
            {
                if (handlerConfiguration.UpdatableWithin != TimeSpan.MinValue)
                {
                    this.ContentUpdatableWithin = handlerConfiguration.UpdatableWithin;
                }

                if (handlerConfiguration.ValidFor != TimeSpan.MinValue)
                {
                    this.ContentValidFor        = handlerConfiguration.ValidFor;
                }

                this.EnableCaching              = handlerConfiguration.EnableCaching;

                if (handlerConfiguration.Format != SyndicationContentFormat.None)
                {
                    this.Format                 = handlerConfiguration.Format;
                }

                if (!String.IsNullOrEmpty(handlerConfiguration.Id))
                {
                    this.Id                     = handlerConfiguration.Id;
                }
            }
        }
        #endregion

        #region ModifyCacheExpiration(HttpContext context)
        /// <summary>
        /// Modifies the <see cref="HttpResponse"/> caching properties on the supplied <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="context">
        ///     An <see cref="HttpContext"/> object that provides references to the intrinsic server objects 
        ///     (for example, <b>Request</b>, <b>Response</b>, <b>Session</b>, and <b>Server</b>) used to service HTTP requests.
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="context"/> is a null reference (Nothing in Visual Basic).</exception>
        private void ModifyCacheExpiration(HttpContext context)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(context, "context");

            //------------------------------------------------------------
            //	Set Cache-Control header
            //------------------------------------------------------------
            context.Response.Cache.SetCacheability(HttpCacheability.Public);

            //------------------------------------------------------------
            //	Modify caching options based on handler settings
            //------------------------------------------------------------
            if (this.EnableCaching)
            {
                context.Response.Cache.VaryByParams["*"]    = true;

                if (this.ContentValidFor.TotalSeconds > 0)
                {
                    //------------------------------------------------------------
                    //	Set cache expiration period
                    //------------------------------------------------------------
                    context.Response.Cache.SetExpires(DateTime.Now.Add(this.ContentValidFor));
                    context.Response.Cache.SetMaxAge(this.ContentValidFor);
                    context.Response.Cache.SetValidUntilExpires(true);

                    //------------------------------------------------------------
                    //	Append pre/post-check cache extension
                    //------------------------------------------------------------
                    if (this.ContentUpdatableWithin.TotalSeconds > 0)
                    {
                        context.Response.Cache.AppendCacheExtension(String.Format(null, "post-check={0},pre-check={1}", this.ContentUpdatableWithin.TotalSeconds, this.ContentValidFor.TotalSeconds));
                    }
                }
            }
        }
        #endregion

        #region WriteResource(HttpContext context, ISyndicationResource resource)
        /// <summary>
        /// Writes the supplied <see cref="ISyndicationResource"/> to the specified <see cref="HttpContext"/> response stream and sets the appropriate headers.
        /// </summary>
        /// <param name="context">
        ///     An <see cref="HttpContext"/> object that provides references to the intrinsic server objects 
        ///     (for example, <b>Request</b>, <b>Response</b>, <b>Session</b>, and <b>Server</b>) used to service HTTP requests.
        /// </param>
        /// <param name="resource">The <see cref="ISyndicationResource"/> to be written to the response stream.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="context"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="resource"/> is a null reference (Nothing in Visual Basic).</exception>
        private void WriteResource(HttpContext context, ISyndicationResource resource)
        {
            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(context, "context");
            Guard.ArgumentNotNull(resource, "resource");

            //------------------------------------------------------------
            //	Process conditional GET information if present
            //  (If conditions met, handler execution stops.)
            //------------------------------------------------------------
            SyndicationResourceHandler.ProcessConditionalGetInformation(context, resource);

            //------------------------------------------------------------
            //	Modify response meta-data information
            //------------------------------------------------------------
            context.Response.ContentType    = SyndicationResourceHandler.ContentFormatAsMimeType(resource.Format);
            context.Response.AppendHeader("Content-Disposition", SyndicationResourceHandler.GetContentDisposition(resource.Format));

            //------------------------------------------------------------
            //	Save XML representation of resource to output stream
            //------------------------------------------------------------
            SyndicationResourceSaveSettings settings    = new SyndicationResourceSaveSettings();
            settings.AutoDetectExtensions               = true;
            settings.MinimizeOutputSize                 = false;

            resource.Save(context.Response.OutputStream, settings);

            //------------------------------------------------------------
            //	Modify response caching expiration information
            //------------------------------------------------------------
            this.ModifyCacheExpiration(context);
        }
        #endregion
    }
}
