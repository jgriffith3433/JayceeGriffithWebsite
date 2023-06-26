using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Linq;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Common;

namespace ContainerNinja.Contracts.Enum
{
    [JsonConverter(typeof(KitchenUnitTypeStringEnumConverter))]
    public enum KitchenUnitType
    {
        [Display(Name = "")]
        [EnumMember(Value = "")]
        [EnumNames("")]
        None = 0,
        
        [Display(Name = "Bulk")]
        [EnumMember(Value = "Bulk")]
        [EnumNames("Bulk")]
        Bulk = 1,
        
        [Display(Name = "Ounce")]
        [EnumMember(Value = "Ounce")]
        [EnumNames("Ounce", "Ounces")]
        Ounce = 2,

        [Display(Name = "Teaspoon")]
        [EnumMember(Value = "Teaspoon")]
        [EnumNames("Teaspoon", "Teaspoons")]
        Teaspoon = 3,
        
        [Display(Name = "Tablespoon")]
        [EnumMember(Value = "Tablespoon")]
        [EnumNames("Tablespoon", "Tablespoons")]
        Tablespoon = 4,
        
        [Display(Name = "Pound")]
        [EnumMember(Value = "Pound")]
        [EnumNames("Pound", "Pounds")]
        Pound = 5,
        
        [Display(Name = "Cup")]
        [EnumMember(Value = "Cup")]
        [EnumNames("Cup", "Cups")]
        Cup = 6,

        [Display(Name = "Clove")]
        [EnumMember(Value = "Clove")]
        [EnumNames("Clove", "Cloves")]
        Clove = 7,
        
        [Display(Name = "Can")]
        [EnumMember(Value = "Can")]
        [EnumNames("Can", "Cans")]
        Can = 8,

        [Display(Name = "Whole")]
        [EnumMember(Value = "Whole")]
        [EnumNames("Whole")]
        Whole = 9,
        
        [Display(Name = "Package")]
        [EnumMember(Value = "Package")]
        [EnumNames("Package", "Packages")]
        Package = 10,
        
        [Display(Name = "Bar")]
        [EnumMember(Value = "Bar")]
        [EnumNames("Bar", "Bars")]
        Bar = 11,

        [Display(Name = "Bun")]
        [EnumMember(Value = "Bun")]
        [EnumNames("Bun", "Buns")]
        Bun = 12,

        [Display(Name = "Bottle")]
        [EnumMember(Value = "Bottle")]
        [EnumNames("Bottle", "Bottles")]
        Bottle = 13,
        
        [Display(Name = "Slice")]
        [EnumMember(Value = "Slice")]
        [EnumNames("Slice", "Slices")]
        Slice = 14,
        
        [Display(Name = "Box")]
        [EnumMember(Value = "Box")]
        [EnumNames("Box", "Boxes")]
        Box = 15,

        [Display(Name = "Bag")]
        [EnumMember(Value = "Bag")]
        [EnumNames("Bag", "Bags")]
        Bag = 16,

        [Display(Name = "Gallon")]
        [EnumMember(Value = "Gallon")]
        [EnumNames("Gallon", "Gallons")]
        Gallon = 17,
        
        [Display(Name = "Gram")]
        [EnumMember(Value = "Gram")]
        [EnumNames("Gram", "Grams")]
        Gram = 18,

        [Display(Name = "Milliliter")]
        [EnumMember(Value = "Milliliter")]
        [EnumNames("Milliliter", "Milliliters")]
        Milliliters = 19,
        
        [Display(Name = "Leaf")]
        [EnumMember(Value = "Leaf")]
        [EnumNames("Leaf", "Leaves")]
        Leaves = 20,
        
        [Display(Name = "Handful")]
        [EnumMember(Value = "Handful")]
        [EnumNames("Handful", "Handfuls")]
        Handful = 21,
        
        [Display(Name = "Packet")]
        [EnumMember(Value = "Packet")]
        [EnumNames("Packet", "Packets")]
        Packet = 22,
        
        [Display(Name = "Count")]
        [EnumMember(Value = "Count")]
        [EnumNames("Count")]
        Count = 23,
        
        [Display(Name = "Pint")]
        [EnumMember(Value = "Pint")]
        [EnumNames("Pint", "Pints")]
        Pint = 24,
        
        [Display(Name = "Pinch")]
        [EnumMember(Value = "Pinch")]
        [EnumNames("Pinch", "Pinches")]
        Pinch = 25,
        
        [Display(Name = "Breast")]
        [EnumMember(Value = "Breast")]
        [EnumNames("Breast", "Breasts")]
        Breast = 26,
    }

