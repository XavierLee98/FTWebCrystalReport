<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CrParams.aspx.cs" Inherits="FTWebCrystalReport.CrParams" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="/css/style.css" rel="stylesheet" type="text/css" />
    <link href="/css/animate.css" rel="stylesheet" type="text/css" />
    <link href="/css/toastr.css" rel="stylesheet" type="text/css" />
    <link href="/css/jquery-ui-1.12.1.css" rel="stylesheet" />
    <script src="/Scripts/jquery-3.2.1.min.js"></script>
<%--<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.11.0/umd/popper.min.js" ></script>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/js/bootstrap.min.js"></script>--%>

    <script src="/scripts/popper.min.js" ></script>
    <script src="/Scripts/bootstrap.min.js" ></script>

    <script src="/Scripts/jquery.dataTables.min.js"></script>
    <script src="/Scripts/dataTables.bootstrap.min.js"></script>
    <script src="/Scripts/jquery-1.12.1-ui.js"></script>
    <script src="/Scripts/toastr.min.js"></script>

    <style>
        #itemdetails th:nth-child(6), #itemdetails td:nth-child(6) {
        display:none;
        }
    .modal-footer {
        display:block;
    }
    .bootbox-body {
         font-size:16px;
        }
    .modal-footer button {
      float:right;
      margin-left: 15px;
    }
       
        .modal .show {
        display:block important;
        
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
<div class="modal fade" id="loadingModal" tabindex="-1"  role="dialog" aria-labelledby="myModalLabel" data-backdrop="static"  style="z-index:3000">
    <div class="modal-dialog" role="document"  style="max-width:90%;height:auto;position: fixed;top: 50%;  left: 50%; transform: translate(-50%, -50%);-ms-transform:translate(-50%, -50%);-webkit-transform:translate(-50%, -50%);box-shadow:none !important" >
        <button type="button" class="close" data-dismiss="modal" aria-label="Close" style="display:none;"><span aria-hidden="true">&times;</span></button>
        <div id="loadingDiv">
            <div class="sk-double-bounce" >
            <div class="sk-child sk-double-bounce1"></div><div class="sk-child sk-double-bounce2"></div>
            </div>
        </div>
    </div>
</div>
 <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" >
  <div class="modal-dialog" role="document"  style="max-width:90%" >
    <div class="modal-content">
      <div class="modal-header">
          <h5 class="modal-title" id="lblTitle">Title</h5>

       <%-- <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>--%>
      </div>
      <div class="modal-body" style="max-height: calc(100vh - 210px);overflow-y: auto;" >
          <div class="form-group" >
            <div class="col-md-12">
                <input type="text" class="form-control" id="txtID" style="display:none;" />
                <input type="text" class="form-control" id="txtLineID" style="display:none;" />

                <div class="form-group row align-items-center">
                 <!-- Label -->
                 <label for="ddlExportType" class="col-md-3 col-form-label">Export Type</label>
 
                <!-- Combo box -->
                <div class="col-md-9">
                        <select id="ddlExportType" class="form-control">
                            <option value="PortableDocFormat" selected="selected">PDF</option>
                            <option value="ExcelWorkbook">Excel</option>
                        </select>
                    </div>
                </div>
                <table id="itemdetails" class="table table-striped table-bordered table-hover" cellspacing="0"  style="width:100%;">
                <thead>
                    <tr>
                        <th>Id</th>
                        <th>Parameter Name</th>
                        <th>Parameter Name</th>
                        <th>Parameter Value</th>
                        <th>Data Type</th>
                        <th>Param SQL</th>
                    </tr>
                </thead>
                </table>
              </div>
          </div>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-primary" id="btnGenerate" >Generate</button>
     <%--   <button type="button" class="btn btn-default" data-dismiss="modal"  >Close</button>--%>
      </div>
    </div>
  </div>
</div>

    <div class="modal fade" id="lookupModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" style=""width:60%" >
  <div class="modal-dialog" role="document" >
    <div class="modal-content">
        <div class="modal-header">
        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
        <h4 class="modal-title"></h4>
        </div>
        <div class="modal-body">
            <div class="form-group">
                <table id="lookuptable" class="table table-striped table-bordered table-hover" cellspacing="0" width="100%">
                <thead>
                    <tr>
                        <th>Code</th>
                        <th>Name</th>
                    </tr>
                </thead>
                </table>
            </div>
        </div>
        <div class="modal-footer">
            <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
            <button type="button" class="btn btn-primary" id="btnSelect">Select</button>
        </div>
    </div>
  </div>
</div>
    </form>
<script>

    var id_column = 0;
    var paramname_column = 1;
    var paramdisplayname_column = 2;
    var paramvalue_column = 3;
    var datatype_column = 4;
    var paramsql_column = 5;

    var id = $("#txtID");
    var idx = $("#txtLineID");
    function getURLParameters(url) {

        var result = {};
        var searchIndex = url.indexOf("?");
        if (searchIndex == -1) return result;
        var sPageURL = url.substring(searchIndex + 1);
        var sURLVariables = sPageURL.split('&');
        for (var i = 0; i < sURLVariables.length; i++) {
            var sParameterName = sURLVariables[i].split('=');
            result[sParameterName[0]] = sParameterName[1];
        }
        return result;
    }
    function hideModal() {
        setTimeout(function () {
            $("#loadingModal .close").click();
        }, 1000);
    }

    function get_hostname(url) {
        var m = url.match(/^http:\/\/[^/]+/);
        return m ? m[0] : null;
    }
    function ButtonLoading(btn) {
        btn.attr('disabled', true);
        btn.html("Loading...");
    }
    function ButtonReset(btn, text) {
        btn.html(text);
        btn.attr('disabled', false);
    }
    $(document).ready(function () {
        var url = window.location.href;
        var pageTitle = "";
        //var domain_url = "http://localhost:50600";
        var domain_url = get_hostname(url)
        var report_app = ""; //"/FTWebCrystalReport";
        console.log(get_hostname(url));
        var api_path = domain_url + report_app + "/api";
        $.ajax({
            type: "get",
            url: api_path + "/getreportparams/query/-1",
            contentType: "application/json;charset=utf-8",
            dataType: "json",
            data: {},
            success: function (result) {
                console.log(result);
            },
            failure: function () {
                alert("Error");
            },
            complete: function (oSettings, json) {
                setTimeout(function () {
                    hideModal();
                }, 500);
            }
        });
        var lookuptable = $('#lookuptable').DataTable({
            ajax: {
                url: api_path + "/getreportparams/query/-1" ,
                dataSrc: ''
            },
            "columnDefs": [{
                "targets": "_all",
                "orderable": false
            }],
            columnDefs: [{
                targets: "_all",
                createdCell: function (td, cellData, rowData, row, col) {
                    $(td).attr('data-label', $(this).closest('table').find('th').eq(col).html());
                },
            }],
            "columns": [
                { data: "code" },
                { data: "name" },
            ],
            paging: false, bFilter: false, "bInfo": false
        });
        $('#lookuptable tbody').on('click', 'tr', function () {
            //$(this).toggleClass('selected');
            if ($(this).hasClass('selected')) {
                $(this).removeClass('selected');
            }
            else {
                lookuptable.$('tr.selected').removeClass('selected');
                $(this).addClass('selected');
            }
        });
        var itemdetails = $('#itemdetails').DataTable({

            "columnDefs": [{
                "targets": "_all",
                "orderable": false
            }],
            columnDefs: [{
                targets: "_all",
                createdCell: function (td, cellData, rowData, row, col) {
                    $(td).attr('data-label', $(this).closest('table').find('th').eq(col).html());
                },
            }],
            "order": [[0, "asc"]],
            "columns": [
                null, // Id
                null, // Param Name
                null, // Param Display Name
                {
                    mRender: function (data, type, row) {
                        return "<input type='text' class='form-control js-paramvalue' style='width:100%' value='" + data + "' /> "
                    }
                }, // Param value
                null, // data type
                null, // Param SQL
            ],
            paging: false, bFilter: false, "bInfo": false
        });
        var params = getURLParameters(url);
        if (Object.keys(params).length) {
            var report_id = params["id"];
            id.val(report_id);

            $("#loadingModal").modal('show');
         
            $.ajax({
                type: "get",
                url: api_path + "/getreportparams/single/" + report_id,
                contentType: "application/json;charset=utf-8",
                dataType: "json",
                data: {},
                success: function (result) {
                    console.log(result);
                    $("#lblTitle").text(result.rptDisplayName);

                    $.each(result.lines, function (idx1, value1) {
                        itemdetails.row.add([
                            this["id"],
                            this["paramName"],
                            this["paramDisplayName"],
                            "",
                            this["dataType"],
                            this["paramSQL"],
                        ]).draw(false);
                        var idx = itemdetails.row(':last').index();
                        var datatype = itemdetails.cell(idx, datatype_column).data();

                        if (datatype == "date") {
                            var paramvalue = itemdetails.cell(idx, paramvalue_column).node();
                            $('input', paramvalue).datepicker();
                            $('input', paramvalue).datepicker("option", "dateFormat", "dd-M-yy");
                        }
                    });
                    //$('#myModal').addClass("show");

                    $('#myModal').modal({ backdrop: 'static', keyboard: false }, 'show');

                },
                failure: function () {
                    alert("Error");
                },
                complete: function (oSettings, json) {
                    setTimeout(function () {
                        hideModal();
                    }, 500);
                }
            });
            //$('#myModal').addClass("show");
            $('#myModal').modal({ backdrop: 'static', keyboard: false }, 'show');

        }
        $("#btnGenerate").on("click", function (e) {
            e.preventDefault();
            var button = $(this);
            var text = button.html();
            var lines = [];
            var d = itemdetails.rows().data();

            d.each(function (value, index) {
                var paramname = itemdetails.cell(index, paramname_column).data();
                var paramdisplayname = itemdetails.cell(index, paramdisplayname_column).data();
                var paramvalue = $('input', itemdetails.cell(index, paramvalue_column).node()).val();
                var datatype = itemdetails.cell(index, datatype_column).data();

                lines.push({
                    paramname: paramname,
                    paramdisplayname: paramdisplayname,
                    paramvalue: paramvalue,
                    datatype: datatype
                });
            });

            var obj = {
                id: id.val(),
                ReportFormat: $("#ddlExportType").val(),
                lines: lines
            }

            $("#loadingModal").modal('show');
            ButtonLoading(button);
            $.ajax({
                url: api_path + "/GenReport",
                method: "POST",
                data: JSON.stringify(obj),
                contentType: "application/json; charset=utf-8",
                success: function (result) {
                        
                    //downloadFile("pathinfo.pdfpath + "PR/" + result, result);
                    ButtonReset(button, text);
                    toastr.success("Report generated.");
                    //window.open(pathinfo.pdfpath + datainfo.username + "/" + result, '_blank', '');

                    var extension = result.split('.').pop().toLowerCase();
                    var fileUrl = domain_url + report_app + "/Output/" + report_id + "/" + result;

                    console.log(fileUrl, extension);

                    if (extension === "xls" || extension === "xlsx") {
                        var link = document.createElement('a');
                        link.href = fileUrl;
                        link.download = result;
                        document.body.appendChild(link);
                        link.click();
                        document.body.removeChild(link);
                    } else {
                        window.open(fileUrl, '_blank', '');
                        //window.open(domain_url + report_app + "/Output/" + report_id + "/" + result, '_blank', '');

                        setTimeout(function () {
                            window.close();
                        }, 500);
                    }
                    hideModal();
                },
                error: function (res, d) {
                    //toastr.error(jQuery.parseJSON(res.responseText).Message);
                    toastr.error(res.responseText);
                    hideModal();
                    //toastr.error(res.responseJSON.message);
                    ButtonReset(button, text);
                }
            })
        });

        $("#itemdetails").on("click", ".js-paramvalue", function (e) {
            e.preventDefault();
            var value = $(this).val();
            var button = $(this);
            idx.val($(this).parents("tr:first")[0].rowIndex - 1);
            var line_id = itemdetails.cell(idx.val(), id_column).data();
            var sql = itemdetails.cell(idx.val(), paramsql_column).data();

            console.log(sql);
            if (sql != "") {
                $('#lookupModal').modal({ backdrop: 'static', keyboard: false }, 'show');
                lookuptable.ajax.url(api_path + "/getreportparams/query/" + line_id).load();
            }
        });
        $("#btnSelect").on("click", function (e) {
            var data = lookuptable.rows('.selected').data();
            lookuptable.$('tr.selected').removeClass('selected');
            $('input', itemdetails.cell(idx.val(), paramvalue_column).node()).val(data[0]["code"]);
            $('#lookupModal').modal('hide');
        });
    });
</script>
</body>
</html>