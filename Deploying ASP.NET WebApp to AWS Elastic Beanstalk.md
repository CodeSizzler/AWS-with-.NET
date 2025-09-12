# Deploy an ASP.NET WebApp to Elastic Beanstalk

This QuickStart tutorial walks you through the process of creating a ASP.NET application and deploying it to an AWS Elastic Beanstalk environment.

## Your AWS account

If you're not already an AWS customer, you need to create an AWS account. Signing up enables you to access Elastic Beanstalk and other AWS services that you need.

If you already have an AWS account, you can move on to [Prerequisites](https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/aspnet-quickstart.html#aspnet-quickstart-prereq).

This QuickStart tutorial walks you through creating a "Hello World" application and deploying it to an Elastic Beanstalk environment with Visual Studio and the AWS Toolkit for Visual Studio.

### Visual Studio

To download and install Visual Studio follow the instructions on the Visual Studio [download page](https://visualstudio.microsoft.com/downloads/). This example uses Visual Studio 2022. During the Visual Studio installation select these specific items:

*   On the **Workloads** tab — select **ASP.NET and web development**.
*   On the **Individual components** tab — select **.NET Framework 4.8 development tools** and **.NET Framework project and item templates**.

### AWS Toolkit for Visual Studio

To download and set up AWS Toolkit for Visual Studio follow the instructions in the [Getting started](https://docs.aws.amazon.com/toolkit-for-visual-studio/latest/user-guide/getting-set-up.html) topic of the AWS Toolkit for Visual Studio User Guide.

## Step 1: Create a ASP.NET application

Next, create an application that you'll deploy to an Elastic Beanstalk environment. We'll create a "Hello World" ASP.NET web application.

###### To create an ASP.NET application

1.  Launch Visual Studio. In the **File** menu, select **New**, then **Project**.
2.  The **Create a new project** dialog box displays. Select **ASP.NET web application (.NET Framework)**, then select **Next**.
3.  On the **Configure your new project** dialog, enter eb-aspnet for your **Project name**. From the **Framework** dropdown menu select **.NET Framework 4.8**, then select **Create**.

Note the project directory. In this example, the project directory is C:\\Users\\Administrator\\source\\repos\\eb-aspnet\\eb-aspnet.

1.  The **Create a new ASP.NET Web Application** dialogue displays. Select the **Empty** template. Next select **Create**.

At this point, you have created an empty ASP.NET web application project using Visual Studio. Next, we'll create a web form that will serve as the entry point for the ASP.NET web application.

1.  From the **Project** menu, select **Add New Item**. On the **Add New Item** page, select **Web Form** and name it Default.aspx. Next select **Add**.
2.  Modify the code at Default.aspx

## Step 2: Run your application locally

In Visual Studio, from the **Debug** menu select **Start Debugging** to run your application locally. The page should display "Hello Elastic Beanstalk! This is an ASP.NET on Windows Server application."

## Step 3: Deploy your ASP.NET application with the AWS Toolkit for Visual Studio

Follow these steps to create an Elastic Beanstalk environment and deploy your new application to it.

###### To create an environment and deploy your ASP.NET application

1.  In **Solution Explorer**, right-click your application, then select **Publish to AWS Elastic Beanstalk**.
2.  Choose a name for your new Elastic Beanstalk application and environment.
3.  Beyond this point, you may proceed with the defaults provided by Elastic Beanstalk or modify any of the options and settings to your liking.
4.  On the **Review** page, select **Deploy**. This will package your ASP.NET web application and deploy it to Elastic Beanstalk.

It takes about five minutes for Elastic Beanstalk to create your environment. The Elastic Beanstalk deployment feature will monitor your environment until it becomes available with the newly deployed code. On the **Env:<environment name>** tab, you'll see the status for your environment.

## Step 4: Run your application on Elastic Beanstalk

When the process to create your environment completes, the **Env:<environment name>** tab, displays information about your environment and application, including the domain URL to launch your application. Select this URL on this tab or copy and paste it into your web browser.

Congratulations! You've deployed a ASP.NET application with Elastic Beanstalk!

## Step 5: Clean up

When you finish working with your application, you can terminate your environment in the AWS Toolkit for Visual Studio.

###### To terminate your environment

1.  Expand the Elastic Beanstalk node and the application node in **AWS Explorer**. Right-click your application environment and select **Terminate Environment**.
2.  When prompted, select **Yes** to confirm that you want to terminate the environment. It will take a few minutes for Elastic Beanstalk to terminate the AWS resources running in the environment.