    public static class KitchenUnitTypeExtensionMethods
    {
        public static KitchenUnitType UnitTypeFromString(this string unitTypeStr)
        {
            switch (unitTypeStr.ToLower())
            {
                case "":
                case "none":
                    return KitchenUnitType.None;
                case "bulk":
                    return KitchenUnitType.Bulk;
                case "ounce":
                case "ounces":
                    return KitchenUnitType.Ounce;
                case "teaspoon":
                case "teaspoons":
                    return KitchenUnitType.Teaspoon;
                case "tablespoon":
                case "tablespoons":
                    return KitchenUnitType.Tablespoon;
                case "pound":
                case "pounds":
                    return KitchenUnitType.Pound;
                case "cup":
                case "cups":
                    return KitchenUnitType.Cup;
                case "clove":
                case "cloves":
                    return KitchenUnitType.Clove;
                case "can":
                case "cans":
                    return KitchenUnitType.Can;
                case "whole":
                case "wholes":
                    return KitchenUnitType.Whole;
                case "package":
                case "packages":
                    return KitchenUnitType.Package;
                case "bar":
                case "bars":
                    return KitchenUnitType.Bar;
                case "bun":
                case "buns":
                    return KitchenUnitType.Bun;
                case "bottle":
                case "bottles":
                    return KitchenUnitType.Bottle;
                case "slice":
                case "slices":
                    return KitchenUnitType.Slice;
                case "box":
                case "boxes":
                    return KitchenUnitType.Box;
                case "bag":
                case "bags":
                    return KitchenUnitType.Bag;
                case "gallon":
                case "gallons":
                    return KitchenUnitType.Gallon;
                case "gram":
                case "grams":
                    return KitchenUnitType.Gram;
                case "milliliter":
                case "milliliters":
                    return KitchenUnitType.Milliliters;
                case "leaf":
                case "leaves":
                    return KitchenUnitType.Leaves;
                case "handful":
                case "handfuls":
                    return KitchenUnitType.Handful;
                case "packet":
                case "packets":
                    return KitchenUnitType.Packet;
                case "count":
                case "counts":
                    return KitchenUnitType.Count;
                case "pint":
                case "pints":
                    return KitchenUnitType.Pint;
                case "pinch":
                case "pinches":
                    return KitchenUnitType.Pinch;
                case "breast":
                case "breasts":
                    return KitchenUnitType.Breast;
                default:
                    return KitchenUnitType.None;
            }
        }

        public static TAttribute GetAttribute<TAttribute>(this System.Enum value) where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = System.Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }
    }

    public class KitchenUnitTypeStringEnumConverter : StringEnumConverter
    {
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            try
            {
                if (reader.TokenType == JsonToken.String)
                {
                    string? enumText = reader.Value?.ToString();
                    if (enumText == null)
                    {
                        return KitchenUnitType.None;
                    }
                    return enumText.UnitTypeFromString();
                }

                if (reader.TokenType == JsonToken.Integer)
                {
                    return (KitchenUnitType)reader.Value;
                }
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException(string.Format("Error converting value {0} to type '{1}'.", reader.Value, objectType));
            }

            // we don't actually expect to get here.
            throw new JsonSerializationException("Unexpected token {0} when parsing enum.");
        }
    }

    public class StringEnumKitchenUnitTypeGenerationProvider : StringEnumGenerationProvider
    {
        public static bool IsNullableEnum(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>) && t.GetGenericArguments()[0].IsEnum;
        }
        public override JSchema GetSchema(JSchemaTypeGenerationContext context)
        {
            var isNullable = IsNullableEnum(context.ObjectType);
            var t = isNullable ? Nullable.GetUnderlyingType(context.ObjectType) : context.ObjectType;
            if (t != typeof(KitchenUnitType))
            {
                return base.GetSchema(context);
            }
            JSchema schema = new JSchema
            {
                Title = context.SchemaTitle,
                Description = context.SchemaDescription,
                Type = JSchemaType.String
            };


            //object? defaultValue = context.MemberProperty?.DefaultValue;
            //if (defaultValue != null)
            //{
            //    schema.Default = defaultValue;
            //}

            var defaultEnumValues = System.Enum.GetValues(typeof(KitchenUnitType)).Cast<KitchenUnitType>().ToList();

            foreach(var enumValue in defaultEnumValues)
            {
                var enumNamesAttribute = enumValue.GetAttribute<EnumNames>();
                if (enumNamesAttribute != null)
                {
                    foreach (var name in enumNamesAttribute.Names)
                    {
                        if (schema.Enum.FirstOrDefault(e => e.Value<string>().ToLower() == name.ToLower()) == null)
                        {
                            schema.Enum.Add(JValue.CreateString(name));
                        }
                    }
                }
                else
                {
                    if (schema.Enum.FirstOrDefault(e => e.Value<string>().ToLower() == enumValue.ToString()) == null)
                    {
                        schema.Enum.Add(JValue.CreateString(enumValue.ToString()));
                    }
                }
            }

            return schema;
        }
    }
}
