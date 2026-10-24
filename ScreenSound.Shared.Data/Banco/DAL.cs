// ScreenSound.Banco/DAL.cs
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ScreenSound.Banco;

/// <summary>
/// Repositório Genérico (DAL) assíncrono que implementa o padrão Unit of Work.
/// (Não chama SaveChanges).
/// </summary>
/// <typeparam name="T">A entidade</typeparam>
public class DAL<T> where T : class
{
    protected readonly ScreenSoundContext context;

    public DAL(ScreenSoundContext context)
    {
        this.context = context;
    }

    // --- Métodos de Leitura (Query) ---

    /// <summary>
    /// Retorna um IQueryable<T> (AsNoTracking) para projeções (ex: AutoMapper.ProjectTo).
    /// </summary>
    public IQueryable<T> ListarComoQueryable()
    {
        return context.Set<T>().AsNoTracking();
    }

    /// <summary>
    /// Recupera uma entidade (somente leitura - AsNoTracking) de forma assíncrona.
    /// </summary>
    public async Task<T?> RecuperarPorAsync(Expression<Func<T, bool>> condicao)
    {
        return await context.Set<T>().AsNoTracking().FirstOrDefaultAsync(condicao);
    }

    /// <summary>
    /// Recupera uma entidade (rastreada - para edição) de forma assíncrona.
    /// </summary>
    public async Task<T?> RecuperarParaEdicaoAsync(Expression<Func<T, bool>> condicao)
    {
        return await context.Set<T>().FirstOrDefaultAsync(condicao);
    }

    /// <summary>
    /// Recupera uma entidade (rastreada) com Includes, para atualização.
    /// </summary>
    public async Task<T?> RecuperarComIncludeAsync(
        Expression<Func<T, bool>> filtro,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = context.Set<T>();

        // Aplica os Includes
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        // Retorna a entidade rastreada
        return await query.FirstOrDefaultAsync(filtro);
    }

    /// <summary>
    /// Recupera um DTO projetado (AsNoTracking) de forma assíncrona.
    /// </summary>
    public async Task<TResult?> RecuperarDTOAsync<TResult>(
        Expression<Func<T, bool>> condicao,
        Expression<Func<T, TResult>> seletor)
    {
        return await context.Set<T>()
            .AsNoTracking()
            .Where(condicao)
            .Select(seletor)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Adiciona um objeto ao DbContext.
    /// </summary>
    public void Adicionar(T objeto)
    {
        context.Set<T>().Add(objeto);
    }

    /// <summary>
    /// Atualiza um objeto no DbContext.
    /// </summary>
    public void Atualizar(T objeto)
    {
        context.Set<T>().Update(objeto);
    }

    /// <summary>
    /// Remove um objeto do DbContext.
    /// </summary>
    public void Deletar(T objeto)
    {
        context.Set<T>().Remove(objeto);
    }
}