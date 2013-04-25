using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections;

[Serializable]
public class UrlProfile
{
	#region Fields

	private Hashtable m_controlProfiles = new Hashtable();

	#endregion

	#region Properties

	public Hashtable ControlProfiles
	{
		get
		{
			return this.m_controlProfiles;
		}
	}

	#endregion

	#region Constructors

	public UrlProfile()
	{
	}

	#endregion

	#region Methods

	internal void RegisterControl(Control p_control, string p_persistableProperty)
	{
		ControlProfile cp;
		if (m_controlProfiles[p_control.UniqueID] == null)
		{
			cp = new ControlProfile();
			m_controlProfiles.Add(p_control.UniqueID, cp);
		}
		else
		{
			cp = m_controlProfiles[p_control.UniqueID] as ControlProfile;
		}

		cp.PersistableProperty = p_persistableProperty;			
	}

	internal void LoadPageFromProfile(Page p_page)
	{
		foreach (string controlID in this.ControlProfiles.Keys)
		{
			ControlProfile controlProfile = this.ControlProfiles[controlID] as ControlProfile;
			controlProfile.LoadControlFromProfile(p_page.FindControl(controlID));
		}
	}
	internal void ExtractProfileFromPage(Page p_page)
	{
		foreach (string controlID in this.ControlProfiles.Keys)
		{
			ControlProfile controlProfile = this.ControlProfiles[controlID] as ControlProfile;
			controlProfile.ExtractProfileFromControl(p_page.FindControl(controlID));
		}
	}

	#endregion
}

[Serializable]
public class ControlProfile
{
	#region Fields

	private string m_persistableProperty;
	private object m_propertyValue;

	#endregion

	#region Constructors

	public ControlProfile()
	{
	}
	
	public ControlProfile(string p_targetProperty)
	{
		this.PersistableProperty = p_targetProperty;
	}

	#endregion

	#region Properties

	public string PersistableProperty
	{
		get { return this.m_persistableProperty; }
		set { this.m_persistableProperty = value; }
	}
	public object PropertyValue
	{
		get { return this.m_propertyValue; }
		set { this.m_propertyValue = value; }
	}

	#endregion

	#region Methods

	internal void ExtractProfileFromControl(Control p_control)
	{
		if (p_control.GetType().GetMember(PersistableProperty)[0].MemberType != System.Reflection.MemberTypes.Property) return;
		
		object value = p_control.GetType().GetProperty(this.PersistableProperty).GetValue(p_control, null);
		this.PropertyValue = value;		
	}

	internal void LoadControlFromProfile(Control p_control)
	{
		if (p_control == null || this.PropertyValue == null) return;
	
		if(p_control.GetType().GetMember(PersistableProperty).Length == 0) return;

		if (p_control.GetType().GetMember(PersistableProperty)[0].MemberType == System.Reflection.MemberTypes.Property)
		{
			p_control.GetType().GetProperty(this.PersistableProperty).SetValue(p_control, this.PropertyValue, null);
		}
	}

	#endregion

}
