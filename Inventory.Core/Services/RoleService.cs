using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Users.ApplicationRoles;
using Inventory.Core.Entities.Users.ApplicationUsers;
using Inventory.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Core.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;

        public RoleService(RoleManager<ApplicationRole> roleManager, IMapper mapper)
        {
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<RoleResponseDto>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return _mapper.Map<IReadOnlyList<RoleResponseDto>>(roles);
        }

        public async Task<RoleResponseDto?> GetRoleByIdAsync(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            return role == null ? null : _mapper.Map<RoleResponseDto>(role);
        }

        public async Task<RoleResponseDto> CreateRoleAsync(RoleRequestDto model)
        {
            var appRole = _mapper.Map<ApplicationRole>(model);
            var result = await _roleManager.CreateAsync(appRole);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create role: {errors}");
            }

            return _mapper.Map<RoleResponseDto>(appRole);
        }

        public async Task<bool> UpdateRoleAsync(int id, RoleRequestDto model)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {id} was not found.");

            _mapper.Map(model, role);

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update role: {errors}");
            }

            return true;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {id} was not found.");

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to delete role: {errors}");
            }

            return true;
        }
    }
}