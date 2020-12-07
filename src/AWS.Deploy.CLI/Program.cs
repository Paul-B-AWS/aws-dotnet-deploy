// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using AWS.Deploy.CLI.Commands;
using AWS.Deploy.Orchestrator;
using AWS.DeploymentCommon;

namespace AWS.Deploy.CLI
{
    internal class Program
    {
        private static readonly IToolInteractiveService _toolInteractiveService = new ConsoleInteractiveServiceImpl();

        private static readonly Option<string> _optionProfile = new Option<string>("--profile", "AWS credential profile used to make calls to AWS");
        private static readonly Option<string> _optionRegion = new Option<string>("--region", "AWS region to deploy application to. For example us-west-2.");
        private static readonly Option<string> _optionProjectPath = new Option<string>("--project-path", getDefaultValue: () => Directory.GetCurrentDirectory(), description: "Path to the project to deploy");

        private static async Task<int> Main(string[] args)
        {
            _toolInteractiveService.WriteLine("AWS .NET Suite for deploying .NET Core applications to AWS");
            _toolInteractiveService.WriteLine("Project Home: https://github.com/aws/aws-dotnet-suite-tooling");
            _toolInteractiveService.WriteLine(string.Empty);

            var rootCommand = new RootCommand { Description = "The AWS .NET Suite for getting .NET applications running on AWS." };

            var deployCommand = new Command("deploy", "Inspect the .NET project and deploy the application to AWS to the appropriate AWS service.") { _optionProfile, _optionRegion, _optionProjectPath, new Option<bool>("--save-cdk-project", getDefaultValue: () => false, description: "Save generated CDK project in solution to customize") };
            deployCommand.Handler = CommandHandler.Create<string, string, string, bool>(async (profile, region, projectPath, saveCdkProject) =>
            {
                var awsUtilities = new AWSUtilities(_toolInteractiveService);

                var previousSettings = PreviousDeploymentSettings.ReadSettings(projectPath, null);

                var awsCredentials = awsUtilities.ResolveAWSCredentials(profile, previousSettings.Profile);
                var awsRegion = awsUtilities.ResolveAWSRegion(region, previousSettings.Region);

                var session = new OrchestratorSession(projectPath, null) { AWSProfileName = profile, AWSCredentials = awsCredentials, AWSRegion = awsRegion, ProjectDirectory = projectPath };

                await new DeployCommand(new DefaultAWSClientFactory(), _toolInteractiveService, session).ExecuteAsync(saveCdkProject);
            });
            rootCommand.Add(deployCommand);

            var setupCICDCommand = new Command("setup-cicd", "Configure the project to be deployed to AWS using the AWS Code services") { _optionProfile, _optionRegion, _optionProjectPath, };
            setupCICDCommand.Handler = CommandHandler.Create<string>(SetupCICD);
            rootCommand.Add(setupCICDCommand);

            var inspectIAMPermissionsCommand = new Command("inspect-permissions", "Inspect the project to see what AWS permissions the application needs to access AWS services the application is using.") { _optionProjectPath };
            inspectIAMPermissionsCommand.Handler = CommandHandler.Create<string>(InspectIAMPermissions);
            rootCommand.Add(inspectIAMPermissionsCommand);

            return await rootCommand.InvokeAsync(args);
        }

        private static void SetupCICD(string projectPath)
        {
            _toolInteractiveService.WriteLine("TODO: Make this work");
        }

        private static void InspectIAMPermissions(string projectPath)
        {
            _toolInteractiveService.WriteLine("TODO: Make this work");
        }
    }
}