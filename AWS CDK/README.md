# Simple AWS CDK (TypeScript) demo — *Hello, CDK!* (Lambda + Function URL)  
I'll give you a tiny, copy-pasteable CDK project (TypeScript) that creates a Lambda and a public Function URL, and the exact commands to run it end-to-end.

> Notes: this uses **AWS CDK v2** and TypeScript. I’ll assume you have an AWS account and can run CLI commands locally.

---

## Quick references (official docs)
- Install / getting started with CDK CLI.
- CDK TypeScript project init.
- Bootstrapping (creates the CDKToolkit stack used by CDK).
- CDK CLI commands (synth / diff / deploy / destroy).
- Lambda Function URL construct (used below).

---

## Prerequisites
1. Node.js (recommend an active LTS — docs recommend Node 22.x or at least Node 18 minimum). Verify with `node -v`.  
2. AWS CLI configured (or AWS SSO); confirm you’re authenticated (e.g. `aws sts get-caller-identity`).  
3. CDK CLI installed (recommended):  
   ```bash
   npm install -g aws-cdk
   # verify:
   cdk --version
   ```  
   (If you prefer not to install globally you can use `npx aws-cdk <cmd>`).

---

## Step-by-step demo

### 1) Create the project
```bash
mkdir hello-cdk
cd hello-cdk
cdk init app --language typescript
```
This generates a TypeScript CDK scaffold (bin/, lib/, cdk.json, package.json, etc.).

If `cdk init` did not run `npm install` for any reason, run:
```bash
npm install
```

---

### 2) Replace the stack with a tiny Lambda + Function URL
Create or replace `lib/hello-cdk-stack.ts` with the code below.

```ts
// lib/hello-cdk-stack.ts
import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import * as lambda from 'aws-cdk-lib/aws-lambda';

export class HelloCdkStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // simple inline Lambda that returns "Hello, CDK!"
    const fn = new lambda.Function(this, 'HelloHandler', {
      runtime: lambda.Runtime.NODEJS_20_X,
      handler: 'index.handler',
      code: lambda.Code.fromInline(`
        exports.handler = async function(event) {
          return {
            statusCode: 200,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Hello, CDK!'
          };
        };
      `),
    });

    // add a public Function URL (no auth) and output the URL
    const fnUrl = fn.addFunctionUrl({
      authType: lambda.FunctionUrlAuthType.NONE,
    });

    new cdk.CfnOutput(this, 'FunctionUrl', { value: fnUrl.url });
  }
}
```

---

### 3) (Optional) confirm your `bin/` entry
`cdk init` created a `bin/hello-cdk.ts` (or similar). Typical contents:

```ts
#!/usr/bin/env node
import * as cdk from 'aws-cdk-lib';
import { HelloCdkStack } from '../lib/hello-cdk-stack';

const app = new cdk.App();
new HelloCdkStack(app, 'HelloCdkStack');
```

---

### 4) Bootstrap your AWS environment (only needed the first time for an account/region)
```bash
# uses your default profile/region, or specify the account/region:
# cdk bootstrap aws://ACCOUNT-NUMBER/REGION
cdk bootstrap
```

---

### 5) See what will be deployed (synth and diff)
```bash
# synthesize CloudFormation (outputs CloudFormation template to cdk.out)
cdk synth

# optional: compare local app to deployed stack
cdk diff
```

---

### 6) Deploy
```bash
cdk deploy
```

---

### 8) Clean up (remove resources)
When done, destroy the stack to avoid lingering resources:
```bash
cdk destroy
```

---
