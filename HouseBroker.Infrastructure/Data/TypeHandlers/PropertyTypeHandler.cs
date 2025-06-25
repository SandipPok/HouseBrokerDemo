using Dapper;
using HouseBroker.Domain.Enums;
using System.Data;

namespace HouseBroker.Infrastructure.Data.TypeHandlers
{
    public class PropertyTypeHandler : SqlMapper.TypeHandler<PropertyType>
    {
        public override PropertyType Parse(object value)
        {
            return Enum.Parse<PropertyType>(value.ToString()!);
        }

        public override void SetValue(IDbDataParameter parameter, PropertyType value)
        {
            parameter.Value = value.ToString();
            parameter.DbType = DbType.String;
            parameter.Size = 50;
        }
    }
}