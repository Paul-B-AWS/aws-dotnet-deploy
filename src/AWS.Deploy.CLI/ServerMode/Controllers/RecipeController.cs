// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.\r
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AWS.Deploy.CLI.ServerMode.Models;
using AWS.Deploy.Common;
using AWS.Deploy.Common.DeploymentManifest;
using AWS.Deploy.Common.IO;
using AWS.Deploy.Common.Recipes;
using AWS.Deploy.Orchestration;
using AWS.Deploy.Orchestration.Utilities;
using AWS.Deploy.Recipes;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AWS.Deploy.CLI.ServerMode.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeHandler _recipeHandler;
        private readonly IProjectDefinitionParser _projectDefinitionParser;

        public RecipeController(IRecipeHandler recipeHandler, IProjectDefinitionParser projectDefinitionParser)
        {
            _recipeHandler = recipeHandler;
            _projectDefinitionParser = projectDefinitionParser;
        }

        /// <summary>
        /// Gets a summary of the specified Recipe.
        /// </summary>
        [HttpGet("{recipeId}")]
        [SwaggerOperation(OperationId = "GetRecipe")]
        [SwaggerResponse(200, type: typeof(RecipeSummary))]
        [ProducesResponseType(Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRecipe(string recipeId, [FromQuery] string? projectPath = null)
        {
            if (string.IsNullOrEmpty(recipeId))
            {
                return BadRequest($"A Recipe ID was not provided.");
            }

            ProjectDefinition? projectDefinition = null;
            var recipePaths = new HashSet<string> { RecipeLocator.FindRecipeDefinitionsPath() };
            HashSet<string> customRecipePaths = new HashSet<string>();
            if (!string.IsNullOrEmpty(projectPath))
            {
                projectDefinition = await _projectDefinitionParser.Parse(projectPath);
                customRecipePaths = await _recipeHandler.LocateCustomRecipePaths(projectDefinition);
            }
            var recipeDefinitions = await _recipeHandler.GetRecipeDefinitions(recipeDefinitionPaths: recipePaths.Union(customRecipePaths).ToList());
            var selectedRecipeDefinition = recipeDefinitions.FirstOrDefault(x => x.Id.Equals(recipeId));

            if (selectedRecipeDefinition == null)
            {
                return BadRequest($"Recipe ID {recipeId} not found.");
            }

            var output = new RecipeSummary(
                selectedRecipeDefinition.Id,
                selectedRecipeDefinition.Version,
                selectedRecipeDefinition.Name,
                selectedRecipeDefinition.Description,
                selectedRecipeDefinition.ShortDescription,
                selectedRecipeDefinition.TargetService,
                selectedRecipeDefinition.DeploymentType.ToString(),
                selectedRecipeDefinition.DeploymentBundle.ToString()
            );

            return Ok(output);
        }
    }
}
