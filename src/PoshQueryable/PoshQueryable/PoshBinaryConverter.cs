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
        private SessionState _sState;
        public PoshBinaryConverter(IQueryable<T> InputArray, SessionState sState)
        {
            _inputArray = InputArray;
            _p = Expression.Parameter(typeof(T), "p");
            _sState = sState;
        }
        private Expression BuildExpression(BinaryExpressionAst ast)
        {
            var leftExpression = GetExpression(ast.Left);
            var rightExpression = GetExpression(ast.Right);
            switch (ast.Operator)
            {
                case TokenKind.Ieq:
                    if(leftExpression.Type == typeof(string) && rightExpression.Type == typeof(string))
                    {
                        var mi = typeof(String).GetMethods().Where(p => p.GetParameters().Count() == 3 && p.Name == "Equals").FirstOrDefault();
                        var ordinalCase = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                        return Expression.Call(mi, new Expression[] { leftExpression, rightExpression, ordinalCase });
                    }
                    return Expression.Equal(leftExpression, rightExpression);
                case TokenKind.Ceq:
                    return Expression.Equal(leftExpression, rightExpression);
                case TokenKind.And:
                    return Expression.And(leftExpression, rightExpression);
                case TokenKind.Or:
                    return Expression.Or(leftExpression, rightExpression);
                default:
                    return null;
            }
        }
        private Expression GetExpression(ExpressionAst expAst)
        {
            Expression returnValue = null;
            switch (expAst)
            {
                case VariableExpressionAst vexp:
                    if (vexp.VariablePath.ToString().ToLower() == "$p")
                    {
                        returnValue = _p;
                    }
                    else
                    {
                        object value = _sState.PSVariable.GetValue(vexp.VariablePath.ToString());
                        returnValue = Expression.Constant(value);
                    }
                    break;
                case MemberExpressionAst mexp:
                    if(mexp.Expression.ToString().ToLower() == "$p" || mexp.Expression.ToString().ToLower().StartsWith("$p"))
                    {
                        var stringArray = mexp.Extent.ToString().Split('.');
                        Type ty = typeof(T);
                        returnValue = _p;
                        foreach(var prop in stringArray)
                        {
                            if(prop.ToLower() != "$p")
                            {
                                var propertyInfo = ty.GetProperties().Where(p => p.Name.ToLower() == prop.ToLower()).FirstOrDefault();
                                ty = propertyInfo.PropertyType;
                                returnValue = Expression.Property(returnValue, propertyInfo);
                            }
                        }
                    }
                    else
                    {
                        object value = _sState.PSVariable.GetValue(mexp.Extent.ToString());
                        returnValue = Expression.Constant(value);
                    }
                    break;
                case BinaryExpressionAst bexp:
                    returnValue = BuildExpression(bexp);
                    break;
                case ConstantExpressionAst cexp:
                    returnValue = Expression.Constant(cexp.Value);
                    break;
                default:
                    break;
            }
            return returnValue;
        }
        public void ConvertBinaryExpression(BinaryExpressionAst binaryExpression)
        {
            var bExp = BuildExpression(binaryExpression);
            var l = Expression.Lambda<Func<T, bool>>(bExp, _p);
            _inputArray = _inputArray.Where(l);
        }

        public IQueryable GetQueryable()
        {
            return _inputArray;
        }
    }
}
