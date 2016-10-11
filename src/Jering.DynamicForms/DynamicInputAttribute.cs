using System;

namespace Jering.DynamicForms
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DynamicInputAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string DisplayNameResourceName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string InitialValueResourceName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Type ResourceType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PlaceholderResourceName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool RenderLabel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayNameResourceName"></param>
        /// <param name="initialValueResourceName"></param>
        /// <param name="order"></param>
        /// <param name="type"></param>
        /// <param name="resourceType"></param>
        /// <param name="tagName"></param>
        /// <param name="placeholderResourceName"></param>
        /// <param name="renderLabel"></param>
        public DynamicInputAttribute(bool renderLabel, 
            string type, 
            string displayNameResourceName, 
            Type resourceType, 
            string tagName, 
            int order = 0, 
            string initialValueResourceName = null, 
            string placeholderResourceName = null)
        {
            PlaceholderResourceName = placeholderResourceName;
            RenderLabel = renderLabel;
            Type = type;
            Order = order;
            DisplayNameResourceName = displayNameResourceName;
            InitialValueResourceName = initialValueResourceName;
            TagName = tagName;
            ResourceType = resourceType;
        }
    }
}
