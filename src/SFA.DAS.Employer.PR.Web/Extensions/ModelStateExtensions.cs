using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SFA.DAS.Employer.PR.Web.Extensions;

public static class ModelStateExtensions
{
    public static void AddValidationErrors(this ModelStateDictionary modelState, IEnumerable<ValidationFailure> errors)
    {
        foreach (var error in errors)
        {
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}
