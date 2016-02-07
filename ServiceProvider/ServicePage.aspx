<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ServicePage.aspx.cs" Inherits="ServiceProvider.ServicePage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
      <h1>Service Provider -1 </h1>
        <asp:LinkButton ID="lnkInitiateSSO" runat="server" Text="Sign In" OnClick="lnkInitiateSSO_Click"></asp:LinkButton>
    </div>
    </form>
</body>
</html>
