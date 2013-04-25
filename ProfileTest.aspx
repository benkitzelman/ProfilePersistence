<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ProfileTest.aspx.cs" Inherits="ProfileTest" %>
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
				<label>Current Profile :</label> <asp:Label runat="server" ID="username" />
			</div>
			
			<br />
			<br />
			
			<div>
				<label>TextBox Registered with Profiler :	</label><asp:TextBox runat="server" ID="testBox" AutoPostBack="true" /><br />
				<label>Normal TextBox : </label><asp:TextBox runat="server" ID="controlBox" AutoPostBack="true"/>
			</div>
			
			<br />			

			<asp:HyperLink runat="server" ID="LinkButton1" NavigateUrl="~/CurrentProfiles.aspx">Navigate Away</asp:HyperLink>
    </form>
</body>
</html>
