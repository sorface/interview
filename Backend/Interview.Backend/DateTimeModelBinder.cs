using System.ComponentModel;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Interview.Backend;

public class DateTimeModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext modelBindingContext)
    {
        if (modelBindingContext == null)
        {
            throw new ArgumentNullException(nameof(modelBindingContext));
        }

        if (modelBindingContext.ModelType != typeof(DateTime))
        {
            return Task.CompletedTask;
        }

        var modelName = modelBindingContext.ModelName;
        var valueProviderResult = modelBindingContext.ValueProvider.GetValue(modelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        modelBindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
        var dateTimeToParse = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(dateTimeToParse))
        {
            return Task.CompletedTask;
        }

        if (DateTime.TryParseExact(dateTimeToParse, DateTimeJsonConverter.DefaultDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var validDate))
        {
            modelBindingContext.Result = ModelBindingResult.Success(validDate);
            return Task.CompletedTask;
        }

        modelBindingContext.ModelState.TryAddModelError(modelBindingContext.ModelName, "The date must be in utc format. For example: 2022-01-01T00:00:00Z");
        return Task.CompletedTask;
    }
}

public class DateTimeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        return context.Metadata.ModelType == typeof(DateTime) ? new BinderTypeModelBinder(typeof(DateTimeModelBinder)) : null;
    }
}
