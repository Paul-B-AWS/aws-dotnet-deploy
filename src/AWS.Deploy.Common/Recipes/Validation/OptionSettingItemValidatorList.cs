// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.\r
// SPDX-License-Identifier: Apache-2.0

namespace AWS.Deploy.Common.Recipes.Validation
{
    public enum OptionSettingItemValidatorList
    {
        /// <summary>
        /// Must be paired with <see cref="RangeValidator"/>
        /// </summary>
        Range,
        /// <summary>
        /// Must be paired with <see cref="RegexValidator"/>
        /// </summary>
        Regex,
        /// <summary>
        /// Must be paired with <see cref="RequiredValidator"/>
        /// </summary>
        Required,
        /// <summary>
        /// Must be paired with <see cref="DirectoryExistsValidator"/>
        /// </summary>
        DirectoryExists,
        /// <summary>
        /// Must be paired with <see cref="DockerBuildArgsValidator"/>
        /// </summary>
        DockerBuildArgs,
        /// <summary>
        /// Must be paried with <see cref="DotnetPublishArgsValidator"/>
        /// </summary>
        DotnetPublishArgs,
        /// <summary>
        /// Must be paired with <see cref="ExistingResourceValidator"/>
        /// </summary>
        ExistingResource,
        /// <summary>
        /// Must be paired with <see cref="FileExistsValidator"/>
        /// </summary>
        FileExists,
        /// <summary>
        /// Must be paired with <see cref="StringLengthValidator"/>
        /// </summary>
        StringLength,
        /// <summary>
        /// Must be paired with <see cref="LinuxInstanceTypeValidator"/>
        /// </summary>
        InstanceType,
        /// <summary>
        /// Must be paired with <see cref="WindowsInstanceTypeValidator"/>
        /// </summary>
        WindowsInstanceType,
        /// <summary>
        /// Must be paired with <see cref="SubnetsInVpcValidator"/>
        /// </summary>
        SubnetsInVpc,
        /// <summary>
        /// Must be paired with <see cref="SecurityGroupsInVpcValidator"/>
        /// </summary>
        SecurityGroupsInVpc,
        /// <summary>
        /// Must be paired with <see cref="UriValidator"/>
        /// </summary>
        Uri,
        /// <summary>
        /// Must be paired with <see cref="ComparisonValidator"/>
        /// </summary>
        Comparison,
        /// <summary>
        /// Must be paired with <see cref="VPCSubnetsInDifferentAZsValidator"/>
        /// </summary>
        VPCSubnetsInDifferentAZs
    }
}
