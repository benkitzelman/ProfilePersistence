using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Web.Profile;

/// <summary>
/// The extension of the Profile allowing Controls to be registered for
/// automatic population and storing of user data.
/// </summary>
/// <remarks>
/// Author : Ben Kitzelman
/// </remarks>
public class Profile : System.Web.Profile.ProfileBase
{
	#region Fields

	private Page m_registeredPage = null;

	#endregion

	#region Properties

	public UrlProfile CurrentUrlProfile
	{
		get
		{
			return UrlProfiles[HttpContext.Current.Request.Url.PathAndQuery] as UrlProfile;
		}
	}

	private System.Collections.Hashtable UrlProfiles
	{
		get
		{
			if (base["UrlProfileCollection"] == null) base["UrlProfileCollection"] = new System.Collections.Hashtable();
			return base["UrlProfileCollection"] as System.Collections.Hashtable;
		}
		set
		{
			base["UrlProfileCollection"] = value;
		}
	}

	#endregion

	#region Constructors

	public Profile() : base()
	{
	}

	#endregion

	#region Methods

	public void Register(Control p_control, string p_persistableProperty)
	{
		//
		// do not register the control for persistence if it has not been added to the page.
		//
		if (p_control == null || p_control.Page == null) return;
				
		//
		// Create a new Url profile if this url has not yet been persisted to the profile 
		// and register this control for this URL.
		//
		if (this.UrlProfiles[p_control.Page.Request.Url.PathAndQuery] == null)
		{
			this.UrlProfiles[p_control.Page.Request.Url.PathAndQuery] = new UrlProfile();			
		}

		UrlProfile profile = this.UrlProfiles[p_control.Page.Request.Url.PathAndQuery] as UrlProfile;
		profile.RegisterControl(p_control, p_persistableProperty);

		AddPageEventListeners(p_control.Page);
		
		this.Save();
	}

	//
	// Adds handlers to a Page's PreLoad and PreRender events
	//
	private void AddPageEventListeners(Page p_page)
	{
		if (m_registeredPage != null) return;

		m_registeredPage            = p_page;
		m_registeredPage.PreLoad   += new EventHandler(RegisteredPagePreLoading);
		m_registeredPage.PreRender += new EventHandler(RegisteredPagePreRender);
	}

	//
	// Save all registered control data for the current URL to the profile
	//
	void RegisteredPagePreRender(object sender, EventArgs e)
	{
		UrlProfile profile = this.UrlProfiles[((Page)sender).Request.Url.PathAndQuery] as UrlProfile;
		profile.ExtractProfileFromPage((Page) sender);
		this.Save();
	}

	//
	// Load all registered control data for the current URL from the profile
	//
	void RegisteredPagePreLoading(object sender, EventArgs e)
	{
		if (((Page) sender).IsPostBack == true) return;
		string key         = ((Page) sender).Request.Url.PathAndQuery;
		UrlProfile urlProfile = this.UrlProfiles[key] as UrlProfile;
		if (urlProfile == null) return;

		urlProfile.LoadPageFromProfile((Page) sender);
	}

	#endregion

}
