using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using dk.nita.saml20.protocol.pages;
using Trace=dk.nita.saml20.Utils.Trace;

namespace dk.nita.saml20.protocol
{
    /// <summary>
    /// A base class for all endpoint handlers.
    /// </summary>
    public abstract class AbstractEndpointHandler : IHttpHandler, IRequiresSessionState
    {
        #region IHttpHandler Members

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public abstract void ProcessRequest(HttpContext context);

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get { return true; }
        }

        #endregion

        /// <summary>
        /// Displays an error page.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="overrideConfigSetting">if set to <c>true</c> [override config setting].</param>
        public void HandleError(HttpContext context, string errorMessage, bool overrideConfigSetting)
        {
            if (!string.IsNullOrEmpty(ErrorBehaviour) && ErrorBehaviour.Equals(dk.nita.saml20.config.ErrorBehaviour.THROWEXCEPTION.ToString()))
            {
                throw new Saml20Exception(errorMessage);
            }
            else
            {
                ErrorPage page = new ErrorPage();
                page.OverrideConfig = overrideConfigSetting;
                page.ErrorText = errorMessage;
                page.ProcessRequest(context);
                context.Response.End();
            }
        }

        /// <summary>
        /// Displays an error page.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="errorMessage">The error message.</param>
        public void HandleError(HttpContext context, string errorMessage)
        {
            HandleError(context, errorMessage, false);
        }

        /// <summary>
        /// Displays an error page.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="e">The exception that caused the error.</param>
        public void HandleError(HttpContext context, Exception e)
        {
            // ThreadAbortException is just part of ASP.NET's slightly broken conditional logic, so don't react to it.
            if (e is ThreadAbortException)
                return;

            StringBuilder sb = new StringBuilder(1000);            
            while (e != null)
            {
                sb.AppendLine(e.ToString());                
                e = e.InnerException;                
            }

            HandleError(context, sb.ToString());
        }

        private string _errorBehaviour;

        /// <summary>
        /// Gets or sets the error handling behaviour.
        /// </summary>
        /// <value>The error handling behaviour.</value>
        public string ErrorBehaviour
        {
            get { return _errorBehaviour; }
            set { _errorBehaviour = value; }
        }

        private string _redirectUrl;

        /// <summary>
        /// Gets or sets the redirect URL.
        /// </summary>
        /// <value>The redirect URL.</value>
        public string RedirectUrl
        {
            get { return _redirectUrl; }
            set{ _redirectUrl = value;}
        }

        /// <summary>
        /// Redirects the user.
        /// </summary>
        /// <param name="context">The context.</param>
        public void DoRedirect(HttpContext context)
        {
            string redirectUrl = (string) context.Session["RedirectUrl"];
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                context.Session.Remove("RedirectUrl");
                context.Response.Redirect(redirectUrl);
            } else if(string.IsNullOrEmpty(RedirectUrl))
            {
                context.Response.Redirect("~/");
            }else
            {
                context.Response.Redirect(RedirectUrl);
            }
        }
    }
}