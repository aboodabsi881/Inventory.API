using Inventory.Core.DTOs;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Inventory.Core.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<IReadOnlyList<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);
        Task<TEntity?> GetByIdAsync(int id);
        Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);
        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task<int> SaveChangesAsync();

        Task<IReadOnlyList<TResponseDto>> GetAllDtoAsync<TResponseDto>(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null) where TResponseDto : class;
        Task<TResponseDto?> GetDtoByIdAsync<TResponseDto>(int id, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null) where TResponseDto : class;
        Task<TResponseDto> CreateFromDtoAsync<TRequestDto, TResponseDto>(TRequestDto dto) where TRequestDto : class where TResponseDto : class;
        Task<TResponseDto> UpdateFromDtoAsync<TRequestDto, TResponseDto>(int id, TRequestDto dto) where TRequestDto : class where TResponseDto : class;
        Task<bool> DeleteAndSaveAsync(int id);
    }
}