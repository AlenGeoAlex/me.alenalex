using System.Reflection;
using FastEndpoints;
using NJsonSchema;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Bloggi.Backend.Api.Web.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class QueryEnumAttribute : Attribute
{
    public string[] AllowedValues { get; }
    
    public QueryEnumAttribute(Type enumType)
    {
        AllowedValues = Enum.GetNames(enumType);
    }
}
public class QueryEnumOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var parameters = context.OperationDescription.Operation.Parameters;

        foreach (var parameter in parameters)
        {
            // Get the actual request type from the endpoint
            if (context is not AspNetCoreOperationProcessorContext aspNetContext)
                continue;

            var requestType = aspNetContext.ApiDescription
                .ActionDescriptor
                .EndpointMetadata
                .OfType<EndpointDefinition>()
                .FirstOrDefault()
                ?.ReqDtoType;

            if (requestType is null) continue;

            var prop = requestType.GetProperty(
                parameter.Name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            var attr = prop?.GetCustomAttribute<QueryEnumAttribute>();
            if (attr is null) continue;

            parameter.Schema = new JsonSchema
            {
                Type = JsonObjectType.Array,
                Item = new JsonSchema
                {
                    Type = JsonObjectType.String,
                }
            };
            
            foreach (var value in attr.AllowedValues)
                parameter.Schema.Item.Enumeration.Add(value);
        }

        return true;
    }
}