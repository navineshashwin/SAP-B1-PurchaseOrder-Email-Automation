using System;

namespace SDK
{
    /// <summary>
    /// Global objects shared across SAP Business One Add-on
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// SAP DI API Company object
        /// </summary>
        public static SAPbobsCOM.Company Company { get; set; }

        /// <summary>
        /// SAP UI API Application object
        /// </summary>
        public static SAPbouiCOM.Application Application { get; set; }

        /// <summary>
        /// Add-on display name
        /// </summary>
        public static string AddonName { get; set; }

        /// <summary>
        /// SAP Server Type (e.g. MSSQL2019, HANA)
        /// </summary>
        public static string ServerType { get; set; }

        /// <summary>
        /// Indicates whether logged-in user is super user
        /// </summary>
        public static bool IsSuperUser { get; set; }
    }
}

