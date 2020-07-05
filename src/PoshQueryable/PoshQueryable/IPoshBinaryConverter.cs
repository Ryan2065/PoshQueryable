using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;

namespace PoshQueryable
{
    public interface IPoshBinaryConverter
    {
        IQueryable GetQueryable();
        void ConvertBinaryExpressions(List<BinaryExpressionAst> binaryExpressions);
    }
}
