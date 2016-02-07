<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ConsumeSLO.aspx.cs" Inherits="ServiceProvider.ConsumeSLO" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Label ID="lblMsg" runat="server"></asp:Label>
        <asp:HyperLink ID="hypHome" runat="server" Text="Go to Home" NavigateUrl="~/ServicePage.aspx"></asp:HyperLink>
    </div>
    </form>
</body>
</html>
