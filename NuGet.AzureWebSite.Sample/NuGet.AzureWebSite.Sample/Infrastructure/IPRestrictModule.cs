
namespace NuGet.AzureWebSite.Sample.Infrastructure
{
	using System;
	using System.Web;
	using System.Collections.Specialized;
	using System.Configuration;

	/// <summary>
	/// See also: 
	/// http://www.hanselman.com/blog/AnIPAddressBlockingHttpModuleForASPNETIn9Minutes.aspx
	/// </summary>
	public class IPRestrictModule : IHttpModule
	{
		private EventHandler onBeginRequest;

		public IPRestrictModule()
		{
			onBeginRequest = new EventHandler(this.HandleBeginRequest);
		}

		void IHttpModule.Dispose()
		{
		}

		void IHttpModule.Init(HttpApplication context)
		{
			context.BeginRequest += onBeginRequest;
		}

		private const string ALLOWEDIPSKEY = "allowedips";

		public static StringDictionary GetAllowedIPs(HttpContext context)
		{
			var ips = (StringDictionary)context.Cache[ALLOWEDIPSKEY];
			if (ips == null)
			{
				ips = GetAllowedIPs();
				context.Cache.Insert(ALLOWEDIPSKEY, ips);
			}
			return ips;
		}

		public static StringDictionary GetAllowedIPs()
		{
			string addresses = ConfigurationManager.AppSettings["allowIPAddresses"];

			var retval = new StringDictionary();

			if (!string.IsNullOrEmpty(addresses))
			{
				foreach (var ip in addresses.Split(','))
				{
					retval.Add(ip, null);
				}
			}
			return retval;
		}

		private void HandleBeginRequest(object sender, EventArgs evargs)
		{
			var app = sender as HttpApplication;

			if (app != null)
			{
				string IPAddr = app.Context.Request.ServerVariables["REMOTE_ADDR"];
				if (IPAddr == null || IPAddr.Length == 0)
				{
					return;
				}

				StringDictionary okIPs = GetAllowedIPs(app.Context);
				if (okIPs == null || !okIPs.ContainsKey(IPAddr))
				{
					app.Context.Response.StatusCode = 403;
					app.Context.Response.SuppressContent = true;
					app.Context.Response.End();
					return;
				}
			}
		}
	}
}
	