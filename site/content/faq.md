# Frequently Asked Questions

#### *FAQ: I have an existing .NET application. Can I use the AWS Deploy Tool to deploy it to AWS?*
Yes, the AWS Deploy Tool can be used to deploy any cloud-native .NET applications to AWS. A cloud-native .NET application is written in .NET with intent to deploy to Linux, not tied to any Windows specific technology such as Windows registry, IIS or MSMQ, and can be deployed on virtualized compute. It cannot be used to deploy .NET Framework, Desktop, Xamarin, or other applications that do not fit the “cloud-native” criteria.

#### *FAQ: How does the AWS Deploy Tool decide what service to choose for deployment?*
It examines .NET project file and project dependencies, and inspects the code for attribution and Dockerfile presence to figure out the application type and the service best suited to run the application.

#### *FAQ: How will my application be packaged and deployed?*
Once the AWS compute service is chosen, the deployment tool will package your application binary artifacts in the appropriate format for that service (for example zip file for AWS Elastic Beanstalk) and deploy it using CDK. Once the application is deployed, it will return an endpoint for the application (for example URL for API backend, or SQS queue for messaging app).

#### *FAQ: How does the AWS Deploy Tool  create infrastructure?*
It generates a .NET CDK project for the suggested service and uses the .NET CDK binding to build constructs. If you are not ready to learn CDK, it will auto-generate the default .NET CDK project behind the scenes. You can also change or extend the CDK project’s behavior to match your exact needs and then execute the deployment.

#### *FAQ: Can I re-deploy my application to a different stack?*
Yes. The AWS Deploy Tool saves your deployment settings, including the environment name (such as the name of the Cloud Formation stack) your application was deployed to. When you re-run “dotnet aws deploy” command, it will detect these previously saved settings, and ask if you want to re-deploy to the same or a different environment. If you choose the latter, it will create a new deployment stack.

#### *FAQ: Can I choose a different AWS service to deploy my application?*
The AWS Deploy Tool  will show you all compute service options available to deploy your application, and will recommend a default with information about why it was chosen. The other compute service options will be shown with an explanation of their differences. If the selected compute option does not match your need, you can select a different compute service.