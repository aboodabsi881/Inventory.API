using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Carts;
using Inventory.Core.Entities.Users.ApplicationRoles;
using Inventory.Core.Entities.Users.ApplicationUsers;
using Inventory.Core.Interfaces;
using Inventory.Data.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Inventory.Data.Services
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _context;
        protected readonly IMapper _mapper;

        public Repository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region Generic CRUD & DTO Operations

        public async Task<IReadOnlyList<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking();
            if (include != null) query = include(query);
            return await query.ToListAsync();
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking();
            if (include != null) query = include(query);
            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }

        public void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<TResponseDto>> GetAllDtoAsync<TResponseDto>(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null) where TResponseDto : class
        {
            var entities = await GetAllAsync(include);
            return _mapper.Map<IReadOnlyList<TResponseDto>>(entities);
        }

        public async Task<TResponseDto?> GetDtoByIdAsync<TResponseDto>(int id, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null) where TResponseDto : class
        {
            IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking();
            if (include != null) query = include(query);

            var entity = await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
            if (entity == null) return null;

            return _mapper.Map<TResponseDto>(entity);
        }

        public async Task<TResponseDto> CreateFromDtoAsync<TRequestDto, TResponseDto>(TRequestDto dto)
            where TRequestDto : class
            where TResponseDto : class
        {
            var entity = _mapper.Map<TEntity>(dto);
            await AddAsync(entity);
            await SaveChangesAsync();
            return _mapper.Map<TResponseDto>(entity);
        }

        public async Task<TResponseDto> UpdateFromDtoAsync<TRequestDto, TResponseDto>(int id, TRequestDto dto)
            where TRequestDto : class
            where TResponseDto : class
        {
            var existingEntity = await GetByIdAsync(id);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Item with ID {id} was not found.");

            _mapper.Map(dto, existingEntity);
            _context.Entry(existingEntity).State = EntityState.Modified;
            await SaveChangesAsync();

            return _mapper.Map<TResponseDto>(existingEntity);
        }

        public async Task<bool> DeleteAndSaveAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Item with ID {id} was not found.");

            Delete(entity);
            var result = await SaveChangesAsync();
            return result > 0;
        }

        #endregion

        #region Account Logic (Exact Previous Working Logic)

        public async Task<UserResponseDto?> SignInUserAsync(LoginRequestDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Username || u.UserName == model.Username);
            if (user == null) return null;

            var dto = await GetDtoByIdAsync<UserResponseDto>(user.Id);
            if (dto != null)
            {
                var roles = await GetRolesForUserAsync(user.Id);
                dto.Roles = roles;
                dto.RoleName = roles.FirstOrDefault() ?? "User";
            }

            return dto;
        }

        public async Task<UserResponseDto> CreateUserFromDtoAsync(RegisterRequestDto model)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == model.Email || (!string.IsNullOrEmpty(model.Username) && u.UserName == model.Username));

            if (existingUser != null)
            {
                if (existingUser.Email == model.Email)
                    throw new InvalidOperationException($"The email '{model.Email}' is already in use.");

                throw new InvalidOperationException($"The username '{model.Username}' is already taken.");
            }

            var dto = await CreateFromDtoAsync<RegisterRequestDto, UserResponseDto>(model);

            var roleName = string.IsNullOrEmpty(model.RoleName) ? "User" : model.RoleName;
            await AddUserToRoleByNameAsync(dto.Id, roleName);

            dto.Roles = new List<string> { roleName };
            dto.RoleName = roleName;
            return dto;
        }

        public async Task<PersonalDataResponseDto> GetPersonalDataDtoAsync(int id)
        {
            var dto = await GetDtoByIdAsync<PersonalDataResponseDto>(id);
            if (dto == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            var roles = await GetRolesForUserAsync(id);
            dto.RoleName = roles.FirstOrDefault() ?? "User";

            return dto;
        }

        public async Task<PersonalDataResponseDto> UpdatePersonalDataFromDtoAsync(int id, PersonalDataRequestDto model)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            _mapper.Map(model, existingUser);

            if (!string.IsNullOrWhiteSpace(model.Username))
            {
                existingUser.UserName = model.Username;
                existingUser.NormalizedUserName = model.Username.ToUpperInvariant();
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                existingUser.Email = model.Email;
                existingUser.NormalizedEmail = model.Email.ToUpperInvariant();
            }

            _context.Entry(existingUser).State = EntityState.Modified;
            await SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(model.RoleName))
            {
                var existingUserRoles = await _context.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
                _context.UserRoles.RemoveRange(existingUserRoles);
                await SaveChangesAsync();

                await AddUserToRoleByNameAsync(id, model.RoleName);
            }

            var dto = _mapper.Map<PersonalDataResponseDto>(existingUser);

            dto.Username = existingUser.UserName ?? string.Empty;

            var roles = await GetRolesForUserAsync(id);
            dto.RoleName = roles.FirstOrDefault() ?? (model.RoleName ?? "User");

            return dto;
        }

        public async Task<IReadOnlyList<UserResponseDto>> GetAllUsersWithRolesAsync()
        {
            var users = await GetAllDtoAsync<UserResponseDto>();
            foreach (var user in users)
            {
                var roles = await GetRolesForUserAsync(user.Id);
                user.Roles = roles;
                user.RoleName = roles.FirstOrDefault() ?? "User";
            }
            return users;
        }

        public async Task<UserResponseDto> GetUserWithRolesByIdAsync(int id)
        {
            var dto = await GetDtoByIdAsync<UserResponseDto>(id);
            if (dto == null)
                throw new KeyNotFoundException($"User with ID {id} was not found.");

            var roles = await GetRolesForUserAsync(id);
            dto.Roles = roles;
            dto.RoleName = roles.FirstOrDefault() ?? "User";
            return dto;
        }

        public async Task<UserResponseDto> GetUserWithRolesByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new KeyNotFoundException($"User with email '{email}' was not found.");

            var dto = await GetDtoByIdAsync<UserResponseDto>(user.Id);
            if (dto != null)
            {
                var roles = await GetRolesForUserAsync(user.Id);
                dto.Roles = roles;
                dto.RoleName = roles.FirstOrDefault() ?? "User";
            }
            return dto!;
        }

        public async Task<bool> AddUserToRoleAsync(int userId, string roleName)
        {
            return await AddUserToRoleByNameAsync(userId, roleName);
        }

        public async Task<bool> ChangePasswordAsync(int id, ChangePasswordRequestDto model)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            return await Task.FromResult(true);
        }

        private async Task<List<string>> GetRolesForUserAsync(int userId)
        {
            var userRoleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var roles = await _context.Roles
                .Where(r => userRoleIds.Contains(r.Id))
                .Select(r => r.Name ?? string.Empty)
                .ToListAsync();

            return roles;
        }

        private async Task<bool> AddUserToRoleByNameAsync(int userId, string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return false;

            await _context.UserRoles.AddAsync(new IdentityUserRole<int>
            {
                UserId = userId,
                RoleId = role.Id
            });

            return await SaveChangesAsync() > 0;
        }

        #endregion

        #region Cart Logic

        public async Task<CartResponseDto?> AddOrUpdateCartItemAsync(int productId, int change)
        {
            var cartItem = await _context.Carts
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.ProductId == productId);

            if (cartItem == null)
            {
                if (change <= 0) change = 1;

                cartItem = new Cart
                {
                    ProductId = productId,
                    Quantity = change
                };

                await _context.Carts.AddAsync(cartItem);
            }
            else
            {
                cartItem.Quantity += change;

                if (cartItem.Quantity <= 0)
                {
                    _context.Carts.Remove(cartItem);
                    await SaveChangesAsync();
                    return null;
                }

                _context.Carts.Update(cartItem);
            }

            await SaveChangesAsync();

            var updatedItem = await _context.Carts
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == cartItem.Id);

            return _mapper.Map<CartResponseDto>(updatedItem);
        }

        public async Task<decimal> GetCartTotalAsync()
        {
            return await _context.Carts
                .Include(c => c.Product)
                .SumAsync(item => item.Quantity * (item.Product != null ? item.Product.Price : 0));
        }

        #endregion
    }
}