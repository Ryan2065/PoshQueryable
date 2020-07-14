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
                case TokenKind.Ccontains:
                case TokenKind.Icontains:
                    var method = leftExpression.Type.GetMethods().Where(p => p.Name == "Contains" && p.GetParameters().Count() == 1).FirstOrDefault();
                    return Expression.Call(leftExpression, method, rightExpression);
                case TokenKind.Ige:
                case TokenKind.Cge:
                    return Expression.GreaterThanOrEqual(leftExpression, rightExpression);
                case TokenKind.Igt:
                case TokenKind.Cgt:
                    return Expression.GreaterThan(leftExpression, rightExpression);
                case TokenKind.Ilt:
                case TokenKind.Clt:
                    return Expression.LessThan(leftExpression, rightExpression);
                case TokenKind.Ile:
                case TokenKind.Cle:
                    return Expression.LessThanOrEqual(leftExpression, rightExpression);
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
                    if (vexp.VariablePath.ToString().ToLower() == "$_")
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
                    if(mexp.Expression.ToString().ToLower() == "$_" || mexp.Expression.ToString().ToLower().StartsWith("$_"))
                    {
                        var stringArray = mexp.Extent.ToString().Split('.');
                        Type ty = typeof(T);
                        returnValue = _p;
                        foreach(var prop in stringArray)
                        {
                            if(prop.ToLower() != "$_")
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
                case ParenExpressionAst pExp:
                    PipelineAst pipeAst = (PipelineAst)pExp.Pipeline;
                    if(pipeAst.PipelineElements[0] is CommandExpressionAst)
                    {
                        var cExp = (CommandExpressionAst)pipeAst.PipelineElements[0];
                        returnValue = GetExpression(cExp.Expression);
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
