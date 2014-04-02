/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/25/2008	brian.kuhn	Created SyndicationResourceHandlerFactory Class
****************************************************************************/
using System;
using System.Collections;
using System.Web;

using Argotic.Common;

namespace Argotic.Web
{
    /// <summary>
    /// HTTP handler factory used to create new <see cref="SyndicationResourceHandler"/> objects.
    /// </summary>
    /// <seealso cref="SyndicationResourceHandler"/>
    /// <remarks>
    ///     <para>
    ///         By default the syndication handler factory will return a default instance of the <see cref="SyndicationResourceHandler"/> class. 
    ///         However, if the configured path of the handler factory uses a supported syndication format as its name (e.g. apml.axd, atom.axd, blogml.axd, opml.axd, rsd.axd, rss.axd, etc.), 
    ///         the factory will automatically configure the handler to use the syndication format that matches the factory path name.
    ///     </para>
    /// </remarks>
    public class SyndicationResourceHandlerFactory : IHttpHandlerFactory
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold a value used to enable initialization mutual-exclusion locking.
        /// </summary>
        private static object factoryHandlerLock    = new Object();
        /// <summary>
        /// Private member to hold the maximum number of handlers that can be used in pooling by the factory.
        /// </summary>
        private static int handlerPoolLimit         = 10;
        /// <summary>
        /// Private member to hold a collection of handlers managed by the factory.
        /// </summary>
        private static Stack factoryHandlers        = new Stack(handlerPoolLimit);
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region SyndicationResourceHandlerFactory()
        /// <summary>
        /// Initializes a new instance of the <see cref="SyndicationResourceHandlerFactory"/> class.
        /// </summary>
        private SyndicationResourceHandlerFactory()
        {
            //------------------------------------------------------------
            //	
            //------------------------------------------------------------
        }
        #endregion

        #region SyndicationResourceHandlerFactory(int poolSize)
        /// <summary>
        /// Creates a new instance of the <see cref="SyndicationResourceHandlerFactory"/> class using the supplied pool size.
        /// </summary>
        /// <param name="poolSize">
        ///     The maximum number of resuable handlers that can be pooled by the factory. 
        ///     A value of 0 or less indicates an unlimted number of handlers will be pooled by the factory
        /// </param>
        public SyndicationResourceHandlerFactory(int poolSize) : this()
        {
            //------------------------------------------------------------
            //	Initialize pooling
            //------------------------------------------------------------
            if (poolSize <= 0)
            {
                factoryHandlers = new Stack();
            }
            else
            {
                factoryHandlers = new Stack(poolSize);
            }
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        /// <summary>
        /// Returns an instance of a class that implements the <see cref="IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">
        ///     An instance of the <see cref="HttpContext"/> class that provides references to 
        ///     intrinsic server objects (for example, <b>Request</b>, <b>Response</b>, <b>Session</b>, and <b>Server</b>) used to service HTTP requests.
        /// </param>
        /// <param name="requestType">The HTTP data transfer method (<b>GET</b> or <b>POST</b>) that the client uses.</param>
        /// <param name="url">The <see cref="HttpRequest.RawUrl">RawUrl</see> of the requested resource.</param>
        /// <param name="pathTranslated">The <see cref="HttpRequest.PhysicalApplicationPath">PhysicalApplicationPath</see> to the requested resource.</param>
        /// <returns>A new <see cref="SyndicationResourceHandler"/> object that processes the request.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="context"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="requestType"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="requestType"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="url"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="url"/> is an empty string.</exception>
        /// <exception cref="HttpException">The factory was unable to create a <see cref="SyndicationResourceHandler"/> for the request.</exception>
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            SyndicationResourceHandler handler  = null;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(context, "context");
            Guard.ArgumentNotNullOrEmptyString(requestType, "requestType");
            Guard.ArgumentNotNullOrEmptyString(url, "url");

            //------------------------------------------------------------
            //	Return reusable handler if available
            //------------------------------------------------------------
            lock (factoryHandlerLock)
            {
                if (factoryHandlers.Count > 0)
                {
                    handler = (SyndicationResourceHandler)factoryHandlers.Pop();
                    return handler;
                }
            }

            //------------------------------------------------------------
            //	Attempt to create handler
            //------------------------------------------------------------
            try
            {
                handler = (SyndicationResourceHandler)Activator.CreateInstance(typeof(SyndicationResourceHandler));

                //------------------------------------------------------------
                //	Get factory name
                //------------------------------------------------------------
                string fullName     = url.Substring(url.LastIndexOf('/') + 1);
                string factoryName  = fullName.Substring(0, fullName.IndexOf('.'));

                //------------------------------------------------------------
                //	Set syndication format of handler based on factory path
                //------------------------------------------------------------
                if (String.Compare(factoryName, "Apml", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    handler.Format  = SyndicationContentFormat.Apml;
                }
                else if (String.Compare(factoryName, "Atom", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    handler.Format  = SyndicationContentFormat.Atom;
                }
                else if (String.Compare(factoryName, "BlogML", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    handler.Format  = SyndicationContentFormat.BlogML;
                }
                else if (String.Compare(factoryName, "Opml", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    handler.Format  = SyndicationContentFormat.Opml;
                }
                else if (String.Compare(factoryName, "Rsd", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    handler.Format  = SyndicationContentFormat.Rsd;
                }
                else if (String.Compare(factoryName, "Rss", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    handler.Format  = SyndicationContentFormat.Rss;
                }

                return handler;
            }
            catch (Exception exception)
            {
                //------------------------------------------------------------
                //	Rethrow exception as HttpException with param information
                //------------------------------------------------------------
                throw new HttpException(String.Format(null, "The SyndicationHttpHandlerFactory was unable to create a handler for requestType='{0}', url='{1}', pathTranslated='{2}'.", requestType, url, pathTranslated), exception);
            }
        }
        #endregion

        #region ReleaseHandler(IHttpHandler handler)
        /// <summary>
        /// Enables a factory to reuse an existing HTTP handler instance.
        /// </summary>
        /// <param name="handler">The <see cref="IHttpHandler"/> object to reuse.</param>
        public void ReleaseHandler(IHttpHandler handler)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            SyndicationResourceHandler syndicationHandler   = null;

            //------------------------------------------------------------
            //	Verify handler is reusable
            //------------------------------------------------------------
            if (handler.IsReusable)
            {
                lock (factoryHandlerLock)
                {
                    if (factoryHandlers.Count < handlerPoolLimit)
                    {
                        syndicationHandler  = handler as SyndicationResourceHandler;

                        if (syndicationHandler != null)
                        {
                            factoryHandlers.Push(syndicationHandler);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
