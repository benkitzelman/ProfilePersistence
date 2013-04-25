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
using System.Web.Profile;

public partial class CurrentProfiles : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
		DateTime test = Profile.LastActivityDate;
		this.currentProfileLabel.Text = Profile.UserName;
  }

	protected void logoutButton_Click(object sender, EventArgs e)
	{
		FormsAuthentication.SignOut();
		FormsAuthentication.RedirectToLoginPage();
	}

	protected void removeProfileButton_Click(object sender, EventArgs e)
	{
		try
		{
			int minute                = int.Parse(this.minuteBox.Text);
			ProfileInfoCollection pic = ProfileManager.GetAllInactiveProfiles(ProfileAuthenticationOption.All, DateTime.Now.Subtract(TimeSpan.FromMinutes(minute)));
			string usernames          = "";
			foreach (ProfileInfo pi in pic)
			{
				usernames += pi.UserName + " ";
			}
			ProfileManager.DeleteProfiles(pic);
			this.statusLabel.Text = "Removed the following profiles : " + usernames;
		}
		catch (Exception ex)
		{
			this.statusLabel.Text = "Error : " + ex.Message + " :- " + ex.ToString();
		}

		this.GridView1.DataBind();
	}
}
