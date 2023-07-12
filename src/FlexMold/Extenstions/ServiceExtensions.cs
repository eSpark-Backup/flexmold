using FlexMold.StartupHelper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.Extenstions
{
    public static class ServiceExtensions
    {
        public static void AddFromFactory<TForm>(this IServiceCollection services) 
            where TForm : class
        {
            services.AddTransient<TForm>();
            services.AddTransient<Func<TForm>>(x=>()=>x.GetService<TForm>()!);
            services.AddSingleton<IAbstarctFactory<TForm>, AbstarctFactory<TForm>>();
        }
    }
}
