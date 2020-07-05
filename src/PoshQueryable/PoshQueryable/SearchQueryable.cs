using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.Serialization;

namespace PoshQueryable
{
    [Cmdlet(VerbsCommon.Search,"Queryable")]
    [OutputType(typeof(IQueryable<object>))]
    public class SearchQueryable : PSCmdlet
    {
        public SearchQueryable()
        {
            _binaryExpressions = new List<BinaryExpressionAst>();
        }
        [Parameter(
            Mandatory = true,
            Position = 0)]
        public object InputArray { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true)]
        public ScriptBlock Expression { get; set; }
        private List<BinaryExpressionAst> _binaryExpressions;
        private IPoshBinaryConverter _binaryConverter;
        protected override void BeginProcessing()
        {
            var expressions = Expression.Ast.FindAll(p => p.GetType().Name.Equals("BinaryExpressionAst"), true);
            foreach(BinaryExpressionAst exp in expressions)
            {
                _binaryExpressions.Add(exp);
            }
            if(_binaryExpressions.Count == 0)
            {
                throw new Exception("Error parsing expression - No binary expressions found!");
            }
            var ienum = (IEnumerable)InputArray;
            var qu = ienum.AsQueryable();
            var arguments = qu.GetType().GetTypeInfo().GenericTypeArguments;
            if (arguments.Count() > 0)
            {
                var genericType = arguments[0];
                var poshBinaryType = typeof(PoshBinaryConverter<>);
                var constructedPoshBinaryType = poshBinaryType.MakeGenericType(genericType);
                _binaryConverter = (IPoshBinaryConverter)Activator.CreateInstance(constructedPoshBinaryType, new object[] { qu,SessionState });
            }
            else
            {
                throw new Exception("Error parsing InputArray - Must be a strongly typed collection like generic collections. See more here: https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic?view=dotnet-plat-ext-3.1");
            }
        }
        protected override void ProcessRecord()
        {
            _binaryConverter.ConvertBinaryExpressions(_binaryExpressions);
        }
        protected override void EndProcessing()
        {
            WriteObject(_binaryConverter.GetQueryable());
        }
        
    }

}
