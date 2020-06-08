using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ord.Accounts.Models.Users;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Net.Http;
using Ord.Accounts.Services;
using IdentityServer4;

namespace Ord.Accounts.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<AccountsUser> _signInManager;
        private readonly UserManager<AccountsUser> _userManager;
        private readonly IUrlHelper _urlHelper;
        private readonly ISmsVerificationService _smsVerify;


    private string _idpUrl { get { return "http://localhost:5000/connect/token"; } }

        public AccountController(SignInManager<AccountsUser> signInManager,
            UserManager<AccountsUser> userManager,
            IUrlHelper urlHelper,
            ISmsVerificationService smsVerificationService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _urlHelper = urlHelper;
            _smsVerify = smsVerificationService;
        }

        [HttpPost]
        [Route("createaccount", Name = "CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromBody] UserLogin userLogin)
        {
            try
            {
                var existingUser = await _userManager.FindByNameAsync(userLogin.UserName);
                if (existingUser != null)
                    return Conflict(new { Error = $"The number '{userLogin.UserName}' is already in use." });

                var user = new AccountsUser { UserName = userLogin.UserName, PhoneNumber = userLogin.UserName };
                var result = await _userManager.CreateAsync(user, userLogin.Password);
                if (result.Succeeded)
                {
		            var userMod = new
                    {
                        user.Id,
                        user.UserName,
                        user.NormalizedUserName,
                        user.Email,
                        user.NormalizedEmail,
                        user.EmailConfirmed,
                        user.PhoneNumber,
                        user.PhoneNumberConfirmed
                    };			

                    var loginResult = await _signInManager.PasswordSignInAsync(userLogin.UserName,
                    userLogin.Password, isPersistent: false, lockoutOnFailure: false);

                    if (loginResult.Succeeded)
                    {
                        var newUser = await _signInManager.UserManager.FindByNameAsync(userLogin.UserName);
                        var request = _urlHelper.ActionContext.HttpContext.Request;
                        var baseAddress = new Uri(request.Scheme + "://" + request.Host.Value).ToString();
                        var tokenRequest = new TokenRequest
                        {
                            ClientId = "orderbuddy_password",
                            ClientSecret = "7baeb4e4",
                            GrantType = GrantType.ResourceOwnerPassword,
                            Parameters = new Dictionary<string, string>
                            {
                                { "username", userLogin.UserName },
                                { "password", userLogin.Password },
                                { "scope", "openid profile orderbuddyapi offline_access" }
                            },
                            Address = _idpUrl
                        };

                        var token = await new HttpClient().RequestTokenAsync(tokenRequest);
                        var authToken = new
                        {
                            token.AccessToken,
                            token.RefreshToken,
                            token.ExpiresIn
                        };

                        return Ok(new
                        {
                            Account = userMod,
                            Token = authToken
                        });
                    }
                }
                return BadRequest(new { Error = result.Errors });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("login", Name = "Login")]
        public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
        {
            var result = await _signInManager.PasswordSignInAsync(userLogin.UserName, 
                userLogin.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _signInManager.UserManager.FindByNameAsync(userLogin.UserName);
		
		        var userMod = new
                {
                    user.Id,
                    user.UserName,
                    user.NormalizedUserName,
                    user.Email,
                    user.NormalizedEmail,
                    user.EmailConfirmed,
                    user.PhoneNumber,
                    user.PhoneNumberConfirmed
                };

                try
                {
                    var request = _urlHelper.ActionContext.HttpContext.Request;
                    var baseAddress = new Uri(request.Scheme + "://" + request.Host.Value).ToString();
                    var tokenRequest = new TokenRequest
                    {
                        ClientId = "orderbuddy_password",
                        ClientSecret = "7baeb4e4",
                        GrantType = GrantType.ResourceOwnerPassword,
                        Parameters = new Dictionary<string, string>
                        {
                            { "username", userLogin.UserName },
                            { "password", userLogin.Password },
                            { "scope", "openid profile orderbuddyapi offline_access" }
                        },
                        Address = _idpUrl
                    };

                    var token = await new HttpClient().RequestTokenAsync(tokenRequest);
                    var authToken = new
                    {
                        token.AccessToken,
                        token.RefreshToken,
                        token.ExpiresIn
                    };

                    return Ok(new
                    {
                        Account = userMod,
                        Token = authToken
                    });
                }
                catch(Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return BadRequest(new { Error = $"Login failed." });
        }

        [HttpGet]
        [Route("sendverificationtoken/{userId}", Name = "sendverificationtoken")]
        public async Task<IActionResult> SendVerificationToken(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { Error = $"A user with Id '{ userId }' could not be found." });

            var token = await _userManager.GenerateUserTokenAsync(user, "PhoneNumberConfirmation", "PhoneNumberConfirmation");

            if (token != null)
            {
                // Send token/code to the user via SMS here.
                if (await _smsVerify.SendVerificationCode(user.PhoneNumber, token))
                    //return Ok(new { Token = token });
                    return Ok();
                //if (await _smsVerify.SendVerificationCode("0767034387", token))
                    //return Ok(new { Token = token });
            }

            return BadRequest(new { Error = $"Could not confirm number '{ user.UserName }" });
        }

        [HttpPost]
        [Route("verifyToken/{userId}", Name = "verifyToken")]
        public async Task<IActionResult> VerifyToken(string userId, [FromBody] VerificationToken vToken)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { Error = $"A user with Id '{ userId }' could not be found." });

            var tokenIsValid = await _userManager.VerifyUserTokenAsync(user, "PhoneNumberConfirmation", 
                "PhoneNumberConfirmation", vToken.Token);

            if (tokenIsValid)
            {
                // should only happen after proper verification code has been supplied.
                user.PhoneNumberConfirmed = true;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                    return Ok(new { Result = "success", User = user });
            }

            return BadRequest(new { Result = "failed" });
        }
    }
}
