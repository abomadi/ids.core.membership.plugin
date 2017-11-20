Identityserver 4 Membership Provider
===================


This repository is based on 
https://github.com/MacLeanElectrical/identityserver-contrib-membership/tree/master/samples/IdentityServer4.Contrib.Membership.IdsvrDemo

The only difference that this project is modified to be working on .Net Core 2

This plugin is for Identityserver4 to provide authenticate your old membership .NET application using your IDS4

it works for DotNetNuke users also i will write below how to use it in details for DNN and one of the best .NET SaaS Platform [CloudScribe] by our friend @ joeaudette (https://www.cloudscribe.com/) online for .NET

----------


How to use with CloudScribe
-------------

 git clone https://github.com/joeaudette/cloudscribe.git
 git clone https://github.com/abomadi/ids.core.membership.plugin.git

    using ids.core.membership.plugin;

 Modify sourceDev.WebApp statup.cs class
 Add this Membership provider on your project startup service collection 

     .AddMembershipService(new MembershipOptions
    {ConnectionString = "Data Source=localhost-source;Initial Catalog=dbschema;User ID=dbuser;Password=password",ApplicationName = "aspnetapplicationname"});   

Now you are ready to validate your old users, now we want to handle how old users are migrated to cloudscribe asp.net identity

We will be modifying cloudscribe.Core.Web/Components/AccountService.cs
Update method TryLogin to the following
 

    public virtual async Task<UserLoginResult> TryLogin(LoginViewModel model)
            {
                var template = new LoginResultTemplate();
                IUserContext userContext = null;
               
                if(userManager.Site.UseEmailForLogin)
                {
                    template.User = await userManager.FindByNameAsync(model.Email);
                }
                else
                {
                    template.User = await userManager.FindByNameAsync(model.UserName);
                }
                
                if (template.User != null)
                {
                    await loginRulesProcessor.ProcessAccountLoginRules(template);
                }
    
                if(template.User != null)
                {
                    userContext = new UserContext(template.User);
                }
               
                if(userContext != null 
                    && template.SignInResult == SignInResult.Failed 
                    &&  template.RejectReasons.Count == 0)
                {
                    var persistent = false;
                    if (userManager.Site.AllowPersistentLogin)
                    {
                        persistent = model.RememberMe;
                    }
    
                    if (userManager.Site.UseEmailForLogin)
                    {
                        template.SignInResult = await signInManager.PasswordSignInAsync(
                            model.Email,
                            model.Password,
                            persistent,
                            lockoutOnFailure: false);
                       
    
                    }
                    else
                    {
                        template.SignInResult = await signInManager.PasswordSignInAsync(
                            model.UserName,
                            model.Password,
                            persistent,
                            lockoutOnFailure: false);
                    }
    
                    if(template.SignInResult.Succeeded)
                    {
                        //update last login time
                        template.User.LastLoginUtc = DateTime.UtcNow;
                        await userManager.UpdateAsync(template.User);
                    }
                }
    
                         
                if (template.SignInResult == SignInResult.Failed)
                {
                    if (await membershipService.ValidateUser(model.Email, model.Password))
                    {
                        // issue authentication cookie with subject ID and username
                        var user = await membershipService.GetUserAsync(model.Email);
                       // var claims = await membershipClaimsService.GetClaimsFromAccount(user);
                        //if we have a user we will import it's information to the new identity and signin him
                        if (user.Email != null)
                        {
                            SiteUser newIdentityUser = new SiteUser();
                            newIdentityUser.Email = user.Email;
                            newIdentityUser.UserName = user.UserName;
                            newIdentityUser.Id = user.UserId;
                            newIdentityUser.LastPasswordChangeUtc = user.PasswordChanged;
                            newIdentityUser.LastModifiedUtc = user.LastActivity;
                            newIdentityUser.IsLockedOut = user.IsLockedOut;
                            newIdentityUser.AccountApproved = user.IsApproved;
                            newIdentityUser.AgreementAcceptedUtc = user.AccountCreated;
                            newIdentityUser.CreatedUtc = user.AccountCreated;
                            newIdentityUser.SiteId = userManager.Site.Id;
                            newIdentityUser.FirstName = "test";
                            newIdentityUser.LastName = "test";
                            newIdentityUser.LastLoginUtc = DateTime.UtcNow;
                            newIdentityUser.DisplayName = user.UserName;
                            newIdentityUser.AccountApproved = userManager.Site.RequireApprovalBeforeLogin ? false : true;
    
    
                            //create user manualy 
                            var result =  await signInManager.UserManager.CreateAsync(newIdentityUser, model.Password);
    
                            if (result.Succeeded)
                            {
                                template.User = newIdentityUser;
                                template.IsNewUserRegistration = true;
                                await loginRulesProcessor.ProcessAccountLoginRules(template);
                            }
                            //Login user 
                            template.SignInResult = await signInManager.PasswordSignInAsync(
                               model.Email,
                               model.Password,
                               false,
                               lockoutOnFailure: false);
                        }
    
                        if (template.SignInResult == SignInResult.Success)
                        {
                            //await signInManager.SignInAsync(user, isPersistent: false);
                            //template.SignInResult = SignInResult.Success;
    
                            if (template.User != null)
                            {
                                userContext = new UserContext(template.User);
                            }
                        }
                    }
                }
    
                return new UserLoginResult(
                    template.SignInResult, 
                    template.RejectReasons, 
                    userContext,
                    template.IsNewUserRegistration,
                    template.MustAcceptTerms,
                    template.NeedsAccountApproval,
                    template.NeedsEmailConfirmation,
                    template.EmailConfirmationToken,
                    template.NeedsPhoneConfirmation
                    );
            }

Now every user try to login it will validate against ASP.NET Identity system if failed it will try membership and create a new user with the same password in asp.net identity system so next time user will be validated from ASP.NET Identity system.

> **Note:**

> - It's not recommended to maintain new users into membership provider. this just helps you to migrate your active user into your new authentication provider.
> - I will be providing a script to fully migrate old membership users to ASP.NET Identity
