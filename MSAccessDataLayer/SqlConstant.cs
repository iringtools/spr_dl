using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.sdk.sql
{
    static class SqlConstant
    {
        public const string DROP_DB = "if exists ( select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 ) DROP TABLE dbo.{0}";

        public const string CREATE_DB = "Create DATABASE [{0}]";

        public const string Get_tableName = "SELECT TABLE_NAME FROM information_schema.tables";
    }
}
