# DynamoCode.EmailSender

![Deploy-AWS](https://github.com/DynamoCode/DynamoCode.EmailSender/workflows/Deploy-AWS/badge.svg)

This repository contains a Lambda function that is connected to an SQS queue to receive messages and send emails based on the information.

The credentials to access SMTP server are expected to stored in SSM Parameter Store. The parameter values are prefixed with ```DynamoCode.EmailSender``` which corresponds to the ```AppName``` value. It can be overridden at deploy time as a template parameter.

```
/DynamoCode.EmailSender/SmtpSettings/Server
/DynamoCode.EmailSender/SmtpSettings/Port
/DynamoCode.EmailSender/SmtpSettings/Username
/DynamoCode.EmailSender/SmtpSettings/Password
/DynamoCode.EmailSender/SmtpSettings/EnableSsl
```

## Message format

```json
{
    "FromEmail": "from@example.com", 
    "FromName": "My App Test",
    "ToEmail": "me@example.com",
    "Subject": "test from lambda SQS",
    "Body": "Text to send in an email body. <br/> Here is a <a href='www.example.com'>link</a> to test HTML"
}
```

## Prerequisites

* .NET Core 3.1 - [Install .NET Core](https://www.microsoft.com/net/download)
* SAM CLI - [Install the SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/serverless-sam-cli-install.html)

## Build 

```bash
sam build
```

## Deploy

Update file ```samconfig.toml``` accordingly and then run

```bash
sam deploy
```

Alternatively just run 

```bash
sam deploy --guided
```