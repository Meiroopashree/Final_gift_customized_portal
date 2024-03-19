using dotnetapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using dotnetapp.Data;
namespace dotnetapp.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context; // Add this field
 
 
        public UserService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
 
        }
 
        public async Task<bool> RegisterAsync(User user)
        {
            try
            {
 
                var identityUser = new IdentityUser
                {
                    UserName = user.Username,
                    Email = user.Email
                };
 
                Console.WriteLine(identityUser.UserName);
                Console.WriteLine(identityUser.Email);
 
 
                var result = await _userManager.CreateAsync(identityUser, user.Password);
                Console.WriteLine("asd"+result);
 
                if (result.Succeeded)
                {
                    // Assign roles to the user
                    if (user.Role == "admin")
                    {
                        Console.WriteLine("data " + identityUser);
                        await _userManager.AddToRoleAsync(identityUser, "admin");
                    }
                    else if (user.Role == "customer")
                    {
                        Console.WriteLine("data1 " + identityUser);
 
                        await _userManager.AddToRoleAsync(identityUser, "customer");
                        Console.WriteLine("data2 " + identityUser);
 
                    }
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                // Handle exceptions appropriately (e.g., logging)
                return false; // Registration failed
            }
        }
 
 
// public async Task<string> LoginAsync(string email, string password)
//         {
//              try
//     {
//         var user = await _userManager.FindByEmailAsync(email);
//         var id = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
//        Console.WriteLine("iddd "+id.UserId);
//         Console.WriteLine("User: " + user?.Email); // Debug output
//         Console.WriteLine("Password: " + password); // Debug output
         
//         if (user == null || !(await _signInManager.CheckPasswordSignInAsync(user, password, false)).Succeeded)
//         {
//             Console.WriteLine("service");
//             Console.WriteLine("Invalid username or password"); // Debug output
//             return null; // Invalid username or password
//         }
        
//         var customer =  await _context.Customers.FirstOrDefaultAsync(c => c.UserId == 18);

//         Console.WriteLine(customer?.CustomerId+"customer-------------------------------------");



//         // Generate a JWT token
//         var token = GenerateJwtToken(user, id.UserId);
//         Console.WriteLine("Token: " + token); // Debug output
//         let cid=null;
//         if(customer?.CustomerId)
//         {
//             cid=customer.CustomerId;
//         }
 
//         return {"token":token,"CustomerId":cid}

        
//     }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("zxcvbnm" + ex.Message);
//                 // Handle exceptions appropriately (e.g., logging)
//                 return null; // Login failed
//             }
//         }
 public async Task<object> LoginAsync(string email, string password)
{
    try
    {
        Console.WriteLine(email);
        var user = await _userManager.FindByEmailAsync(email);
        var id = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        Console.WriteLine("iddd " + id.UserId);
        Console.WriteLine("User: " + user?.Email); // Debug output
        Console.WriteLine("Password: " + password); // Debug output

        if (user == null || !(await _signInManager.CheckPasswordSignInAsync(user, password, false)).Succeeded)
        {
            Console.WriteLine("service");
            Console.WriteLine("Invalid username or password"); // Debug output
            return null; // Invalid username or password
        }

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == id.UserId);

        Console.WriteLine(customer?.CustomerId + "customer-------------------------------------");

        // Generate a JWT token
    //     var token = GenerateJwtToken(user, id.UserId);
    //     Console.WriteLine("Token: " + token); // Debug output
        
    //     // Conditionally assign customer ID
    //     var cid = customer?.CustomerId != null ? customer.CustomerId.ToString() : null;
    //    int? cid2 = null; // Use nullable type for CartId
    //     if(true)
    //     {
    //       var customer1 = await _context.Cart.FirstOrDefaultAsync(c => c.CustomerId == cid);

    // cid2 = customer1?.CartId; // Assign CartId to cid2
    //     }
      

    //     // Return token and customer ID as an anonymous object
    //     return new { token, CustomerId = cid,CartId:cid2 };
    var token = GenerateJwtToken(user, id.UserId);
Console.WriteLine("Token: " + token); // Debug output

// Conditionally assign customer ID
var cid = customer?.CustomerId != null? customer.CustomerId : 0;
long? cid2 = 0; // Use nullable type for CartId
if (cid != null)
{
    var customer1 = await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == cid);
    cid2 = customer1?.CartId; // Assign CartId to cid2
}

// Return token and customer ID as an anonymous object
return new { token, CustomerId = cid, CartId = cid2 };

    }
    catch (Exception ex)
    {
        Console.WriteLine("zxcvbnm" + ex.Message);
        // Handle exceptions appropriately (e.g., logging)
        return null; // Login failed
    }
}

    private string GenerateJwtToken(IdentityUser user, long id)
{
    Console.WriteLine("User: " + user.Email);
    user = _userManager.FindByIdAsync(user.Id).Result;
 
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.NameIdentifier, id.ToString()),
        // new Claim(ClaimTypes.NameIdentifier, user.Id)
    };
 
    // Retrieve roles for the user
    var roles = _userManager.GetRolesAsync(user).Result;
 
    Console.WriteLine("Roles: " + string.Join(", ", roles));
 
    foreach (var role in roles)
{
    var roleClaim = new Claim(ClaimTypes.Role, role);
    Console.WriteLine($"Adding role claim: {roleClaim.Type} - {roleClaim.Value}");
    claims.Add(roleClaim);
}
 
    var token = new JwtSecurityToken(
        _configuration["Jwt:Issuer"],
        _configuration["Jwt:Audience"],
        claims,
        expires: DateTime.Now.AddHours(2),
        signingCredentials: credentials
    );
    Console.WriteLine("Token generated successfully: " + token);
 
    return new JwtSecurityTokenHandler().WriteToken(token);
}
 
    }
}