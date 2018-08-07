﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Options used to configure behavior for types annotated with <see cref="ApiControllerAttribute"/>.
    /// </summary>
    public class ApiBehaviorOptions : IEnumerable<ICompatibilitySwitch>
    {
        private readonly CompatibilitySwitch<bool> _allowUseProblemDetailsForClientErrorResponses;
        private readonly ICompatibilitySwitch[] _switches;

        private Func<ActionContext, IActionResult> _invalidModelStateResponseFactory;

        /// <summary>
        /// Creates a new instance of <see cref="ApiBehaviorOptions"/>.
        /// </summary>
        public ApiBehaviorOptions()
        {
            _allowUseProblemDetailsForClientErrorResponses = new CompatibilitySwitch<bool>(nameof(AllowUseProblemDetailsForClientErrorResponses));
            _switches = new[]
            {
                _allowUseProblemDetailsForClientErrorResponses,
            };
        }

        /// <summary>
        /// Delegate invoked on actions annotated with <see cref="ApiControllerAttribute"/> to convert invalid
        /// <see cref="ModelStateDictionary"/> into an <see cref="IActionResult"/>
        /// <para>
        /// By default, the delegate produces a <see cref="BadRequestObjectResult"/> that wraps a serialized form
        /// of <see cref="ModelStateDictionary"/>.
        /// </para>
        /// </summary>
        public Func<ActionContext, IActionResult> InvalidModelStateResponseFactory
        {
            get => _invalidModelStateResponseFactory;
            set => _invalidModelStateResponseFactory = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets a value that determines if the filter that returns an <see cref="BadRequestObjectResult"/> when
        /// <see cref="ActionContext.ModelState"/> is invalid is suppressed. <seealso cref="InvalidModelStateResponseFactory"/>.
        /// </summary>
        public bool SuppressModelStateInvalidFilter { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if model binding sources are inferred for action parameters on controllers annotated
        /// with <see cref="ApiControllerAttribute"/> is suppressed.
        /// <para>
        /// When enabled, the following sources are inferred:
        /// Parameters that appear as route values, are assumed to be bound from the path (<see cref="BindingSource.Path"/>).
        /// Parameters of type <see cref="IFormFile"/> and <see cref="IFormFileCollection"/> are assumed to be bound from form.
        /// Parameters that are complex (<see cref="ModelMetadata.IsComplexType"/>) are assumed to be bound from the body (<see cref="BindingSource.Body"/>).
        /// All other parameters are assumed to be bound from the query.
        /// </para>
        /// </summary>
        public bool SuppressInferBindingSourcesForParameters { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if an <c>multipart/form-data</c> consumes action constraint is added to parameters
        /// that are bound from form data.
        /// </summary>
        public bool SuppressConsumesConstraintForFormFileParameters { get; set; }


        public bool AllowUseProblemDetailsForClientErrorResponses
        {
            get => _allowUseProblemDetailsForClientErrorResponses.Value;
            set => _allowUseProblemDetailsForClientErrorResponses.Value = value;
        }

        public IDictionary<int, Func<ActionContext, ProblemDetails>> ProblemDetailsFactory { get; } = new Dictionary<int, Func<ActionContext, ProblemDetails>>
        {
            [400] = _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Status = 400,
                Detail = "Unable to process the request due to a client error.",
            },

            [401] = _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Status = 401,
                Detail = "Authentication is required and has failed or has not yet been provided.",
            },

            [404] = _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Status = 404,
                Detail = "The server has not found anything matching the Request-URI",
            },
        };

        IEnumerator<ICompatibilitySwitch> IEnumerable<ICompatibilitySwitch>.GetEnumerator()
        {
            return ((IEnumerable<ICompatibilitySwitch>)_switches).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => _switches.GetEnumerator();
    }
}
