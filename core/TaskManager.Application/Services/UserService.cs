using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task UpdateUserAsync(int id, UserDto userDto)
        {
            var user = await _userRepository.GetAsync(id);

            if (user == null)
                throw new Exception("User with given id was not found");

            ///TODO: mapper
            user.Email = userDto.Email;
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.PhoneNumber = userDto.PhoneNumber;
            user.Username = userDto.Username;

            await _userRepository.UpdateAsync(user);
        }
    }
}