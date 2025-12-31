using System.Collections.Generic;
using System.Runtime.Serialization;

namespace APIService.RetrieveEntityMetadata
{
    public class EntityOption
    {
        public string entityName { get; set; }

        public string displayName { get; set; }
    }

    public enum AttributeTypeCode
    {
        [EnumMember(Value = "Boolean")]
        Boolean,
        [EnumMember(Value = "DateTime")]
        DateTime,
        [EnumMember(Value = "Number")]
        Number,
        [EnumMember(Value = "Lookup")]
        Lookup,
        [EnumMember(Value = "Picklist")]
        Picklist,
        [EnumMember(Value = "String")]
        String,
    }

    public class AttributeItem
    {
        public string logicalName { get; set; }

        public string displayName { get; set; }

        public string attributeType { get; set; }

        public string requiredLevel { get; set; }
    }

    public class StringAttributeItem : AttributeItem
    {
        public StringAttributeItem() { }

        public StringAttributeItem(AttributeItem item)
        {
            logicalName = item.logicalName;
            displayName = item.displayName;
            attributeType = item.attributeType;
            requiredLevel = item.requiredLevel;
        }

        public int strLength { get; set; }

        public string strFormat { get; set; }
    }

    public class BooleanAttributeItem : AttributeItem
    {
        public BooleanAttributeItem() { }

        public BooleanAttributeItem(AttributeItem item)
        {
            logicalName = item.logicalName;
            displayName = item.displayName;
            attributeType = item.attributeType;
            requiredLevel = item.requiredLevel;
        }

        public bool defaultValue { get; set; }

        public string defaultValueStr
        {
            get
            {
                return defaultValue.ToString();
            }
        }
    }

    public class DateTimeAttributeItem : AttributeItem
    {
        public DateTimeAttributeItem() { }

        public DateTimeAttributeItem(AttributeItem item)
        {
            logicalName = item.logicalName;
            displayName = item.displayName;
            attributeType = item.attributeType;
            requiredLevel = item.requiredLevel;
        }

        public string dateTimeFormat { get; set; }

        public string dateTimeBehavior { get; set; }
    }

    public class NumberAttributeItem : AttributeItem
    {
        public NumberAttributeItem() { }

        public NumberAttributeItem(AttributeItem item)
        {
            logicalName = item.logicalName;
            displayName = item.displayName;
            attributeType = item.attributeType;
            requiredLevel = item.requiredLevel;
        }

        public string precision { get; set; }

        public string minimum { get; set; }

        public string maximum { get; set; }

        public bool isMoney { get; set; } = false;

        public string isMonryStr
        {
            get
            {
                return isMoney.ToString();
            }
        }
    }

    public class PickListAttributeItem : AttributeItem
    {
        public PickListAttributeItem() { }

        public PickListAttributeItem(AttributeItem item)
        {
            logicalName = item.logicalName;
            displayName = item.displayName;
            attributeType = item.attributeType;
            requiredLevel = item.requiredLevel;
        }

        public Dictionary<int, string> options { get; set; } = new Dictionary<int, string>();

        public string optionsStr { get; set; }
    }

    public class LookupAttributeItem : AttributeItem
    {
        public LookupAttributeItem() { }

        public LookupAttributeItem(AttributeItem item)
        {
            logicalName = item.logicalName;
            displayName = item.displayName;
            attributeType = item.attributeType;
            requiredLevel = item.requiredLevel;
        }

        public string linkedEntityName { get; set; }

        public string linkedEntityDisplayName { get; set; }
    }
}
