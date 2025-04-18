using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ScreenSound.Banco;

public class DAL<T> where T : class
{
    protected readonly ScreenSoundContext context;

    public DAL(ScreenSoundContext context)
    {
        this.context = context;
    }

    public IEnumerable<T> Listar()
    {
        return context.Set<T>().ToList();
    }
    public void Adicionar(T objeto)
    {

        context.Set<T>().Add(objeto);
        context.SaveChanges();

    }
    public void Atualizar(T objeto)
    {
        context.Set<T>().Update(objeto);
        context.SaveChanges();
    }
    public void Deletar(T objeto)
    {
        context.Set<T>().Remove(objeto);
        context.SaveChanges();

    }
    public T? RecuperarPor(Expression<Func<T, bool>> condição)
    {
        return context.Set<T>().AsNoTracking().FirstOrDefault(condição);
    }
    public IEnumerable<T> ListarPor(Expression<Func<T, bool>> condicao)
    {
        return context.Set<T>().Where(condicao);
    }
    public TResult? RecuperarDTO<TResult>(Expression<Func<T, bool>> condicao, Expression<Func<T, TResult>> seletor)
    {
        return context.Set<T>().AsNoTracking().Where(condicao).Select(seletor).FirstOrDefault();
    }

    public IEnumerable<TResult> ListarDTO<TResult>(Expression<Func<T, bool>> condicao, Expression<Func<T, TResult>> seletor)
    {
        return context.Set<T>().AsNoTracking().Where(condicao).Select(seletor).ToList();
    }

}