using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;

namespace GraphQL.Instrumentation
{
    public static class ApolloTracingExtensions
    {
        public static void EnrichWithApolloTracing(this ExecutionResult result, DateTime start)
        {
            var perf = result?.Perf;
            if (perf == null)
            {
                return;
            }

            var trace = CreateTrace(result.Operation, perf, start);
            if (result.Extensions == null)
            {
                result.Extensions = new Dictionary<string, object>();
            }
            result.Extensions["tracing"] = trace;
        }

        public static ApolloTrace CreateTrace(
            Operation operation,
            PerfRecord[] perf,
            DateTime start)
        {
            var operationStat = perf.FirstOrDefault(x => x.Category == "operation");
            var documentStats = perf.Where(x => x.Category == "document");
            var fieldStats = perf.Where(x => x.Category == "field");

            var trace = new ApolloTrace(start, operationStat?.Duration ?? 0);

            var parsingStat = documentStats.FirstOrDefault(x => x.Subject == "Building document");
            if (parsingStat != null)
            {
                trace.Parsing.StartOffset = ApolloTrace.ConvertTime(parsingStat.Start);
                trace.Parsing.Duration = ApolloTrace.ConvertTime(parsingStat.Duration);
            }

            var validationStat = documentStats.FirstOrDefault(x => x.Subject == "Validating document");
            if (validationStat != null)
            {
                trace.Validation.StartOffset = ApolloTrace.ConvertTime(validationStat.Start);
                trace.Validation.Duration = ApolloTrace.ConvertTime(validationStat.Duration);
            }

            foreach (var fieldStat in fieldStats)
            {
                var stringPath = fieldStat.MetaField<IEnumerable<string>>("path");
                trace.Execution.Resolvers.Add(
                    new ApolloTrace.ResolverTrace
                    {
                        FieldName = fieldStat.MetaField<string>("fieldName"),
                        Path = ConvertPath(stringPath).ToList(),
                        ParentType = fieldStat.MetaField<string>("typeName"),
                        ReturnType = fieldStat.MetaField<string>("returnTypeName"),
                        StartOffset = ApolloTrace.ConvertTime(fieldStat.Start),
                        Duration = ApolloTrace.ConvertTime(fieldStat.Duration),
                    });
            }

            return trace;
        }

        private static IEnumerable<object> ConvertPath(IEnumerable<string> stringPath)
        {
            foreach (var step in stringPath)
            {
                if (int.TryParse(step, out var arrayIndex))
                {
                    yield return arrayIndex;
                }
                else
                {
                    yield return step;
                }
            }
        }
    }
}
