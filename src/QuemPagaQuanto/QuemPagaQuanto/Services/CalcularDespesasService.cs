﻿using Microsoft.EntityFrameworkCore;
using QuemPagaQuanto.Database;

namespace QuemPagaQuanto.Services
{
    public class CalculoDespesas
    {
        public double CalculoPerCapta { get; set; }
        public List<ProporcionalMorador> ProporcionalMoradores { get; set; }
        public int NumeroMoradores { get; set; }
    }

    public class ProporcionalMorador
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public double Valor { get; set; }
        public double Renda { get; set; }

    }

    public class CalcularDespesasService
    {
        private readonly AppDbContext db;
        public CalcularDespesasService(AppDbContext dbContext) {
            this.db = dbContext;
        }
        
        public CalculoDespesas CalcularProporcional(int grupoId)
        
        {
            var grupo = db.Grupos.Find(grupoId);
            if (grupo == null) return null;

            var despesas = db.Despesas.Where(d => d.GrupoId == grupoId).ToList();
            var moradores = db.Moradores.Include(m => m.Rendas).Where(m => m.GrupoId == grupoId).ToList();

            double despesasTotalGrupo = 0;

            foreach(var despesa in despesas)
            {
                despesasTotalGrupo += despesa.Valor;
            }

            double calculoPerCapta = despesasTotalGrupo / moradores.Count;

            List<ProporcionalMorador> listaProprocionalMorador = new List<ProporcionalMorador>();

            double rendaTotalGrupo = 0;
            foreach(var morador in moradores)
            {
                rendaTotalGrupo += morador.RendaTotal();
            }

            foreach (var morador in moradores)
            {
                listaProprocionalMorador.Add(new ProporcionalMorador()
                {
                    Id = morador.Id,
                    Nome = morador.Nome,
                    Valor = morador.RendaTotal() / rendaTotalGrupo * despesasTotalGrupo,
                    Renda = morador.RendaTotal()
                });
            }

            return new CalculoDespesas() {
                CalculoPerCapta = calculoPerCapta,
                ProporcionalMoradores = listaProprocionalMorador.OrderByDescending(i => i.Valor).ToList(),
                NumeroMoradores = moradores.Count
            };
        }
    }
}