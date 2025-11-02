using AutoMapper;
using TaskManager.Application.DTOs;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordService _passwordService;
        public UserService(IUserRepository userRepository, IMapper mapper, IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _passwordService = passwordService;
        }
        public async Task<UserResponseDto?> CreateUserAsync(CreateUserDto userDto)
        {
            if (await _userRepository.EmailExistsAsync(userDto.Email)) return null;

            User user = _mapper.Map<User>(userDto);
            user.PasswordHash = _passwordService.SecurePassword(userDto.Password);
            return await _userRepository.AddAsync(user) == true ? _mapper.Map<UserResponseDto>(user) : null;
        }
        public async Task<UserResponseDto?> GetUserAsync(int id)
        {
            var user = await _userRepository.GetAsync(id);
            return user == null ? null : _mapper.Map<UserResponseDto>(user);
        }
        public async Task<bool> CheckIfEmailExists(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
        }
        public async Task<string?> GetUserHashedPasswordByUsernameAsync(string username)
        {
            return await _userRepository.GetUserPasswordHashByUsernameAsync(username);
        }
        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto userDto)
        {
            var user = await _userRepository.GetAsync(id);
            if (user == null) return false;

            _mapper.Map(userDto, user);
            user.UpdatedAt = DateTime.UtcNow;
            return await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetAsync(id);
            return user == null ? false : await _userRepository.DeleteAsync(id);
        }
    }
}