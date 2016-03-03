<%@ Page Title="Generate Playlist" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="GeneratePlaylistWebApp._Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %></h1>
            </hgroup>
            <p>
                <asp:Table runat="server">
                    <asp:TableRow>
                        <asp:TableCell>Playlist</asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell />
                        <asp:TableCell>Title</asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox runat="server"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell />
                        <asp:TableCell>Description</asp:TableCell>
                        <asp:TableCell>
                            <asp:TextBox runat="server"></asp:TextBox>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell />
                        <asp:TableCell>Output</asp:TableCell>
                        <asp:TableCell>
                            <asp:DropDownList runat="server"></asp:DropDownList>
                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell />
                        <asp:TableCell>Bounds</asp:TableCell>
                        <asp:TableCell>
                            <asp:Table runat="server">
                                <asp:TableRow>
                                    <asp:TableCell>Track Nr.</asp:TableCell>
                                    <asp:TableCell>
                                        <asp:TextBox runat="server"></asp:TextBox>
                                    </asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow>
                                    <asp:TableCell>Duration</asp:TableCell>
                                    <asp:TableCell>
                                        <asp:TextBox runat="server"></asp:TextBox>
                                    </asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
            </p>
        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
</asp:Content>
