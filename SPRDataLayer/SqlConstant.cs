using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bechtel.iRING.SPR
{
    static class SqlConstant
    {
        public const string DROP_DB = "if exists ( select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 ) DROP TABLE dbo.{0}";

        public const string CREATE_DB = "Create DATABASE [{0}]";

        public const string Get_tableName = "SELECT TABLE_NAME FROM information_schema.tables";

        public const string IndexOn_tblLabels = "CREATE CLUSTERED INDEX SPRindex ON dbo.labels (linkage_index)";

        public const string IndexOn_tblLabel_Values = "CREATE CLUSTERED INDEX SPRLabelValueindex ON dbo.label_values (label_value_index)";

        public const string MDBProvider = "Microsoft.Jet.OLEDB.4.0";

        public const string ifPrimaryKey = "SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE  CONSTRAINT_TYPE = 'PRIMARY KEY'" +
                                           " AND TABLE_NAME = '{0}'";

        public const string dropPrimaryKey = "ALTER TABLE {0} DROP CONSTRAINT {1}";
        

    }
}
