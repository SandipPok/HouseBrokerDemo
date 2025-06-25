using Dapper;
using HouseBroker.Domain.ValueObjects;
using System.Data;

namespace HouseBroker.Infrastructure.Data.TypeHandlers
{
    public class MoneyTypeHandler : SqlMapper.TypeHandler<Money>
    {
        public override Money Parse(object value)
        {
            return value is decimal amount ? new Money(amount) : new Money(0);
        }

        public override void SetValue(IDbDataParameter parameter, Money value)
        {
            parameter.Value = value.Amount;
            parameter.DbType = DbType.Decimal;
            parameter.Precision = 18;
            parameter.Scale = 2;
        }
    }

}
