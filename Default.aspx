<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>    
</head>
<body>
    <form id="form1" runat="server">
    <div>
			<asp:Login ID="Login1" runat="server" OnAuthenticate="Login1_Authenticate" OnLoggedIn="Login1_LoggedIn">
			</asp:Login>
			
			<hr />
			<br /><br />			
			<asp:Label runat="server" ID="profileUser" /><br />
			<asp:Label runat="server" ID="profileName" />
    </div>
    
    </form>
</body>
</html>
