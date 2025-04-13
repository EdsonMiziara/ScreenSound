using Microsoft.EntityFrameworkCore;
using ScreenSound.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenSound.Shared.Data.Banco;

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
    public T? RecuperarPor(Func<T, bool> condição)
    {
        return context.Set<T>().AsNoTracking().FirstOrDefault(condição);

    }
    public IEnumerable<T> ListarPor(Func<T, bool> condicao)
    {
        return context.Set<T>().Where(condicao);
    }
    public TResult? RecuperarDTO<TResult>(Func<T, bool> condicao, Func<T, TResult> seletor)
    {
        return context.Set<T>().AsNoTracking().Where(condicao).Select(seletor).FirstOrDefault();
    }

    public IEnumerable<TResult> ListarDTO<TResult>(Func<T, bool> condicao, Func<T, TResult> seletor)
    {
        return context.Set<T>().AsNoTracking().Where(condicao).Select(seletor).ToList();
    }

}
