using Dapper;
using HouseBroker.Domain.ValueObjects;
using System.Data;

namespace HouseBroker.Infrastructure.Data.TypeHandlers
{
    public class LocationTypeHandler : SqlMapper.TypeHandler<Location>
    {
        public override Location Parse(object value)
        {
            if (value is string str)
            {
                var parts = str.Split('|');
                return new Location(parts[0], parts[1], parts[2]);
            }
            return new Location("", "", "");
        }

        public override void SetValue(IDbDataParameter parameter, Location value)
        {
            parameter.Value = $"{value.Street}|{value.City}|{value.PostalCode}";
            parameter.DbType = DbType.String;
            parameter.Size = 300;
        }
    }
}
