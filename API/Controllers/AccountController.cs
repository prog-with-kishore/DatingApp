using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController: BaseAPIController
    {
        private DataContext _context { get; }
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;            
        }

        [HttpPost("register")] // POST: api/account/register
        public async Task<ActionResult<UserDto>> Register (RegisterDto registerDto)
        {
            if(await UserExists(registerDto.Username)) return BadRequest("UserName taken!");

            using var hmac = new HMACSHA512();
            var user = new AppUser{
                UserName = registerDto.Username,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key  
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new UserDto{UserName = user.UserName, Token=_tokenService.CreateToken(user) });
        }

        private async Task<bool> UserExists (string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName.ToLower().Equals(username.ToLower()));
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login (LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.Username);
            if (user is null) return Unauthorized("Username not found");

            var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for (int i = 0; i< user.PasswordHash.Length; i++) 
            {
                if (user.PasswordHash[i] != computedHash[i]) return Unauthorized("Invalid Password");
            }

            var userDto = new UserDto {UserName = user.UserName, Token = _tokenService.CreateToken(user) };

            return Ok(userDto);
        }
    }
}