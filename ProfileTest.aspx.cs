using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class ProfileTest : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
		username.Text = HttpContext.Current.User.Identity.Name;
	}	

	protected override void OnInit(EventArgs e)
	{		
		base.OnInit(e);
		Profile.Register(testBox, "Text");
	}

	protected void navLink_Click(object sender, EventArgs e)
	{
		//Response.Redirect("~/CurrentProfiles.aspx");
	}
}
