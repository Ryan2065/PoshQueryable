using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;

namespace PoshQueryable
{
    public class PoshBinaryConverter<T> : IPoshBinaryConverter
    {
        private IQueryable<T> _inputArray;
        private ParameterExpression _p;
        private BinaryExpression _binExp;
        private Dictionary<string, BinaryExpression> _binExpDictionary;
        private SessionState _sState;
        public PoshBinaryConverter(IQueryable<T> InputArray, SessionState sState)
        {
            _inputArray = InputArray;
            _p = Expression.Parameter(typeof(T), "p");
            _binExpDictionary = new Dictionary<string, BinaryExpression>();
            _sState = sState;
        }
        private BinaryExpression BuildSelfContained(BinaryExpressionAst ast)
        {
            if(ast.Operator == TokenKind.Ieq)
            {
                return Expression.Equal(GetExpression(ast.Left), GetExpression(ast.Right));
            }
            return null;
        }
        private Expression GetExpression(ExpressionAst expAst)
        {
            Expression returnValue = null;
            switch (expAst)
            {
                case VariableExpressionAst vexp:
                    returnValue = Expression.Constant(_sState.PSVariable.GetValue(vexp.VariablePath.ToString()));
                    break;
                case MemberExpressionAst mexp:
                    if(mexp.Expression.ToString().ToLower() == "$p")
                    {
                        var member = mexp.Member.ToString();
                        returnValue = Expression.Property(_p, member);
                    }
                    break;
                default:
                    break;
            }
            return returnValue;
        }
        public void ConvertBinaryExpressions(List<BinaryExpressionAst> binaryExpressions)
        {
            binaryExpressions.Reverse();
            
            foreach(var exp in binaryExpressions)
            {
                string dicKey = exp.Extent.ToString();
                var bExp = BuildSelfContained(exp);
                var l = Expression.Lambda<Func<T, bool>>(bExp, _p);
                _inputArray = _inputArray.Where(l);
            }
        }

        public IQueryable GetQueryable()
        {
            return _inputArray;
        }
    }
}
