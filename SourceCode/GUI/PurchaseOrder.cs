using SDK;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;

namespace GK_DeliverySchedule.GUI
{
    class PurchaseOrder
    {
        public SAPbouiCOM.Form frmPurchaseOrder;
        private SAPbouiCOM.DBDataSource oDBDSHeader;
        private SAPbouiCOM.Matrix oMatrix;

        private static PurchaseOrder instance;
        public static PurchaseOrder Instance => instance ?? (instance = new PurchaseOrder());

        #region Form Initialization

        private void InitForm()
        {
            try
            {
                oDBDSHeader = frmPurchaseOrder.DataSources.DBDataSources.Item("OPOR");
                oMatrix = frmPurchaseOrder.Items.Item("38").Specific;

                SAPbouiCOM.Item btnItem = frmPurchaseOrder.Items.Add(
                    "btnSendMail",
                    SAPbouiCOM.BoFormItemTypes.it_BUTTON);

                btnItem.Top = frmPurchaseOrder.Items.Item("2").Top;
                btnItem.Left = frmPurchaseOrder.Items.Item("2").Left +
                               frmPurchaseOrder.Items.Item("2").Width + 20;
                btnItem.Width = frmPurchaseOrder.Items.Item("2").Width + 10;
                btnItem.Height = frmPurchaseOrder.Items.Item("2").Height;

                ((SAPbouiCOM.Button)btnItem.Specific).Caption = "Send E-Mail";
            }
            catch (Exception ex)
            {
                Global.oApplication.StatusBar.SetText(
                    "InitForm failed : " + ex.Message,
                    SAPbouiCOM.BoMessageTime.bmt_Short,
                    SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            finally
            {
                frmPurchaseOrder.Freeze(false);
            }
        }

        #endregion

        #region Item Events

        internal bool ItemEvent(SAPbouiCOM.ItemEvent pVal)
        {
            try
            {
                if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_FORM_LOAD &&
                    !pVal.Before_Action)
                {
                    frmPurchaseOrder = Global.oApplication.Forms
                        .GetFormByTypeAndCount(142, pVal.FormTypeCount);

                    InitForm();
                }

                if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED &&
                    !pVal.Before_Action &&
                    pVal.ItemUID == "btnSendMail")
                {
                    HandleSendMail();
                }
            }
            catch (Exception ex)
            {
                Global.oApplication.SetStatusBarMessage(
                    "ItemEvent error : " + ex.Message,
                    SAPbouiCOM.BoMessageTime.bmt_Short,
                    true);
            }

            return true;
        }

        private void HandleSendMail()
        {
            if (frmPurchaseOrder.Mode != SAPbouiCOM.BoFormMode.fm_OK_MODE)
            {
                Global.oApplication.MessageBox("Form must be in OK mode.");
                return;
            }

            string cardCode = frmPurchaseOrder.Items.Item("4").Specific.Value;
            if (string.IsNullOrEmpty(cardCode))
            {
                Global.oApplication.MessageBox("Customer Code is empty.");
                return;
            }

            string email = GFun.getSingleValue(
                $"SELECT \"E_Mail\" FROM OCRD WHERE \"CardCode\"='{cardCode}'");

            if (string.IsNullOrEmpty(email))
            {
                Global.oApplication.MessageBox("Customer email is empty.");
                return;
            }

            SendEmail(email);
        }

        #endregion

        #region Email Logic

        private void SendEmail(string toEmail)
        {
            try
            {
                string reportPath = ResolveReportPath();
                string poPdf = GeneratePurchaseOrderPdf(reportPath);

                List<string> attachments = new List<string> { poPdf };
                attachments.AddRange(GetAttachmentFiles(
                    Convert.ToInt32(oDBDSHeader.GetValue("DocEntry", 0))));

                string mergedPdf = MergePdfFiles(attachments);

                if (EmailService.Send(
                    toEmail,
                    "Purchase Order : " + oDBDSHeader.GetValue("DocNum", 0),
                    BuildEmailBody(),
                    mergedPdf))
                {
                    Global.oApplication.MessageBox("Mail sent successfully.");
                }
                else
                {
                    Global.oApplication.MessageBox("Mail sending failed.");
                }
            }
            catch (Exception ex)
            {
                Global.oApplication.SetStatusBarMessage(
                    "SendEmail failed : " + ex.Message,
                    SAPbouiCOM.BoMessageTime.bmt_Short,
                    true);
            }
        }

        private string ResolveReportPath()
        {
            string basePath = System.Windows.Forms.Application.StartupPath;
            return basePath + "\\AP Purchase Order (Item).rpt";
        }

        private string GeneratePurchaseOrderPdf(string reportPath)
        {
            string pdfPath = System.Windows.Forms.Application.StartupPath +
                "\\PO_" + oDBDSHeader.GetValue("DocNum", 0) + ".pdf";

            GFun.PurchaseOrder_CrystalReportToPDF(
                reportPath,
                oDBDSHeader.GetValue("DocEntry", 0),
                pdfPath,
                false);

            return pdfPath;
        }

        private static string BuildEmailBody()
        {
            return new StringBuilder()
                .Append("Dear Sir/Madam,<br/><br/>")
                .Append("Please find attached Purchase Order.<br/><br/>")
                .Append("Regards,<br/>")
                .Append("Company Name")
                .ToString();
        }

        #endregion

        #region Utilities

        private static List<string> GetAttachmentFiles(int docEntry)
        {
            List<string> files = new List<string>();

            string query = $@"
                SELECT T1.""trgtPath"" || '\\' || T1.""FileName"" || '.' || T1.""FileExt"" AS ""FilePath""
                FROM ATC1 T1
                WHERE T1.""AbsEntry"" =
                (SELECT ""AtcEntry"" FROM OPOR WHERE ""DocEntry""={docEntry})";

            SAPbobsCOM.Recordset rs =
                (SAPbobsCOM.Recordset)Global.oCompany.GetBusinessObject(
                    SAPbobsCOM.BoObjectTypes.BoRecordset);

            rs.DoQuery(query);

            while (!rs.EoF)
            {
                string path = rs.Fields.Item("FilePath").Value.ToString();
                if (System.IO.File.Exists(path))
                    files.Add(path);

                rs.MoveNext();
            }

            return files;
        }

        private static string MergePdfFiles(List<string> pdfFiles)
        {
            string output = System.Windows.Forms.Application.StartupPath +
                "\\Merged_PO.pdf";

            using (PdfDocument dest = new PdfDocument(new PdfWriter(output)))
            {
                PdfMerger merger = new PdfMerger(dest);

                foreach (string file in pdfFiles)
                {
                    using (PdfDocument src = new PdfDocument(new PdfReader(file)))
                    {
                        merger.Merge(src, 1, src.GetNumberOfPages());
                    }
                }
            }
            return output;
        }

        #endregion
    }
}

