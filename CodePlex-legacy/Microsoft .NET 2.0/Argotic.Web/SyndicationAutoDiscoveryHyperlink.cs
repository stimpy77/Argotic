/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
04/13/2008	brian.kuhn	Created SyndicationAutoDiscoveryHyperlink Class
****************************************************************************/
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using Argotic.Common;

[assembly: TagPrefix("Argotic.Web", "argotic")]

namespace Argotic.Web
{
    /// <summary>
    /// A control that enables the auto-discovery of syndicated content by generating a header link that describes the syndication resource.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Auto-discovery is a technique that makes it possible for browsers and other software to automatically find a web site's syndication feeds and other resources. 
    ///         Supported by Mozilla Firefox 2.0, Microsoft Internet Explorer 7.0 and other browsers, autodiscovery has become the best way to inform users that a web site 
    ///         offers a syndication feed. When a browser loads a page and discovers that a feed is available, Firefox and Internet Explorer display the common feed icon in the address bar.
    ///     </para>
    ///     <para>
    ///         This implementation conforms to the <i>RSS Autodiscovery</i> 1.0 guidelines as close as possible, 
    ///         which can be found at <a href="http://www.rssboard.org/rss-autodiscovery">http://www.rssboard.org/rss-autodiscovery</a>.
    ///     </para>
    ///     <para>
    ///         Regardless of whether the <see cref="SyndicationAutoDiscoveryHyperlink"/> control is placed within the <i>head</i> section 
    ///         of an ASP.NET page or somewhere within the <i>body</i> of an ASP.NET page, the auto-discovery link will be written within the 
    ///         <i>head</i> section of the page, as client browsers that support syndication auto-discovery expect it to be located in this HTML section.
    ///     </para>
    /// </remarks>
    [System.Web.AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = System.Web.AspNetHostingPermissionLevel.Minimal)]
    public class SyndicationAutoDiscoveryHyperlink : HyperLink
    {
        //============================================================
		//	CONSTRUCTORS
        //============================================================
        #region SyndicationAutoDiscoveryHyperlink()
        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationAutoDiscoveryHyperlink"/> class.
        /// </summary>
        public SyndicationAutoDiscoveryHyperlink() : base()
		{
			//------------------------------------------------------------
			//	Initialization handled by base class
			//------------------------------------------------------------
		}
		#endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Format
        /// <summary>
        /// Gets or sets the <see cref="SyndicationContentFormat"/> that the auto-discoverable syndicated content conforms to.
        /// </summary>
        /// <value>The <see cref="SyndicationContentFormat"/> enumeration value that indicates the type of syndication format that auto-discoverable syndicated content conforms to.</value>
        [TypeConverter(typeof(SyndicationContentFormat))]
        [Bindable(true), Browsable(true), Category("Data"), Description("Gets or sets the syndication format that the auto-discoverable syndicated content conforms to.")]
        public SyndicationContentFormat Format
        {
            get
            {
                if (base.ViewState["SyndicationAutoDiscoveryHyperlink_Format"] == null)
                {
                    return SyndicationContentFormat.None;
                }
                else
                {
                    SyndicationContentFormat format = (SyndicationContentFormat)base.ViewState["SyndicationAutoDiscoveryHyperlink_Format"];
                    return format;
                }
            }

            set
            {
                base.ViewState["SyndicationAutoDiscoveryHyperlink_Format"] = value;
            }
        }
        #endregion

        #region MediaType
        /// <summary>
        /// Gets or sets the MIME content type of the auto-discoverable syndicated content.
        /// </summary>
        /// <value>
        ///     The <a href="http://www.iana.org/assignments/media-types/">IANA MIME media type</a> for the auto-discoverable syndicated content. 
        ///     The default value is an empty string.
        /// </value>
        /// <remarks>
        ///     If a MIME media type is explictily specified, its value takes precedence over 
        ///     the MIME type that would be generated based on the <see cref="Format">format</see>.
        /// </remarks>
        [Bindable(true), Browsable(true), Category("Data"), Description("Gets or sets the MIME content type of the syndicated content you wish to broadcast as discoverable.")]
        public string MediaType
        {
            get
            {
                if (base.ViewState["SyndicationAutoDiscoveryHyperlink_MediaType"] == null)
                {
                    return String.Empty;
                }
                else
                {
                    return base.ViewState["SyndicationAutoDiscoveryHyperlink_MediaType"].ToString();
                }
            }

            set
            {
                if(String.IsNullOrEmpty(value))
                {
                    base.ViewState["SyndicationAutoDiscoveryHyperlink_MediaType"] = String.Empty;
                }
                else
                {
                    base.ViewState["SyndicationAutoDiscoveryHyperlink_MediaType"] = value.Trim();
                }
            }
        }
        #endregion

        #region NavigateUrl
        /// <summary>
        /// Gets or sets the <see cref="Uri">URL</see> to the auto-discoverable syndicated content.
        /// </summary>
        /// <value>The <see cref="Uri">URL</see> to the auto-discoverable syndicated content. The default value is a null reference (Nothing in Visual Basic).</value>
        /// <remarks>
        ///     Use the NavigateUrl property to specify the URL of the syndicated content you wish to broadcast as discoverable.
        /// </remarks>
        [TypeConverter(typeof(Uri))]
        [Bindable(true), Browsable(true), Category("Data"), Description("Gets or sets the URL of the syndicated content you wish to broadcast as discoverable.")]
        public new Uri NavigateUrl
        {
            get
            {
                if (String.IsNullOrEmpty(base.NavigateUrl))
                {
                    return null;
                }
                else
                {
                    Uri navigationUri = null;
                    if (Uri.TryCreate(base.NavigateUrl, UriKind.RelativeOrAbsolute, out navigationUri))
                    {
                        return navigationUri;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            set
            {
                if (value == null)
                {
                    base.NavigateUrl = String.Empty;
                }
                else
                {
                    base.NavigateUrl = value.ToString();
                }
            }
        }
        #endregion

        #region Text
        /// <summary>
        /// Gets or sets the display name for the auto-discoverable syndicated content.
        /// </summary>
        /// <value>The display name for the auto-discoverable syndicated content. The default value is an empty string.</value>
        [Bindable(true), Browsable(true), Localizable(true), Category("Appearance"), Description("Gets or sets the title of the syndicated content you wish to broadcast as discoverable.")]
        public new string Text
        {
            get
            {
                return base.Text;
            }

            set
            {
                if(String.IsNullOrEmpty(value))
                {
                    base.Text = String.Empty;
                }
                else
                {
                    base.Text = value.Trim();
                }
            }
        }
        #endregion

        //============================================================
        //	PROTECTED METHODS
        //============================================================
        #region OnPreRender(EventArgs e)
        /// <summary>
        /// Raises the <see cref="Control.PreRender">PreRender</see> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        /// <remarks>
        ///     <para>This method notifies the server control to perform any necessary pre-rendering steps prior to saving view state and rendering content.</para>
        ///     <para>
        ///         During this step in the ASP.NET page life-cycle, the <see cref="SyndicationAutoDiscoveryHyperlink"/> will attempt to add 
        ///         a <i><link rel="alternate" type="{MIME Type}" title="{Text}" href="{NavigateUrl}" /></i> to the page header section if 
        ///         it is available to be modified.
        ///     </para>
        ///     <para>
        ///         The MIME media type of the link will be automatically selected based on the <see cref="Format">format</see> 
        ///         unless the <see cref="MediaType">media type</see> has been explicitly specified.
        ///     </para>
        ///     <para>
        ///         The <i>title</i> attribute will only be rendered if the control's <see cref="Text"/> property has been specified.
        ///     </para>
        /// </remarks>
        protected override void OnPreRender(EventArgs e)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string mimeType = String.Empty;
            string href     = String.Empty;
            string title    = String.Empty;

            //------------------------------------------------------------
            //	Do not render control is invisible or disabled
            //------------------------------------------------------------
            if (!this.Visible || !this.Enabled)
            {
                base.OnPreRender(e);
                return;
            }

            //------------------------------------------------------------
            //	As the page header control collection cannot be 
            //  modified during the PreRender phase, exit this event 
            //  handler to prevent raising an exception.
            //------------------------------------------------------------
            if(this.Parent is System.Web.UI.HtmlControls.HtmlHead)
            {
                base.OnPreRender(e);
                return;
            }
            else if (this.Parent is System.Web.UI.WebControls.ContentPlaceHolder && this.Parent.Parent is System.Web.UI.HtmlControls.HtmlHead)
            {
                base.OnPreRender(e);
                return;
            }

            //------------------------------------------------------------
            //	Render auto-discovery link
            //------------------------------------------------------------
            href        = this.NavigateUrl != null ? this.NavigateUrl.ToString() : String.Empty;
            mimeType    = SyndicationResourceHandler.ContentFormatAsMimeType(this.Format);
            if (!String.IsNullOrEmpty(this.Text))
            {
                title   = this.Text;
            }

            if(this.Page.Header != null)
            {
                string controlContent   = String.Format(null, "\r\n<link rel=\"alternate\" type=\"{0}\" href=\"{1}\" />", mimeType, href);
                if (!String.IsNullOrEmpty(title))
                {
                    controlContent      = String.Format(null, "\r\n<link rel=\"alternate\" type=\"{0}\" href=\"{1}\" title=\"{2}\" />", mimeType, href, title);
                }
                LiteralControl link     = new LiteralControl(controlContent);

                this.Page.Header.Controls.Add(link);
            }

            base.OnPreRender(e);
        }
        #endregion

        #region RenderControl(HtmlTextWriter writer)
        /// <summary>
        /// Outputs server control content to a provided <see cref="HtmlTextWriter"/> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="HtmlTextWriter"/> object that receives the control content.</param>
        /// <remarks>
        ///     <para>
        ///         The MIME media type of the link will be automatically selected based on the <see cref="Format">format</see> 
        ///         unless the <see cref="MediaType">media type</see> has been explicitly specified.
        ///     </para>
        ///     <para>
        ///         The <i>title</i> attribute will only be rendered if the control's <see cref="Text"/> property has been specified.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference (Nothing in Visual Basic).</exception>
        public override void RenderControl(HtmlTextWriter writer)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            string mimeType = String.Empty;
            string href     = String.Empty;
            string title    = String.Empty;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            //------------------------------------------------------------
            //	Do not render control is invisible or disabled
            //------------------------------------------------------------
            if (!this.Enabled || !this.Visible)
            {
                return;
            }

            //------------------------------------------------------------
            //	Determine if link is positioned in HTML head
            //------------------------------------------------------------
            if (this.Parent is System.Web.UI.HtmlControls.HtmlHead)
            {
                //------------------------------------------------------------
                //	Render auto-discovery link
                //------------------------------------------------------------
                href        = this.NavigateUrl != null ? this.NavigateUrl.ToString() : String.Empty;
                mimeType    = SyndicationResourceHandler.ContentFormatAsMimeType(this.Format);
                if (!String.IsNullOrEmpty(this.Text))
                {
                    title   = this.Text;
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Rel, "alternate");
                writer.AddAttribute(HtmlTextWriterAttribute.Type, mimeType);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
                if (!String.IsNullOrEmpty(title))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, title);
                }

                writer.RenderBeginTag(HtmlTextWriterTag.Link);
                writer.RenderEndTag();
                writer.WriteLine();
            }
        }
        #endregion
    }
}
