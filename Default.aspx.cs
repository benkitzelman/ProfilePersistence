using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class _Default : System.Web.UI.Page 
{
  protected void Page_Load(object sender, EventArgs e)
  {
		if(User.Identity.IsAuthenticated == true) this.profileUser.Text = Profile.UserName;		
  }

	protected void Login1_Authenticate(object sender, AuthenticateEventArgs e)
	{
		e.Authenticated = true;		
	}
	protected void Login1_LoggedIn(object sender, EventArgs e)
	{
		Response.Redirect("~/ProfileTest.aspx");
	}
}
