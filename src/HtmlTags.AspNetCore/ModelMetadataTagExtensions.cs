﻿using System;
using System.Globalization;
using HtmlTags.Conventions;
using HtmlTags.Conventions.Elements;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace HtmlTags
{
    public static class ModelMetadataTagExtensions
    {
        public static void ModelMetadata(this HtmlConventionRegistry registry)
        {
            registry.Labels.Modifier<DisplayNameElementModifier>();
            registry.Displays.Modifier<MetadataModelDisplayModifier>();
            registry.Editors.Modifier<MetadataModelEditModifier>();
            registry.Editors.Modifier<PlaceholderElementModifier>();
            registry.Editors.Modifier<ModelStateErrorsModifier>();
        }

        private class DisplayNameElementModifier : IElementModifier
        {
            public bool Matches(ElementRequest token) 
                => token.Get<ModelExplorer>()?.Metadata.DisplayName != null;

            public void Modify(ElementRequest request)
                => request.CurrentTag.Text(request.Get<ModelExplorer>().Metadata.DisplayName);
        }

        private static object BuildFormattedModelValue(ElementRequest request, Func<ModelMetadata, string> formatStringFinder)
        {
            var modelMetadata = request.Get<ModelExplorer>().Metadata;

            var formattedModelValue = request.RawValue;

            if (request.RawValue == null)
            {
                formattedModelValue = modelMetadata.NullDisplayText;
            }

            var formatString = formatStringFinder(modelMetadata);

            if (formatString != null && formattedModelValue != null)
            {
                formattedModelValue = string.Format(CultureInfo.CurrentCulture, formatString, formattedModelValue);
            }

            return formattedModelValue;
        }

        private class MetadataModelDisplayModifier : IElementModifier
        {
            public bool Matches(ElementRequest token) 
                => token.Get<ModelExplorer>() != null;

            public void Modify(ElementRequest request) 
                => request.CurrentTag.Text(BuildFormattedModelValue(request, m => m.DisplayFormatString)?.ToString());
        }

        private class MetadataModelEditModifier : IElementModifier
        {
            public bool Matches(ElementRequest token)
                => token.Get<ModelExplorer>() != null;

            public void Modify(ElementRequest request) 
                => request.CurrentTag.Value(BuildFormattedModelValue(request, m => m.EditFormatString)?.ToString());
        }

        private class PlaceholderElementModifier : IElementModifier
        {
            public bool Matches(ElementRequest token) 
                => token.Get<ModelExplorer>()?.Metadata.Placeholder != null;

            public void Modify(ElementRequest request) 
                => request.CurrentTag.Attr("placeholder", request.Get<ModelExplorer>().Metadata.Placeholder);
        }

        private class ModelStateErrorsModifier : IElementModifier
        {
            public bool Matches(ElementRequest token) 
                => token.TryGet(out IHtmlHelper helper)
                   && token.TryGet(out ElementName elementName)
                   && helper.ViewData.ModelState.TryGetValue(elementName.Value, out var entry)
                   && entry.Errors.Count > 0;

            public void Modify(ElementRequest request) 
                => request.CurrentTag.AddClass(HtmlHelper.ValidationInputCssClassName);
        }
    }
}