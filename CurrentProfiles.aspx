<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CurrentProfiles.aspx.cs" Inherits="CurrentProfiles" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <style>
    body
    {
			font-family : arial;
			font-size:12px;
    }
		Label
		{
			display:block;
			width:200px;
			font-weight:bold;
		}
		a
		{
			color:Black;
		}
		a:hover
		{
			color:red;
		}
    </style>
</head>
<body>
    <form id="form1" runat="server">
    
    
			<div>
				Current Profile : <asp:Label ID="currentProfileLabel" runat="server"></asp:Label><br />
				<br />
				Current Persisted Profiles&nbsp;<br />
				
				<asp:GridView ID="GridView1" runat="server" BackColor="White" BorderColor="#CCCCCC"
					BorderStyle="None" BorderWidth="1px" CellPadding="3" DataSourceID="ProfileDataSource">
					<FooterStyle BackColor="White" ForeColor="#000066" />
					<RowStyle ForeColor="#000066" />
					<SelectedRowStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
					<PagerStyle BackColor="White" ForeColor="#000066" HorizontalAlign="Left" />
					<HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />				
				</asp:GridView>
				
				
				<asp:ObjectDataSource ID="ProfileDataSource" 
															runat="server" 
															TypeName="System.Web.Profile.ProfileManager" 
															DataObjectTypeName="System.Web.Profile.ProfileInfo" 
															SelectMethod="GetAllProfiles">
					<SelectParameters>
						<asp:Parameter Type="Object" Name="authenticationOption" DefaultValue="All" />
					</SelectParameters>
				</asp:ObjectDataSource>
				
				<br />
				<asp:Label ID="statusLabel" runat="server" ForeColor="Red"></asp:Label><br />
				
				<br />
				Remove Profiles which haven't been accessed in the last
				<asp:TextBox ID="minuteBox" runat="server" Width="60px"></asp:TextBox>
				Minutes
				<asp:Button ID="removeProfileButton" runat="server" OnClick="removeProfileButton_Click"	Text="Remove" />
				
				<br />
				<br />
				
				<asp:LinkButton runat="server" ID="logoutButton" Text="Log Out" OnClick="logoutButton_Click" />
				<asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/ProfileTest.aspx">Test Profiling</asp:HyperLink>
			</div>
    </form>
</body>
</html>
