// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using GalleryCart.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using GalleryCart.Models.Models;
using GalleryCart.Utilities.Constants;
using Newtonsoft.Json;

namespace GalleryCart.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly IUserRepository  _userRepository;

        public ExternalLoginModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IUserStore<User> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender, 
            IUserRepository userRepository)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            _userRepository = userRepository;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            
            [Required]
            [RegularExpression(@"^[a-zA-Z0-9\-._@+àáảãạâầấẩẫậăằắẳẵặèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđ() ]+$",
                ErrorMessage = "Username must only contain numbers, letters, or the following special characters: \"-._@+()\"")]
            public string UserName { get; set; }
            
            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Date of Birth")]
            [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
            public DateOnly? UserDOB { get; set; } = null;
            
            [Display(Name = "Is Artist")]
            public bool IsArtist { get; set; }  // Note: fix typo if needed to IsArtist

            [Display(Name = "Profession Summary")]
            public string ProfessionSummary { get; set; } = string.Empty;

            [Display(Name = "Skills")]
            public string Skills { get; set; } = string.Empty;

            [Display(Name = "Software")]
            public string Software { get; set; } = string.Empty;

            [Display(Name = "Contact Info")]
            public string ContactInfo { get; set; } = string.Empty;

            [Display(Name = "Is Jobless")]
            public bool IsJobLess { get; set; } = true;

            [Display(Name = "Accept Commission? (You can change it whenever you want)")]
            public bool CommissionStatus { get; set; } = false;
        }
        
        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                HttpContext.Session.SetString("CurrentUser", JsonConvert.SerializeObject(user));
                // Redirect based on user role
                if (await _userManager.IsInRoleAsync(user, RoleConstants.Admin))
                {
                    returnUrl = Url.Action("Index", "Dashboard", new { area = "Admin" });
                }
                else if (await _userManager.IsInRoleAsync(user, RoleConstants.User))
                {
                    returnUrl = Url.Page("/Home/Index", new { area = "Customer" });

                }
                else
                {
                    returnUrl = Url.Page("/Home/Index", new { area = "Artist" });
                }   
                
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                var userEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
                var existingUser = await _userRepository.GetAsync(u => u.Email == userEmail);
                if (existingUser != null)
                {
                    if (existingUser.IsBanned)
                    {
                        ModelState.AddModelError("", "User account is banned!");
                        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                    }
                    var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(existingUser);

                    // Make the account confirmed if not confirmed before:
                    if (!isEmailConfirmed)
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(existingUser);
                        await _userManager.ConfirmEmailAsync(existingUser, token);
                    }
                    HttpContext.Session.SetString("CurrentUser", JsonConvert.SerializeObject(existingUser));
                    _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name,
                        info.LoginProvider);
                    await _signInManager.SignInAsync(existingUser, isPersistent: false, authenticationMethod: null);
                    // Redirect based on user role
                    if (await _userManager.IsInRoleAsync(existingUser, RoleConstants.Admin))
                    {
                        returnUrl = Url.Action("Index", "Dashboard", new { area = "Admin" });
                    }
                    else if (await _userManager.IsInRoleAsync(existingUser, RoleConstants.User))
                    {
                        returnUrl = Url.Page("/Home/Index", new { area = "Customer" });

                    }
                    else
                    {
                        returnUrl = Url.Page("/Home/Index", new { area = "Artist" });
                    }   
                    return Redirect(returnUrl);
                }
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    var userName  = info.Principal.FindFirstValue(ClaimTypes.Name);
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                        UserName = info.Principal.FindFirstValue(ClaimTypes.Name),
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        // Log the user login event
                        _logger.LogInformation("User {Email} logged in at {Time}.", Input.Email, DateTime.UtcNow);

                        // Redirect based on user role
                        if (await _userManager.IsInRoleAsync(user, RoleConstants.Admin))
                        {
                            returnUrl = Url.Action("Index", "Dashboard", new { area = "Admin" });
                        }
                        else if (await _userManager.IsInRoleAsync(user, RoleConstants.User))
                        {
                            returnUrl = Url.Page("/Home/Index", new { area = "Customer" });

                        }
                        else
                        {
                            returnUrl = Url.Page("/Home/Index", new { area = "Artist" });
                        }              
                        
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private User CreateUser()
        {
            var registerUser = new User();
            registerUser.UserDOB = Input.UserDOB;
            registerUser.UserAvatar = GeneralConstants.DefaultAvatar;
            registerUser.IsArtits = Input.IsArtist;
            if (Input.IsArtist)
            {
                registerUser.ProfessionSummary = Input.ProfessionSummary;
                registerUser.Skills = Input.Skills;
                registerUser.Software = Input.Software;
                registerUser.ContactInfo = Input.ContactInfo;
                registerUser.IsJobLess = Input.IsJobLess;
                registerUser.CommissionStatus = Input.CommissionStatus;
            }
            return registerUser;
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}
