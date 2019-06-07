﻿using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace FoolProof.Core
{
	public class FoolProofValidationAdapter : AttributeAdapterBase<ModelAwareValidationAttribute>
    {
        public FoolProofValidationAdapter(ModelAwareValidationAttribute attribute, IStringLocalizer stringLocalizer)
            : base(attribute, stringLocalizer) { }

		public override void AddValidation(ClientModelValidationContext context)
		{
			if (Attribute is ContingentValidationAttribute)
			{
				var attribute = Attribute as ContingentValidationAttribute;
				var otherPropertyInfo = context.ModelMetadata.ContainerType.GetProperty(attribute.DependentProperty);

				var displayName = GetMetaDataDisplayName(otherPropertyInfo);
				if (displayName != null)
					attribute.DependentPropertyDisplayName = displayName;
			}

			//Add validation rule attributes
			MergeAttribute(context.Attributes, "data-val", "true");
			MergeAttribute(context.Attributes, $"data-val-{Attribute.ClientTypeName.ToLowerInvariant()}", GetErrorMessage(context));

			//Add validation params attributes
			foreach (var validationParam in Attribute.ClientValidationParameters)
				MergeAttribute(context.Attributes, $"data-val-{validationParam.Key}", validationParam.Value + "");
		}

		public override string GetErrorMessage(ModelValidationContextBase validationContext)
		{
			return GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
		}


		private string GetAttributeDisplayName(PropertyInfo property)
        {
            var atts = property.GetCustomAttributes(typeof(DisplayAttribute), true);

            if (atts.Length == 0)
                return null;

            return (atts[0] as DisplayAttribute).GetName();
        }

        private string GetMetaDataDisplayName(PropertyInfo property)
        {
            var atts = property.DeclaringType.GetCustomAttributes(
                typeof(ModelMetadataTypeAttribute), true);

            if (atts.Length == 0)
                return GetAttributeDisplayName(property); 
            
            var metaAttr = atts[0] as ModelMetadataTypeAttribute;
            
            var metaProperty = metaAttr.MetadataType.GetProperty(property.Name);
            
            if (metaProperty == null)
                return null;

            return GetAttributeDisplayName(metaProperty);
        }
    }
}