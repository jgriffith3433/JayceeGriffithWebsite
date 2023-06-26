using System.Text.Json.Nodes;
using System.Text.Json;
using Newtonsoft.Json.Schema;
using System.Security.Principal;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using ContainerNinja.Contracts.Enum;

namespace ContainerNinja.Contracts.Common
{
    public class ChatCommandSpecification : Attribute
    {
        protected JSchema? m_SchemaObject = null;
        protected IList<ValidationError> m_ValidationErrors = null;

        public ChatCommandSpecification(string name, string? description)
        {
            Names = new string[]
            {
                name
            };
            Description = description;
        }

        public ChatCommandSpecification(string[] names, string? description)
        {
            Names = names;
            Description = description;
        }

        public bool IsValidAgainstSchema(JObject jsonObject, Type type)
        {
            EnsureSchemaCreated(type);
            return jsonObject.IsValid(m_SchemaObject, out m_ValidationErrors);
        }

        public IList<ValidationError> GetValidationErrors()
        {
            return m_ValidationErrors;
        }

        protected void EnsureSchemaCreated(Type type)
        {
            if (m_SchemaObject == null)
            {
                var generator = new JSchemaGenerator
                {
                    SchemaReferenceHandling = SchemaReferenceHandling.None,
                    DefaultRequired = Required.Default
                };
                generator.GenerationProviders.Add(new StringEnumKitchenUnitTypeGenerationProvider());
                m_SchemaObject = generator.Generate(type);
                m_SchemaObject.AllowAdditionalItems = false;
                m_SchemaObject.AllowAdditionalProperties = false;
            }
        }

        public dynamic? GetFunctionParametersFromType(Type type)
        {
            EnsureSchemaCreated(type);
            var schemaObject = JsonConvert.DeserializeObject<ExpandoObject>(m_SchemaObject.ToString());

            if (schemaObject.Any(s => s.Key == "properties") == false)
            {
                schemaObject.TryAdd("properties", new JObject());
            }
            return schemaObject;

            //var jsonObject = JsonSerializer.Deserialize<JsonObject>(jsonString, new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true,
            //    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            //    AllowTrailingCommas = true,
            //});
            //if (jsonObject["properties"] == null)
            //{
            //    jsonObject.Add("properties", new JsonObject());
            //}
            //return jsonObject;
            //return JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true,
            //    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            //    AllowTrailingCommas = true,
            //});
        }

        public string[] Names { get; set; }
        public string? Description { get; set; }
    }
}


/*using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;

namespace ContainerNinja.Contracts.Common
{
    public class ChatCommandSpecification : Attribute
    {
        public ChatCommandSpecification(string name, string? description)
        {
            Names = new string[]
            {
                name
            };
            Description = description;
        }

        public ChatCommandSpecification(string[] names, string? description)
        {
            Names = names;
            Description = description;
        }

        public void CreateFunctionParametersFromType(Type type)
        {
            var properties = type.GetProperties();

            var functionParameterPropertiesObject = new JsonObject();
            var functionParameterPropertiesRequiredListArray = new JsonArray();

            foreach (var property in properties)
            {
                var required = property.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(RequiredAttribute)) != null;
                var description = property.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(DescriptionAttribute));
                var typeName = property.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(JsonSchemaTypeName));
                //var enumList = property.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(JsonSchemaEnumList));

                var functionParameterPropertyObject = new JsonObject();
                {
                    functionParameterPropertyObject.Add("type", typeName.ToString());
                    functionParameterPropertyObject.Add("description", description.ToString());
                    //functionParameterPropertyObject.Add("enum", string.Join(", ", enumList));
                };
                if (required)
                {
                    functionParameterPropertiesRequiredListArray.Add(property.Name);
                }
                functionParameterPropertiesObject.Add(property.Name, functionParameterPropertyObject);
            }
            var functionParametersObject = new JsonObject();
            {
                functionParametersObject.Add("type", "object");
                functionParametersObject.Add("properties", functionParameterPropertiesObject);
                functionParametersObject.Add("required", functionParameterPropertiesRequiredListArray);
            };
        }

        public string[] Names { get; set; }
        public string? Description { get; set; }
        public JsonObject? FunctionParameters { get; set; }
    }
}
*/