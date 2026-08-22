using AutoMapper;
using Inventory.Core.Interfaces;
using Inventory.Data.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

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

        public async Task<IReadOnlyList<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>()
                                                        .AsNoTracking();

            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _context.Set<TEntity>()
                                        .FindAsync(id);
        }

        public async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null) 
        {
            IQueryable<TEntity> query = _context.Set<TEntity>()
                                                        .AsNoTracking();

            if (include != null)
            {
                query = include(query);
            }

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
    }
}