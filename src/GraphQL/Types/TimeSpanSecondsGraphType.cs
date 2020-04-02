using System;
using GraphQL.Language.AST;

namespace GraphQL.Types
{
    public class TimeSpanSecondsGraphType : ScalarGraphType
    {
        public TimeSpanSecondsGraphType()
        {
            Name = "Seconds";
            Description =
                "The `Seconds` scalar type represents a period of time represented as the total number of seconds.";
        }

        public override object Serialize(object value)
        {
            var v = ParseValue(value) as TimeSpan?;
            return v?.TotalSeconds;
        }

        public override object ParseValue(object value)
        {
            if (value is TimeSpan timeSpan) return timeSpan;

            if (value is int || value is long || value is float || value is double || value is decimal)
            {
                return TimeSpan.FromSeconds(Convert.ToDouble(value));
            }

            if (value is string strVal)
            {
                return TimeSpan.TryParse(strVal, out var ts) ? ts : null as TimeSpan?;
            }

            return null;
        }

        public override object ParseLiteral(IValue value)
        {
            return ParseValue(value?.Value);
        }
    }
}
