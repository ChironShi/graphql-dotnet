using System.Collections.Generic;
using GraphQL.Language.AST;

namespace GraphQL.Validation.Rules
{
    /// <summary>
    /// Unique variable names
    ///
    /// A GraphQL operation is only valid if all its variables are uniquely named.
    /// </summary>
    public class UniqueVariableNames : IValidationRule
    {
        public string DuplicateVariableMessage(string variableName)
        {
            return $"There can be only one variable named \"{variableName}\"";
        }

        public INodeVisitor Validate(ValidationContext context)
        {
            Dictionary<string, VariableDefinition> knownVariables = null;

            return new EnterLeaveListener(_ =>
            {
                _.Match<Operation>(__ => knownVariables = new Dictionary<string, VariableDefinition>());

                _.Match<VariableDefinition>(variableDefinition =>
                {
                    var variableName = variableDefinition.Name;

                    if (knownVariables.ContainsKey(variableName))
                    {
                        var error = new ValidationError(
                            context.OriginalQuery,
                            "5.7.1",
                            DuplicateVariableMessage(variableName),
                            knownVariables[variableName],
                            variableDefinition)
                        {
                            Path = context.TypeInfo.GetPath()
                        };
                        context.ReportError(error);
                    }
                    else
                    {
                        knownVariables[variableName] = variableDefinition;
                    }
                });
            });
        }
    }
}
