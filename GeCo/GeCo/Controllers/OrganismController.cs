﻿using GeCo.Data.Abstract;
using GeCo.Model;
using GeCo.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GeCo.Data;

[Route("api/[controller]")]
public class OrganismController : Controller
{
    private IOrganismRepository _organismRepository;
    private ITraitRepository _traitRepository;
    private IAlleleRepository _alleleRepository;
    private IGenotypeRepository _genotypeRepository;
    private IPhenotypeRepository _phenotypeRepository;
    private IInheritanceRepository _inheritanceRepository;
    private GeCoDbContext _context;
    public OrganismController(GeCoDbContext context, IOrganismRepository organismRepository, IInheritanceRepository inheritanceRepository, ITraitRepository traitRepository, IAlleleRepository alleleRepository, IGenotypeRepository genotypeRepository, IPhenotypeRepository phenotypeRepository)
    {
        _organismRepository = organismRepository;
        _traitRepository = traitRepository;
        _alleleRepository = alleleRepository;
        _genotypeRepository = genotypeRepository;
        _phenotypeRepository = phenotypeRepository;
        _inheritanceRepository = inheritanceRepository;
        _context = context;

    }

    [HttpGet("GetAll")]
    public IActionResult Get()
    {

        IEnumerable<Organism> _organism = _organismRepository.GetAll();

        if (_organism != null)
        {
            return new OkObjectResult(_organism);
        }
        else
        {
            return NotFound();
        }
    }

    [HttpGet("Get/{id}")]
    public IActionResult Get(int id)
    {
        Organism _organism = _context.Organisms.Where(o => o.Id == id).Include(g => g.Traits).SingleOrDefault();
        IEnumerable<Trait> _trait = _context.Traits.Include(t => t.Organism).Where(t => t.Organism.Id == _organism.Id).Include(t => t.Inheritance).ToList();

        IEnumerable<Phenotype> _phenotypes;
        IEnumerable<Genotype> _genotypes;

        List<TraitView> TraitView = new List<TraitView>();
        List<CharacteristicsView> CharacteristicsView = new List<CharacteristicsView>();

        foreach (Trait trait in _trait)
        {
            // _genotypes = _genotypeRepository.GetAll().ToList();
            _genotypes = _context.Genotypes.Include(g => g.Phenotype).ToList();
            foreach (Genotype genotype in _genotypes)
            {
                _phenotypes = _phenotypeRepository.FindBy(tr => trait.Id == tr.Trait.Id).ToList();
                foreach (Phenotype phenotype in _phenotypes)
                {
                    if (genotype.Phenotype.Id == phenotype.Id)
                    {
                        Allele FirstAllele = _alleleRepository.GetSingle(genotype.FirstAlleleId);
                        Allele SecondAllele = _alleleRepository.GetSingle(genotype.SecondAlleleId);
                        TraitView.Add(new TraitView(FirstAllele.Symbol + SecondAllele.Symbol, phenotype.Name, FirstAllele.Symbol.Equals(SecondAllele.Symbol) ? "homozigot" : "heterozigot"));
                    }
                }
            }
            CharacteristicsView.Add(new CharacteristicsView(trait.Name, TraitView, trait.Inheritance.Name));
        }

        OrganismView OrganismView = new OrganismView(_organism.Name, CharacteristicsView);

        if (OrganismView != null)
        {
            return new OkObjectResult(OrganismView);
        }
        else
        {
            return NotFound();
        }
    }

    [HttpGet("Name={name}")]
    public IActionResult Get(string name)
    {
        IEnumerable<Organism> _organism = _organismRepository
            .FindBy(s => s.Name == name);

        if (_organism != null)
        {
            return new OkObjectResult(_organism);
        }
        else
        {
            return NotFound();
        }
    }
}