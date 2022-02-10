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
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public AccountController(DataContext context, ITokenService tokenService, IUserRepository userRepository, IMapper mapper)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {

        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

        var user = _mapper.Map<AppUser>(registerDto);

        using var hmac = new HMACSHA512();

        user.UserName = registerDto.Username.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        user.PasswordSalt = hmac.Key;

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user),
            KnownAs=user.KnownAs
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _userRepository.GetUserByUsernameAsync(loginDto.Username);
        //var user = await _context.Users.SingleOrDefaultAsync<AppUser>(user => user.UserName == loginDto.Username);

        if (user == null) return Unauthorized("Invalid username");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHas = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHas.Length; i++)
        {
            if (computedHas[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
        }

        var dto = new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user),
            PhotoUrl = user.Photos?.FirstOrDefault(p => p.IsMain)?.Url
        };

        return Ok(dto);
    }

    private async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(user => user.UserName == username.ToLower());
    }

}
