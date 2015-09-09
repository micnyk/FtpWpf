using System;
using System.Linq.Expressions;

namespace FtpWpf
{
    public static class Extensions
    {
        public static string GetPropertyName(this object obj, Expression<Func<object>> propertyExpression)
        {
            var unaryExpression = propertyExpression.Body as UnaryExpression;
            var memberExpression = unaryExpression == null ? (MemberExpression)propertyExpression.Body : (MemberExpression)unaryExpression.Operand;
            var propertyName = memberExpression.Member.Name;
            return propertyName;
        }
    }
}
