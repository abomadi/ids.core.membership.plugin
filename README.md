﻿Identityserver 4 Membership Provider
===================


This repository is based on 
https://github.com/MacLeanElectrical/identityserver-contrib-membership/tree/master/samples/IdentityServer4.Contrib.Membership.IdsvrDemo

The only difference that this project is modified to be working on .Net Core 2

This plugin is for Identityserver4 to provide authenticate for your old membership .NET application.

it works for DotNetNuke, Webforms users also i will write below how to use it in details for DNN and one of the best .NET SaaS Platform [CloudScribe] by our friend @ joeaudette (https://www.cloudscribe.com/) online for .NET

----------


How to use with CloudScribe
-------------

You can start by downloading cloudscribe template generator
https://marketplace.visualstudio.com/items?itemName=joeaudette.cloudscribeProjectTemplate

 - Install the CloudScribe Template for Visual Studio
 - Create a new CloudScribe project and check the Include IdentityServer4 Integration
 - Add reference for ids.cloudscribe.auth.dll and ids.core.membership.plugin.dll to your project
 - Open your Startup.cs class file and add this Membership provider on your project startup service collection 

    .AddMembershipService(new MembershipOptions
    {ConnectionString = "Data Source=localhost-source;Initial Catalog=dbschema;User ID=dbuser;Password=password",ApplicationName = "aspnetapplicationname"});   

 - Setup your connection string to your membership store.
 -Add the following line in your Startup.cs

    services.AddScoped<IAccountService, MembershipFallbackAccountService>();

Now every user try to login it will validate against cloudscribe ASP.NET Identity system if failed it will try membership and create a new user with the same password in asp.net identity system so next time user will be validated from ASP.NET Identity system.

> **Note:**
> - Give attention to ASP.NET identity password requirments it should match the old membership one rules.
> - It's not recommended to maintain new users into membership provider. this just helps you to migrate your active user into your new authentication provider.
> - I will be providing a script to fully migrate old membership users to ASP.NET Identity


How to use with DotNetNuke
-------------
Coming Soon