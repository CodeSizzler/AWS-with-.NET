# Develop & Host a .NET Web App on EC2 Linux

### Objective

To develop a simple [ASP.NET](https://asp.net/) Core Web API application on a local machine and successfully deploy and host it on an AWS EC2 instance running Amazon Linux 2023. This end-to-end procedure will cover creating the infrastructure (EC2 instance), installing the necessary runtime, developing the application, and deploying it to make it accessible over the internet.

#### Phase 1: Create and Configure an EC2 Instance

1.  **Navigate to EC2 in AWS Console:**
    *   Log in to the AWS Management Console.
    *   Search for and select **EC2** from the services menu.
2.  **Launch an Instance:**
    *   On the EC2 Dashboard, click the **"Launch Instances"** button.
3.  **Name Your Instance:**
    *   Enter a descriptive name for your instance (e.g., cds-demo-ec2).
4.  **Choose an Amazon Machine Image (AMI):**
    *   Under "Application and OS Images," select a **Quick Start** AMI.
    *   Choose **"Amazon Linux 2023"** (e.g., ami-0861f4e788f5069dd), a suitable, low-cost Linux distribution.
5.  **Choose an Instance Type:**
    *   Select the t3.micro instance type as it is **"Free tier eligible"** and sufficient for a demo.
6.  **Create a Key Pair:**
    *   In the "Key pair" section, click **"Create new key pair"**.
    *   Name your key pair (e.g., cds-demo-ec2-kp).
    *   Choose the .pem key format.
    *   Click **"Create key pair"** and securely store the downloaded .pem file.
7.  **Configure Network Settings:**
    *   Under "Firewall (security groups)", select **"Allow HTTPS traffic"** and **"Allow HTTP traffic"** from the internet. This creates firewall rules for web traffic (ports 80 and 443).
8.  **Configure Storage:**
    *   Retain the default storage (8 GiB of gp3).
9.  **Launch the Instance:**
    *   Review the summary and click **"Launch instance"**.
    *   Wait for the instance's state to change to running.

#### Phase 2: Connect to the EC2 Instance via SSH

1.  **Locate Connection Details:**
    *   In the EC2 console, select your instance and note its **"Public IPv4 DNS"** (e.g., ec2-3-110-161-197.ap-south-1.compute.amazonaws.com).
2.  **Connect Using SSH:**
    *   On your local machine, open a terminal.
    *   Navigate to the directory containing your .pem key file.
    *   Secure the key file permissions:

bash

chmod 400 cds-demo-ec2-kp.pem

*   *   Connect to the instance (the username for Amazon Linux is ec2-user):

bash

ssh -i "cds-demo-ec2-kp.pem" ec2-user@<Your-Public-DNS>

#### Phase 3: Install the .NET Runtime on the EC2 Instance

1.  **Download the .NET Runtime:**
    *   In your SSH session, download the latest Linux [ASP.NET](https://asp.net/) Core Runtime binaries using wget. Get the URL from the [official .NET downloads page](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

bash

wget [https://download.visualstudio.microsoft.com/download/pr/<unique-id>/aspnetcore-runtime-8.0.19-linux-x64.tar.gz](https://download.visualstudio.microsoft.com/download/pr/%3cunique-id%3e/aspnetcore-runtime-8.0.19-linux-x64.tar.gz)

1.  **Install the Runtime:**
    *   Create a directory for .NET and extract the downloaded file into it.

bash

mkdir -p $HOME/dotnet

tar zxf aspnetcore-runtime-8.0.19-linux-x64.tar.gz -C $HOME/dotnet

1.  **Set Permanent Environment Variables:**
    *   Add the paths to your shell profile to make the dotnet command available permanently.

bash

echo 'export DOTNET\_ROOT=$HOME/dotnet' >> ~/.bash\_profile

echo 'export PATH=$PATH:$HOME/dotnet' >> ~/.bash\_profile

source ~/.bash\_profile

1.  **Verify the Installation:**
    *   Confirm the runtime is installed correctly.

bash

dotnet --list-runtimes

#### Phase 4: Develop and Publish the Application Locally

1.  **Create a New .NET Web App (Local Machine):**
    *   On your local machine, open a terminal and create a new project.

bash

dotnet new webapi -n hello-world-ec2

cd hello-world-ec2

1.  **Publish the Application:**
    *   Publish the application for the Linux environment.

bash

dotnet publish -c Release -o ./publish --os linux

### Final Outcome

A fully functional [ASP.NET](https://asp.net/) Core Web API application running on an AWS EC2 Linux instance, accessible to anyone on the internet via its public IP address and port 5000. The foundational infrastructure and deployment pipeline are now complete.
