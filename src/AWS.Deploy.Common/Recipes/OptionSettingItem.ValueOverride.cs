// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AWS.Deploy.Common.Recipes.Validation;
using Newtonsoft.Json;

namespace AWS.Deploy.Common.Recipes
{
    /// <see cref="GetValue{T}"/>, <see cref="GetValue"/> and <see cref="SetValueOverride"/> methods
    public partial class OptionSettingItem
    {
        public T? GetValue<T>(IDictionary<string, string> replacementTokens, IDictionary<string, bool>? displayableOptionSettings = null)
        {
            var value = GetValue(replacementTokens, displayableOptionSettings);

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
        }

        public object GetValue(IDictionary<string, string> replacementTokens, IDictionary<string, bool>? displayableOptionSettings = null)
        {
            if (_value != null)
            {
                return _value;
            }

            if (Type == OptionSettingValueType.Object)
            {
                var objectValue = new Dictionary<string, object>();
                foreach (var childOptionSetting in ChildOptionSettings)
                {
                    var childValue = childOptionSetting.GetValue(replacementTokens);

                    if (
                        displayableOptionSettings != null &&
                        displayableOptionSettings.TryGetValue(childOptionSetting.Id, out bool isDisplayable))
                    {
                        if (!isDisplayable)
                            continue;
                    }

                    objectValue[childOptionSetting.Id] = childValue;
                }
                return objectValue;
            }

            if (DefaultValue == null)
            {
                return string.Empty;
            }

            if (DefaultValue is string defaultValueString)
            {
                return ApplyReplacementTokens(replacementTokens, defaultValueString);
            }

            return DefaultValue;
        }

        public T? GetDefaultValue<T>(IDictionary<string, string> replacementTokens)
        {
            var value = GetDefaultValue(replacementTokens);
            if (value == null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
        }

        public object? GetDefaultValue(IDictionary<string, string> replacementTokens)
        {
            if (DefaultValue == null)
            {
                return null;
            }

            if (DefaultValue is string defaultValueString)
            {
                return ApplyReplacementTokens(replacementTokens, defaultValueString);
            }

            return DefaultValue;
        }

        /// <summary>
        /// Assigns a value to the OptionSettingItem.
        /// </summary>
        /// <param name="valueOverride">Value to assign</param>
        /// <param name="recommendation">Current deployment recommendation, may be used if the validator needs to consider properties other than itself</param>
        /// <exception cref="ValidationFailedException">
        /// Thrown if one or more <see cref="Validators"/> determine
        /// <paramref name="valueOverride"/> is not valid.
        /// </exception>
        public async Task SetValue(IOptionSettingHandler optionSettingHandler, object valueOverride, IOptionSettingItemValidator[] validators, Recommendation recommendation, bool skipValidation)
        {
            if (!skipValidation)
            {
                foreach (var validator in validators)
                {
                    var result = await validator.Validate(valueOverride, recommendation, this);
                    if (!result.IsValid)
                    {
                        Validation.ValidationStatus = ValidationStatus.Invalid;
                        Validation.ValidationMessage = result.ValidationFailedMessage?.Trim() ?? $"The value '{valueOverride}' is invalid for option setting '{Name}'.";
                        Validation.InvalidValue = valueOverride;
                        throw new ValidationFailedException(DeployToolErrorCode.OptionSettingItemValueValidationFailed, Validation.ValidationMessage);
                    }
                }
            }

            if (AllowedValues != null && AllowedValues.Count > 0 && valueOverride != null &&
                !AllowedValues.Contains(valueOverride.ToString() ?? ""))
            {
                Validation.ValidationStatus = ValidationStatus.Invalid;
                Validation.ValidationMessage = $"Invalid value for option setting item '{Name}'";
                Validation.InvalidValue = valueOverride;
                throw new InvalidOverrideValueException(DeployToolErrorCode.InvalidValueForOptionSettingItem, Validation.ValidationMessage);
            }

            Validation.ValidationStatus = ValidationStatus.Valid;
            Validation.ValidationMessage = string.Empty;
            Validation.InvalidValue = null;

            try
            {
                if (valueOverride is bool || valueOverride is int || valueOverride is long || valueOverride is double || valueOverride is Dictionary<string, string> || valueOverride is SortedSet<string>)
                {
                    _value = valueOverride;
                }
                else if (Type.Equals(OptionSettingValueType.KeyValue))
                {
                    var deserialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(valueOverride?.ToString() ?? "");
                    _value = deserialized;
                }
                else if (Type.Equals(OptionSettingValueType.List))
                {
                    var deserialized = JsonConvert.DeserializeObject<SortedSet<string>>(valueOverride?.ToString() ?? "");
                    _value = deserialized;
                }
                else if (valueOverride is string valueOverrideString)
                {
                    if (bool.TryParse(valueOverrideString, out var valueOverrideBool))
                    {
                        _value = valueOverrideBool;
                    }
                    else if (int.TryParse(valueOverrideString, out var valueOverrideInt))
                    {
                        _value = valueOverrideInt;
                    }
                    else
                    {
                        _value = valueOverrideString;
                    }
                }
                else
                {
                    var deserialized = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(valueOverride));
                    foreach (var childOptionSetting in ChildOptionSettings)
                    {
                        if (deserialized?.TryGetValue(childOptionSetting.Id, out var childValueOverride) ?? false)
                        {
                            await optionSettingHandler.SetOptionSettingValue(recommendation, childOptionSetting, childValueOverride, skipValidation: skipValidation);
                        }
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                Validation.ValidationStatus = ValidationStatus.Invalid;
                Validation.ValidationMessage = $"The value you are trying to set is invalid.";
                Validation.InvalidValue = valueOverride;
                throw new UnsupportedOptionSettingType(DeployToolErrorCode.UnsupportedOptionSettingType,
                    $"The value you are trying to set for the option setting '{Name}' is invalid.",
                    ex);
            }
        }

        private string ApplyReplacementTokens(IDictionary<string, string> replacementTokens, string defaultValue)
        {
            foreach (var token in replacementTokens)
            {
                defaultValue = defaultValue.Replace(token.Key, token.Value);
            }

            return defaultValue;
        }
    }
}
