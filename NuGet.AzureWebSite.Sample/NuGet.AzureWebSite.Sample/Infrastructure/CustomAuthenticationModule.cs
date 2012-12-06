using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace NuGet.AzureWebSite.Sample.Infrastructure
{
	/// <summary>
	/// See also: http://blogs.msdn.com/b/tsmatsuz/archive/2011/09/12/restful-service-using-custom-basic-authentication-in-windows-azure.aspx
	/// </summary>
	public class CustomAuthenticationModule : IHttpModule
	{
		public void Init(HttpApplication context)
		{
			context.AuthenticateRequest += ContextOnAuthenticateRequest;
		}

		private void ContextOnAuthenticateRequest(object sender, EventArgs eventArgs)
		{
			var application = sender as HttpApplication;
			var context = application.Context;
			string userName = null;
			string password = null;
			// memo :
			// This function is called for each request.
			// So, it's better to use cache, etc ...
			// (Here, I skipped this functionality.)

			// 1. Get username and password from authorizationHeader
			// 2. Validate username and password
			var authorizationHeader = context.Request.Headers["Authorization"];
			if (!ExtractBasicCredentials(authorizationHeader, ref userName, ref password) 
				|| !ValidateCredentials(userName, password))
			{
				context.Response.StatusCode = 401;
				context.Response.AddHeader("WWW-Authenticate", "Basic realm =\"NuGet.AzureWebSite\"");
			}
			else
			{
				context.User = new GenericPrincipal(new GenericIdentity(userName), null);
			}
		}

		protected bool ExtractBasicCredentials(string authorizationHeader, ref string username, ref string password)
		{
			const string HttpBasicSchemeName = "Basic";

			if ((authorizationHeader == null) || (authorizationHeader.Equals(string.Empty)))
				return false;

			var verifiedAuthorizationHeader = authorizationHeader.Trim();

			if (verifiedAuthorizationHeader.IndexOf(HttpBasicSchemeName) != 0)
				return false;

			// Get sub string (eliminated the first "Basic" string)
			// from verifiedAuthorizationHeader
			verifiedAuthorizationHeader =
			  verifiedAuthorizationHeader.Substring(
				HttpBasicSchemeName.Length,
				verifiedAuthorizationHeader.Length - HttpBasicSchemeName.Length).Trim();

			// Decode the base64 encoded string
			byte[] credentialBase64DecodedArray = Convert.FromBase64String(verifiedAuthorizationHeader);
			var encoding = new UTF8Encoding();
			var decodedAuthorizationHeader = encoding.GetString(credentialBase64DecodedArray, 0, credentialBase64DecodedArray.Length);

			// Get username and password string
			int separatorPosition = decodedAuthorizationHeader.IndexOf(':');
			if (separatorPosition <= 0)
				return false;
			username = decodedAuthorizationHeader.Substring(0, separatorPosition).Trim();
			password = decodedAuthorizationHeader.Substring(separatorPosition + 1, (decodedAuthorizationHeader.Length - separatorPosition - 1)).Trim();

			if (username.Equals(string.Empty) || password.Equals(string.Empty))
				return false;

			return true;
		}

		protected bool ValidateCredentials(string userName, string password)
		{
			//ACS使おうかと思ったけど面倒くさい
			var allowuser = ConfigurationManager.AppSettings["userId"];
			var allowpass = ConfigurationManager.AppSettings["userPassword"];

			if (System.String.Compare(allowuser, userName, System.StringComparison.OrdinalIgnoreCase) != 0) return false;
			if (System.String.CompareOrdinal(allowpass, password) != 0) return false;

			return true;
		}
		
		public void Dispose()
		{
		}
	}
}