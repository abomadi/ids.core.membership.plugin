using cloudscribe.Core.Identity;
using cloudscribe.Core.Models;
using cloudscribe.Core.Web.Components;
using cloudscribe.Core.Web.ViewModels.SiteUser;
using ids.core.membership.plugin.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace ids.cloudscribe.auth
{
    public class MembershipFallbackAccountService : AccountService, IAccountService
    {
        public MembershipFallbackAccountService(
            SiteUserManager<SiteUser> userManager,
            SignInManager<SiteUser> signInManager,
            IIdentityServerIntegration identityServerIntegration,
            ISocialAuthEmailVerfificationPolicy socialAuthEmailVerificationPolicy,
            IProcessAccountLoginRules loginRulesProcessor,
            IUserStore<SiteUser> userStore,
            IMembershipService membershipService,
            IMembershipClaimsService membershipClaimsService,


            ILogger<MembershipFallbackAccountService> logger
            ) : base(userManager, signInManager, identityServerIntegration, socialAuthEmailVerificationPolicy, loginRulesProcessor)
        {
            this.membershipService = membershipService;
            this.membershipClaimsService = membershipClaimsService;

            this.userStore = userStore;
            _log = logger;
        }

        private ILogger _log;
        private IUserStore<SiteUser> userStore;

        private readonly IMembershipService membershipService;
        private readonly IMembershipClaimsService membershipClaimsService;

        public override async Task<UserLoginResult> TryLogin(LoginViewModel model)
        {

            var identityResult = await base.TryLogin(model);

            if (!identityResult.SignInResult.Succeeded)
            {

                //if login failed for ASPIDENTITY, WE SHALL TRY MEMBERSHIP PROVIDER
                //this could be added in site settings to allow validate against membership provider
                //also we can generate import users scripts for fully migrations

                if (await membershipService.ValidateUser(model.Email, model.Password))
                {
                    var template = new LoginResultTemplate();
                    IUserContext userContext = null;
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
                        var result = await signInManager.UserManager.CreateAsync(newIdentityUser, model.Password);

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

                        UserLoginResult membershipLoginResult = new UserLoginResult(
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

                        identityResult = membershipLoginResult;
                    }
                }

           
            }

            return identityResult;
        }
    }
}
