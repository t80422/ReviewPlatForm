using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace WebApplication1.HtmlHelpers
{
    public static class DateInput
    {
        public static MvcHtmlString DateInputFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,Expression<Func<TModel, TProperty>> expression,object htmlAttributes = null)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var dateValue = metadata.Model as DateTime?;

            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            if (dateValue.HasValue)
            {
                attributes["value"] = dateValue.Value.ToString("yyyy-MM-dd");
            }

            // Ensure the input type is 'date'
            attributes["type"] = "date";

            return htmlHelper.TextBoxFor(expression, "{0:yyyy-MM-dd}", attributes);
        }

        public static MvcHtmlString YearMonthInputFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var dateValue = metadata.Model as DateTime?;

            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            if (dateValue.HasValue)
            {
                attributes["value"] = dateValue.Value.ToString("yyyy-MM");
            }

            // Ensure the input type is 'date'
            attributes["type"] = "month";

            return htmlHelper.TextBoxFor(expression, "{0:yyyy-MM}", attributes);
        }

        public static MvcHtmlString TextBoxForWithReadonly<TModel,TProperty>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel,TProperty>> expression,
            bool isReadOnly, 
            object htmlAttributes = null) 
        {
            var attributes=new Dictionary<string,object>();

            if (isReadOnly)
            {
                attributes["readonly"] = "readonly";
            }

            if(htmlAttributes != null)
            {
                var additionalAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

                foreach (var attribute in additionalAttributes)
                {
                    attributes[attribute.Key] = attribute.Value;
                }
            }

            return htmlHelper.TextBoxFor(expression, attributes);
        }
    }
}