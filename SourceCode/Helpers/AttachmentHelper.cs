using System.Collections.Generic;
using System.IO;

namespace SDK.Helpers
{
    /// <summary>
    /// SAP Business One Attachment Helper
    /// Fetches attachment file paths from ATC1 for Purchase Orders (OPOR)
    /// </summary>
    public static class AttachmentHelper
    {
        /// <summary>
        /// Get attachment file paths for a Purchase Order
        /// </summary>
        /// <param name="docEntry">OPOR.DocEntry</param>
        /// <returns>List of existing attachment file paths</returns>
        public static List<string> GetPurchaseOrderAttachments(int docEntry)
        {
            List<string> files = new List<string>();

            string query = $@"
                SELECT 
                    T1.""trgtPath"" || '\\' || 
                    T1.""FileName"" || '.' || 
                    T1.""FileExt"" AS ""FilePath""
                FROM ATC1 T1
                WHERE T1.""AbsEntry"" = 
                    (SELECT ""AtcEntry"" FROM OPOR WHERE ""DocEntry"" = {docEntry})";

            SAPbobsCOM.Recordset rs =
                (SAPbobsCOM.Recordset)Global.oCompany.GetBusinessObject(
                    SAPbobsCOM.BoObjectTypes.BoRecordset);

            rs.DoQuery(query);

            while (!rs.EoF)
            {
                string filePath = rs.Fields.Item("FilePath").Value.ToString();

                if (File.Exists(filePath))
                    files.Add(filePath);

                rs.MoveNext();
            }

            return files;
        }
    }
}

