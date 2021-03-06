﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using System.Collections.Generic;
using System.Linq;

namespace Etch.OrchardCore.Workflows.Validation.Workflows.Activities
{
    public class ValidateRequiredWhenCheckedTask : TaskActivity
    {
        #region Constants

        private const string OutcomeDone = "Done";
        private const string OutcomeInvalid = "Invalid";
        private const string OutcomeValid = "Valid";

        #endregion Constants

        #region Dependencies

        private IHttpContextAccessor _httpContextAccessor;
        private IUpdateModelAccessor _updateModelAccessor;

        #region Public

        public IStringLocalizer T { get; set; }

        #endregion Public

        #endregion Dependencies

        #region Constructor 

        public ValidateRequiredWhenCheckedTask(
                IHttpContextAccessor httpContextAccessor,
                IStringLocalizer<ValidateMultipleTask> stringLocalizer,
                IUpdateModelAccessor updateModelAccessor
            )
        {
            _httpContextAccessor = httpContextAccessor;
            T = stringLocalizer;
            _updateModelAccessor = updateModelAccessor;
        }

        #endregion Constructor

        #region Implementation

        #region Properties

        public override LocalizedString DisplayText => T["Validate Required When Checked Task"];
        public override string Name => nameof(ValidateRequiredWhenCheckedTask);

        public override LocalizedString Category => T["Validation"];

        #endregion Properties

        #region Input

        public string CheckboxField
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string ErrorMessage
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string ToValidate
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        #endregion Input

        #region Actions
        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var outcome = OutcomeValid;
            var form = _httpContextAccessor.HttpContext?.Request?.Form;

            if (form == null)
            {
                return Outcomes(OutcomeDone, outcome);
            }

            var updater = _updateModelAccessor.ModelUpdater;

            if (!IsChecked(form))
            {
                return Outcomes(OutcomeDone, outcome);
            }

            foreach (var field in GetFieldsToValidate())
            {
                if (!string.IsNullOrWhiteSpace(form[field]))
                {
                    continue;
                }

                updater.ModelState.AddModelError(field, ErrorMessage);
                outcome = OutcomeInvalid;
            }

            return Outcomes(OutcomeDone, outcome);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T[OutcomeDone], T[OutcomeValid], T[OutcomeInvalid]);
        }

        #endregion Actions

        #endregion Implementation

        #region Private methods

        private bool IsChecked(IFormCollection form)
        {
            if (!form.ContainsKey(CheckboxField))
            {
                return false;
            }
            
            return form[CheckboxField].ToString().ToLower() == "true" || form[CheckboxField].ToString().ToLower() == "on";
        }

        private IList<string> GetFieldsToValidate()
        {
            if (string.IsNullOrWhiteSpace(ToValidate))
            {
                return new List<string>();
            }

            return ToValidate.Split(',').Select(x => x.Trim()).ToList();
        }

        #endregion Private methods
    }
}
