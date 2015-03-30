<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="SCAnyLogDownloader.aspx.cs" Inherits="SearchPredicateSample.sitecore.admin.LogDownloader.SCAnyLogDownloader" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <meta http-equiv="content-type" content="text/html; charset=utf-8" />
    <title>Sitecore Any Log Downloader and Viewer</title>
    <link rel="stylesheet" type="text/css" href="Resources/css/LogDownloader.css">
    <link rel="stylesheet" type="text/css" href="//cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/2.3.2/css/bootstrap.min.css" />
    <link rel="stylesheet" type="text/css" href="//cdnjs.cloudflare.com/ajax/libs/bootstrap-modal/2.1.0/bootstrap-modal.min.css" />

    <link href="Resources/css/slider.css" rel="stylesheet" />

    <link rel="stylesheet" type="text/css" href="Resources/css/DT_bootstrap.css">

    <%--<script type="text/javascript" charset="utf-8" language="javascript" src="Resources/js/jquery.js"></script>--%>


    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js"></script>
    <script src="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.9/jquery-ui.js" type="text/javascript"></script>
    <script type="text/javascript" charset="utf-8" language="javascript" src="Resources/js/jquery.dataTables.js"></script>
    <script type="text/javascript" charset="utf-8" language="javascript" src="Resources/js/DT_bootstrap.js"></script>
    <script src="Resources/js/bootstrap-slider.js"></script>
    <%--<link href="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.9/themes/start/jquery-ui.css" rel="stylesheet" type="text/css" />--%>
    <script type="text/javascript" src="//cdnjs.cloudflare.com/ajax/libs/bootstrap-modal/2.1.0/bootstrap-modal.pack.min.js"></script>
    <%--<script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/js/bootstrap.min.js"></script>--%>

    <%--http://www.eyecon.ro/bootstrap-slider/--%>
    <script type="text/javascript">
        function ShowPopup() {
            $(function () {
                $("#dialog").show();
                //$("#dialog").html(message);
                //$("#dialog").dialog({
                //    title: header,
                //    width:600,
                //    height:400,
                //    buttons: {
                //        Close: function () {
                //            $(this).dialog('close');
                //        }
                //    },
                //    modal: true
                //});
            });
        };

        $('#dialog - close').click(function () {
            window.parent.jQuery('#dialog').dialog('close');
        });
    </script>
    <style type="text/css">
        body .modal {
            /* new custom width */
            width: 750px;
            /* must be half of the width, minus scrollbar on the left (30px) */
            margin-left: -375px;
            overflow-y: auto;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container" style="margin-top: 30px">
            <fieldset>
                <legend>Select Log File Location :</legend>
                <div class="row">
                    <div class="span4">
                        <div class="control-group">
                            <div class="controls">
                                <asp:DropDownList ID="ddlFileLocations" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlFileLocations_SelectedIndexChanged">
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                    <div class="span8">
                        <div class="control-group">
                            <div class="controls">
                                <div id="Info">
                                    <asp:Label ID="lblInfo" runat="server" CssClass="" Text="" Visible="False"></asp:Label>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </fieldset>
            <fieldset>
                <legend>Files at the Selected Location</legend>
                <div style="height: 450px; overflow-y: auto">
                    <asp:Table ID="tblLogFiles" CellPadding="0" CellSpacing="0" runat="server" CssClass="table table-striped" />
                </div>
            </fieldset>

            <div style="margin-top: 30px">
                <center>
                <asp:Button ID="btnDownload" CssClass="btn btn-primary" runat="server" Text="Download Selected Log Files" OnClick="btnDownload_Click"></asp:Button>
                    </center>
            </div>

            <!-- Modal -->
            <div id="myModal" class="modal hide fade" tabindex="-1" role="dialog">

                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal">×</button>
                    <h5>Uninstall Package Dialog &nbsp; <sup><span class="label label-warning">BETA</span></sup></h5>
                </div>
                <div class="modal-body">
                    <iframe src="" width="700" height="410" frameborder="0" allowtransparency="true"></iframe>
                </div>
                <div class="modal-footer">
                    <button class="btn" data-dismiss="modal">Close</button>
                </div>
            </div>
            <%--<div class="modal fade" id="Div1" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
                 <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button>
                            <h4 class="modal-title" id="myModalLabel">Modal title</h4>
                        </div>
                        <div class="modal-body">
                            ...
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                            <button type="button" class="btn btn-primary">Save changes</button>
                        </div>
                    </div>
                </div>
            </div>
            <div id="dialog" class="modal hide fadeIn" style="display: none;">
                <div class="modal-header">
                    <asp:Label Style="font: bold" ID="lblViewFileName" runat="server" Text=""></asp:Label>
                    <a 1href="#" class="btn btn-danger" data-dismiss="modal" style="margin-right: auto">X</a>

                </div>
                <div class="modal-body" style="width: 600px; height: 500px">
                    <asp:TextBox ID="txtLogDetails" runat="server" TextMode="MultiLine" Style="width: 100%"></asp:TextBox>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnRefresh" CssClass="btn btn-primary" runat="server" Text="Refresh" />
                    <a href="#" class="btn btn-danger" id="dialog-close" data-dismiss="modal">Close</a>
                </div>--%>
        </div>
        </div>
    </form>
</body>
</html>
