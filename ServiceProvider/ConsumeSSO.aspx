<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ConsumeSSO.aspx.cs" Inherits="ServiceProvider.ConsumeSSO" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <fieldset>
                <legend>User Details</legend>
                <asp:Label ID="lblUser" runat="server"></asp:Label>
            </fieldset>
            <asp:LinkButton ID="lnkInitiateSLO" runat="server" Text="Sign Out" OnClick="lnkInitiateSSO_Click"></asp:LinkButton>

        </div>
    </form>
</body>
</html>
